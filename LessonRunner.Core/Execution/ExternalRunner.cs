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
using System.Text;
using LessonRunner.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#endregion

namespace LessonRunner.Core.Execution;

/// <summary>
/// Compiles a lesson step's source to a temporary .exe on disk and launches
/// it as a subprocess. Used for steps that require COM interop assemblies,
/// WinForms, or other dependencies not available to in-memory compilation.
/// Stdout and stderr are captured and streamed to the caller via the
/// onOutputLine callback, same as SnippetRunner.
/// </summary>
public class ExternalRunner : ILessonRunner
{
    #region Methods
    /// <summary>
    /// Compiles the lesson step source to a temporary executable, runs it, and returns the execution result.
    /// </summary>
    /// <remarks>Creates files in a temporary directory and attempts to delete that directory after execution
    /// completes.</remarks>
    /// <param name="step">Contains the source code and target framework to compile and execute.</param>
    /// <param name="onOutputLine">Receives each line written to standard output or standard error during execution.</param>
    /// <param name="onInputRequired">Provides input text when the running process requests input.</param>
    /// <param name="args">Specifies command-line arguments passed to the compiled executable.</param>
    /// <param name="ownerHwnd">Specifies the owner window handle used for process interaction scenarios.</param>
    /// <param name="cancellationToken">Signals cancellation for compilation and execution operations.</param>
    /// <returns>A task that resolves to an <see cref="ExecutionResult"/> describing success or failure output and elapsed time.</returns>
    public async Task<ExecutionResult> RunAsync(
        LessonStep step,
        Action<string>? onOutputLine = null,
        Func<string, string>? onInputRequired = null,
        string[]? args = null,
        IntPtr ownerHwnd = default,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Compile to a temp directory
        var tempDir  = Path.Combine(Path.GetTempPath(), $"LessonRunner_{Guid.NewGuid():N}");
        var exePath  = Path.Combine(tempDir, "lesson.exe");
        Directory.CreateDirectory(tempDir);

        try
        {
            var compileError = await Task.Run(
                () => CompileToDisk(step.SourceCode, exePath, step.TargetFramework),
                cancellationToken);

            if (compileError is not null)
            {
                sw.Stop();
                return ExecutionResult.Failed(compileError, sw.Elapsed);
            }

            // Write a minimal app.config so the CLR selects the correct
            // framework version when launching the compiled executable.
            if (step.TargetFramework.StartsWith("net4", StringComparison.OrdinalIgnoreCase))
            {
                var version = step.TargetFramework switch
                {
                    "net48"  => "4.8",
                    "net472" => "4.7.2",
                    "net471" => "4.7.1",
                    "net47"  => "4.7",
                    "net462" => "4.6.2",
                    _        => "4.8"
                };
                var config =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<configuration>\r\n" +
                    "  <startup>\r\n" +
                    $"    <supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v{version}\" />\r\n" +
                    "  </startup>\r\n" +
                    "</configuration>";
                await File.WriteAllTextAsync(exePath + ".config", config, cancellationToken);
            }

            return await RunProcess(exePath, args, ownerHwnd,
                onOutputLine, onInputRequired, sw, cancellationToken);
        }
        finally
        {
            // Clean up the temp directory after the process exits
            try { Directory.Delete(tempDir, recursive: true); }
            catch { /* best effort */ }
        }
    }
    #endregion

    #region Helper Functions (Compilation)
    // Compiles the source code to a temporary executable on disk, returning null on success or an error message on failure.
    private static string? CompileToDisk(string source, string outputPath, string targetFramework)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // For external steps we must reference the correct BCL for the target
        // framework rather than the assemblies loaded in the current net10 process.
        // net48 interop assemblies (e.g. Office) expect mscorlib, not System.Runtime.
        var references = targetFramework.StartsWith("net4", StringComparison.OrdinalIgnoreCase)
            ? GetNet48References()
            : GetCurrentProcessReferences();

        // Add any interop assemblies found in the NuGet cache / GAC
        foreach (var asm in GetInteropAssemblies())
            references.Add(MetadataReference.CreateFromFile(asm));

        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetFileNameWithoutExtension(outputPath),
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        var result = compilation.Emit(outputPath);

        if (result.Success) return null;

