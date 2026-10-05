#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using LessonRunner.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#endregion

namespace LessonRunner.Core.Execution;

/// <summary>
/// Compiles a lesson step's source in-memory using Roslyn and executes
/// the resulting assembly in an isolated AssemblyLoadContext, capturing
/// all console output. This is the guided-mode runner.
/// </summary>
public class SnippetRunner : ILessonRunner
{
    #region Public Methods
    /// <summary>
    /// Compiles and runs the lesson step source code asynchronously, reporting output and handling interactive input
    /// when requested.
    /// </summary>
    /// <remarks>Compilation and execution are dispatched to thread pool threads. Execution runs in an
    /// isolated load context to avoid assembly accumulation across repeated runs.</remarks>
    /// <param name="step">Contains the source code and target framework used for compilation and execution.</param>
    /// <param name="onOutputLine">Receives each line of standard output produced during execution.</param>
    /// <param name="onInputRequired">Provides input text when execution requests interactive input.</param>
    /// <param name="args">Supplies command-line arguments passed to the executed program.</param>
    /// <param name="ownerHwnd">Specifies the owner window handle used by execution features that require a parent window.</param>
    /// <param name="cancellationToken">Signals cancellation for the asynchronous compile and run operation.</param>
    /// <returns>A task that resolves to an execution result containing success state, diagnostics when compilation fails, and
    /// elapsed time.</returns>
    public async Task<ExecutionResult> RunAsync(
        LessonStep step,
        Action<string>? onOutputLine = null,
        Func<string, string>? onInputRequired = null,
        string[]? args = null,
        IntPtr ownerHwnd = default,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Compilation is CPU-bound; run it on a thread pool thread so the
        // UI stays responsive while Roslyn does its work.
        var compileResult = await Task.Run(
            () => Compile(step.SourceCode, step.TargetFramework),
            cancellationToken);

        if (!compileResult.Success)
        {
            sw.Stop();
            return ExecutionResult.Failed(compileResult.Diagnostics, sw.Elapsed);
        }

        // Execute in its own isolated load context so repeated runs don't
        // accumulate assemblies in the default context.
        return await Task.Run(
            () => Execute(compileResult.Assembly!, step, onOutputLine, onInputRequired, args, ownerHwnd, sw),
            cancellationToken);
    }
    #endregion

    #region Data Types (Compilation)
    //Represents the result of a compilation operation.
    private sealed record CompileResult(bool Success, Assembly? Assembly, string Diagnostics);
    #endregion

    #region Helper Functions (Compilation)
    // Compiles the provided C# source code into an in-memory assembly using Roslyn.
    private static CompileResult Compile(string source, string targetFramework)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Reference the BCL assemblies appropriate for the target framework.
        // For net48 snippets we reference the assemblies that are actually
        // loaded in the current (net10) process -- .NET 5+ ships the full
        // surface area of the .NET Framework API and this works cleanly in
        // practice for the lesson content here. A true cross-compilation
        // targeting the net48 reference packs would require resolving them
        // from the SDK on disk, which is left as a future enhancement.
        // Ensure assemblies that snippets commonly need are loaded into the
        // current process before we snapshot AppDomain. Some are not loaded
        // until first use (e.g. System.Text.Json, Microsoft.CSharp), so
        // snippets that use them would fail to compile without this.
        var assembliesToPreload = new[]
        {
            "Microsoft.CSharp",       // required for all dynamic dispatch
            "System.Text.Json",        // not loaded until first use
            "System.Runtime.Serialization.Primitives",
        };

        foreach (var name in assembliesToPreload)
        {
            try { Assembly.Load(name); }
            catch { /* not available in this runtime -- skip */ }
        }

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            assemblyName: $"LessonSnippet_{Guid.NewGuid():N}",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
        {
            var errors = emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString());
            return new CompileResult(false, null, string.Join(Environment.NewLine, errors));
        }

        ms.Seek(0, SeekOrigin.Begin);
        var context = new CollectibleLoadContext();
        var assembly = context.LoadFromStream(ms);
        return new CompileResult(true, assembly, string.Empty);
    }
    #endregion

    #region Helper Functions (Execution)
    // Executes the compiled assembly, capturing console output and handling interactive input.
    private static ExecutionResult Execute(
        Assembly assembly,
        LessonStep step,
        Action<string>? onOutputLine,
        Func<string, string>? onInputRequired,
        string[]? args,
        IntPtr ownerHwnd,
        Stopwatch sw)
    {
        var output = new StringBuilder();
        var errors = new StringBuilder();

        // Redirect Console.Out so we capture all WriteLine calls made by
        // the snippet, and optionally stream each line to the UI as it arrives.
        var originalOut = Console.Out;
        var originalError = Console.Error;
        var originalIn = Console.In;

        using var writer = new CallbackWriter(line =>
        {
            output.AppendLine(line);
            onOutputLine?.Invoke(line);
        });

        // Replace Console.In with a reader that invokes the UI callback when
        // the snippet calls ReadLine(), so the user can supply input via a dialog.
        // Falls back to an empty string if no callback is provided.
        using var inputReader = new CallbackReader(
            prompt => onInputRequired?.Invoke(prompt) ?? string.Empty);

        try
        {
            Console.SetOut(writer);
            Console.SetError(writer);
            Console.SetIn(inputReader);

            // Pass the owner window handle so snippets that call native
            // dialogs (MessageBox, etc.) can parent them to the runner window.
            if (ownerHwnd != IntPtr.Zero)
                Environment.SetEnvironmentVariable(
                    "LESSON_RUNNER_HWND", ownerHwnd.ToInt64().ToString());

            // Find the entry point. Roslyn names it based on the source,
            // so we search rather than assuming a fixed type name.
            var entryPoint = assembly.EntryPoint
                ?? FindMainMethod(assembly);

            if (entryPoint is null)
            {
                sw.Stop();
                return ExecutionResult.Failed("Could not locate an entry point (Main method) in the compiled snippet.", sw.Elapsed);
            }

            // Invoke Main. It may take string[] args or no parameters.
            var parameters = entryPoint.GetParameters().Length > 0
                ? new object?[] { args ?? [] }
                : null;

            entryPoint.Invoke(null, parameters);
        }
        catch (TargetInvocationException tie)
        {
            var inner = tie.InnerException ?? tie;
            errors.AppendLine($"{inner.GetType().Name}: {inner.Message}");
            errors.AppendLine(inner.StackTrace);
        }
        catch (Exception ex)
        {
            errors.AppendLine($"{ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
            Console.SetIn(originalIn);
            Environment.SetEnvironmentVariable("LESSON_RUNNER_HWND", null);
        }

        sw.Stop();

        return errors.Length > 0
            ? ExecutionResult.Failed(errors.ToString(), sw.Elapsed)
            : ExecutionResult.Succeeded(output.ToString(), sw.Elapsed);
    }

    // Searches the assembly for a static Main method to use as the entry point.
    private static MethodInfo? FindMainMethod(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            #pragma warning disable S3011 // BindingFlags.NonPublic safe in this training sample
            var m = type.GetMethod("Main",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            #pragma warning restore S3011
            if (m is not null) return m;
        }
        return null;
    }
    #endregion

    #region Helper Classes (I/O Redirection)
    /// <summary>
    /// A TextReader that invokes a callback each time ReadLine() is called,
    /// allowing the UI to prompt the user for input when a snippet needs it.
    /// ReadKey() is not supported in this context -- it throws an
    /// InvalidOperationException, which surfaces cleanly in the output pane.
    /// </summary>
    private sealed class CallbackReader(Func<string, string> onReadLine) : TextReader
    {
        public override string? ReadLine()
        {
            // The snippet calls Console.ReadLine() for two reasons:
            // 1. Pause() -- we pass the sentinel as the prompt so the UI
            //    knows to show Continue rather than an input dialog.
            // 2. Genuine user input -- prompt is empty, UI shows a dialog.
            return onReadLine(string.Empty);
        }

        public override int Read() => 0;
        public override int Peek() => -1;
    }

    /// <summary>
    /// A collectible load context so compiled snippet assemblies can be
    /// unloaded once execution is complete, rather than accumulating in
    /// the default context for the lifetime of the process.
    /// </summary>
    private sealed class CollectibleLoadContext : AssemblyLoadContext
    {
        public CollectibleLoadContext() : base(isCollectible: true) { }
    }

    /// <summary>
    /// A TextWriter that forwards each completed line to a callback,
    /// allowing the UI to update incrementally as the snippet runs.
    /// </summary>
    private sealed class CallbackWriter(Action<string> onLine) : TextWriter
    {
        private readonly StringBuilder _buffer = new();

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            if (value == '\n')
            {
                onLine(_buffer.ToString());
                _buffer.Clear();
            }
            else if (value != '\r')
            {
                _buffer.Append(value);
            }
        }

        public override void Write(string? value)
        {
            if (value is null) return;
            foreach (var c in value) Write(c);
        }

        protected override void Dispose(bool disposing)
        {
            // Flush any unterminated final line.
            if (disposing && _buffer.Length > 0)
            {
                onLine(_buffer.ToString());
                _buffer.Clear();
            }
            base.Dispose(disposing);
        }
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