        return string.Join(Environment.NewLine,
            result.Diagnostics
                  .Where(d => d.Severity == DiagnosticSeverity.Error)
                  .Select(d => d.ToString()));
    }

    // Returns a list of MetadataReference objects for the assemblies loaded in the current process.
    private static List<MetadataReference> GetCurrentProcessReferences()
    {
        return [.. AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))];
    }

    // Returns a list of MetadataReference objects for the .NET Framework 4.8 reference assemblies.
    private static List<MetadataReference> GetNet48References()
    {
        // Locate the .NET Framework 4.8 reference assemblies from the SDK.
        // These ship with Visual Studio and the .NET Framework targeting pack.
        var candidates = new[]
        {
            // Visual Studio targeting pack location
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.8"),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v4.8"),
        };

        var refPath = candidates.FirstOrDefault(Directory.Exists);

        if (refPath is null)
        {
            // Fallback: use what's in the current process -- likely to have
            // BCL mismatch but better than failing outright with no references.
            return GetCurrentProcessReferences();
        }

        return [.. Directory
            .EnumerateFiles(refPath, "*.dll", SearchOption.TopDirectoryOnly)
            .Where(f =>
            {
                // A handful of files in the net48 reference pack are unmanaged
                // shims that Roslyn cannot load as managed metadata references.
                var name = Path.GetFileNameWithoutExtension(f);
                return !name.EndsWith(".Thunk",   StringComparison.OrdinalIgnoreCase)
                    && !name.EndsWith(".Wrapper", StringComparison.OrdinalIgnoreCase);
            })
            .Select(f => (MetadataReference)MetadataReference.CreateFromFile(f))];
    }

    // Returns paths to interop assemblies that are likely needed for external
    // steps (Office interop, etc.) by scanning known locations.
    private static IEnumerable<string> GetInteropAssemblies()
    {
        var candidates = new List<string>();

        // The Excel interop package lives in the NuGet cache -- check the
        // standard user and machine cache locations.
        var nugetCaches = new[]
        {
            Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile), ".nuget", "packages"),
            Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft SDKs", "NuGetPackages"),
        };

        foreach (var cache in nugetCaches)
        {
            if (!Directory.Exists(cache)) continue;
            candidates.AddRange(
                Directory.EnumerateFiles(cache,
                    "Microsoft.Office.Interop.Excel.dll",
                    SearchOption.AllDirectories));
            candidates.AddRange(
                Directory.EnumerateFiles(cache,
                    "office.dll",
                    SearchOption.AllDirectories));
        }

        // GAC fallback
        var gacPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.Windows), "assembly"),
        };

        foreach (var gac in gacPaths)
        {
            if (!Directory.Exists(gac)) continue;
            candidates.AddRange(
                Directory.EnumerateFiles(gac,
                    "Microsoft.Office.Interop.*.dll",
                    SearchOption.AllDirectories));
        }

        return candidates.Distinct(StringComparer.OrdinalIgnoreCase);
    }
    #endregion

    #region Helper Functions (Process Execution)
    // Runs the compiled executable as a subprocess, capturing stdout and stderr, and returning an ExecutionResult.
    private static async Task<ExecutionResult> RunProcess(
        string exePath,
        string[]? args,
        IntPtr ownerHwnd,
        Action<string>? onOutputLine,
        Func<string, string>? onInputRequired,
        Stopwatch sw,
        CancellationToken cancellationToken)
    {
        var errors = new StringBuilder();

        var psi = new ProcessStartInfo
        {
            FileName               = exePath,
            Arguments              = args is { Length: > 0 }
                                     ? string.Join(" ", args.Select(a => $"\"{a}\""))
                                     : string.Empty,
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            RedirectStandardInput  = true,
            CreateNoWindow         = true,
        };

        // Pass the owner HWND so native dialogs launched by the subprocess
        // can parent themselves to the runner window.
        if (ownerHwnd != IntPtr.Zero)
            psi.Environment["LESSON_RUNNER_HWND"] = ownerHwnd.ToInt64().ToString();

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        var outputDone = new TaskCompletionSource<bool>();
        var errorDone  = new TaskCompletionSource<bool>();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null) { outputDone.TrySetResult(true); return; }
            onOutputLine?.Invoke(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) { errorDone.TrySetResult(true); return; }
            errors.AppendLine(e.Data);
            onOutputLine?.Invoke(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Handle ReadLine calls from the subprocess via stdin
        _ = Task.Run(async () =>
        {
            try
            {
                while (!process.HasExited && !cancellationToken.IsCancellationRequested)
                {
                    // The subprocess will block on Console.ReadLine()
                    // we can't detect that from outside, so just keep the
                    // stdin pipe open and let onInputRequired handle it
                    // when the user interacts.
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (OperationCanceledException) { /* Ignore cancellation */ }
        }, cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        await outputDone.Task;
        await errorDone.Task;

        sw.Stop();

        bool success = process.ExitCode == 0 && errors.Length == 0;
        return success
            ? ExecutionResult.Succeeded(string.Empty, sw.Elapsed)
            : ExecutionResult.Failed(errors.ToString(), sw.Elapsed);
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
