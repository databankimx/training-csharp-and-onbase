# LessonRunner.Core

Platform-agnostic library that powers the LessonRunner. Contains the data models, the step file parser, and the two runner implementations. `LessonRunner.Wpf` references this project; nothing here depends on WPF or any UI technology.

Targets **net10.0** (not net48) because it needs Roslyn (`Microsoft.CodeAnalysis.CSharp`) and hosts the runner process itself. The lesson *steps* it compiles are written as net48 code - see the compiler mismatch section below.

---

## Lesson file format

Each lesson step is a single `.md` file inside a `Lessons/` subdirectory of a chapter project. The filename controls sort order: numeric prefixes (`01-`, `02-`, ...) guarantee the steps appear in the right sequence regardless of filesystem ordering.

### Structure

```
---
title: "Display Name Shown in the Step List"
chapter: 9
index: 1
dependencies: []
---

```csharp
// The complete, compilable C# source for this step.
// Must be a single top-level file - no partial classes across files.
internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Hello.");
    }
}
```
```

### YAML frontmatter fields

| Field | Type | Required | Description |
|---|---|---|---|
| `title` | string | yes | Display name in the step list |
| `chapter` | int | yes | Chapter number (informational) |
| `index` | int | yes | Sort position within the chapter |
| `dependencies` | string list | no | Other step filenames whose source is merged into compilation (see below) |
| `cumulative` | bool | no | Whether steps are additive checkpoints (Ch01 pattern) vs. isolated snippets |
| `defaultArgs` | string | no | Pre-filled content for the args bar when this step is selected |
| `launchMode` | string | no | `external` to compile to disk and launch as a subprocess; omit for in-process |
| `targetFramework` | string | no | Roslyn compilation target; defaults to `net48` |

### Dependencies

When a step uses a type defined in a previous step (e.g. an enum declared in step 2 that step 4 references), list the earlier step's filename in `dependencies`:

```yaml
dependencies: ["02-Enums.md", "03-Structs.md"]
```

`LessonStepParser` resolves these filenames relative to the same `Lessons/` directory and merges their source code with the current step before Roslyn sees it. The dependency source is prepended to the step source, so type declarations are in scope.

### The code block

`LessonStepParser` uses Markdig to parse the file into an AST and locates the first ` ```csharp ` fenced code block. It does not regex the raw text. This matters because code blocks may contain triple-backtick sequences in string literals or comments - the AST handles that correctly where a regex would not.

The extracted source must be a **single, complete, compilable C# file** with its own `Main()` method. The runner does not assemble fragments; it compiles what it finds verbatim.

---

## Parsing (`LessonStepParser`)

`ParseDirectory(lessonsDirectory)` enumerates `*.md` files, sorts them by filename (ordinal), and calls `ParseFile` on each. Files that cannot be parsed (missing code block, malformed frontmatter) are silently skipped - `ParseFile` returns `null` and the caller filters with `OfType<LessonStep>()`.

Frontmatter is extracted by a compiled `Regex` matching `--- ... ---` at the start of the file. The YAML is parsed with a simple line-split + colon-split approach, not a full YAML library - it handles the small set of types actually used (string, int, bool, string list) and nothing else. If you need a new frontmatter field type, add a `GetXxx` helper in `LessonStepParser`.

---

## Models

### `LessonStep`

An immutable record (init-only properties) produced by the parser. Notable properties:

- **`SourceCode`** - the compilable C# extracted from the code block, with dependencies prepended if any
- **`SourceFile`** - absolute path to the originating `.md` file (used by the WPF layer to locate related files)
- **`LaunchMode`** - `InProcess` (default) or `External`, controls which runner is used
- **`TargetFramework`** - passed to Roslyn as the compilation target; also used by `ExternalRunner` to find the correct reference assemblies

### `ExecutionResult`

Simple record returned by both runners. `Success` is `false` on either compile failure or runtime exception. `Error` is non-empty in that case and contains either Roslyn diagnostic strings or the exception message and stack trace. `Output` holds everything the snippet wrote to `Console.Out` and `Console.Error`.

---

## Runners

Both runners implement `ILessonRunner` and expose a single `RunAsync` method with the same signature. The WPF layer picks one based on `step.LaunchMode`.

```csharp
Task<ExecutionResult> RunAsync(
    LessonStep step,
    Action<string>? onOutputLine = null,
    Func<string, string>? onInputRequired = null,
    string[]? args = null,
    IntPtr ownerHwnd = default,
    CancellationToken cancellationToken = default);
```

- **`onOutputLine`** - called for each line of output as it arrives, so the UI can stream rather than wait for completion
- **`onInputRequired`** - called when the snippet calls `Console.ReadLine()`; the callback blocks until the user provides input (via the `ConsoleInputDialog` in the WPF layer) and returns the entered string
- **`ownerHwnd`** - passed through to the execution environment via `LESSON_RUNNER_HWND` so snippets that open native dialogs can parent them correctly

### SnippetRunner (in-process)

The default path. Compiles the source in memory using Roslyn and executes it in the same process, inside a `CollectibleAssemblyLoadContext` so the compiled assembly can be unloaded afterward.

**The compiler/runtime mismatch**

The runner process targets net10. The step source is written as net48 code. Roslyn compiles against whichever assemblies are currently loaded in the net10 process - not the net48 reference packs. This works in practice because .NET 5+ ships the full .NET Framework API surface, but it creates two categories of problem:

1. **Assemblies not yet loaded** - Roslyn only sees assemblies that are actually in `AppDomain.CurrentDomain.GetAssemblies()` at compile time. If an assembly hasn't been loaded yet (because no net10 code in the runner has needed it), its types are invisible to Roslyn even though they'd be available at runtime. The `Compile()` method works around this with an explicit preload list:

```csharp
var assembliesToPreload = new[]
{
    "Microsoft.CSharp",
    "System.Text.Json",
    "System.Runtime.Serialization.Primitives",
    "System.Xml.Linq",
    "System.Xml.XDocument",
    "System.Private.Xml.Linq",
    "System.Diagnostics.TextWriterTraceListener",
    "System.Diagnostics.EventLog",
    "System.Diagnostics.PerformanceCounter",
    "System.Security.Cryptography.X509Certificates",
    "System.Security.Cryptography.Algorithms",
};
```

2. **Type-forwarding** - On net10, many types are forwarded from a facade assembly (e.g. `System.Xml.Linq`) to the actual implementation assembly (e.g. `System.Private.Xml.Linq`). Loading the facade alone leaves Roslyn with a reference to a stub that doesn't define any types. The fix is to force-instantiate one type from each affected namespace, which causes the runtime to load the implementation assembly into the process:

```csharp
try { _ = new System.Xml.Linq.XElement("_"); } catch { }
ForceLoad("System.Diagnostics.EventLog, System.Diagnostics.EventLog");
// etc.
```

`ForceLoad` uses `Type.GetType` (which resolves forwarding) followed by `RuntimeHelpers.RunClassConstructor` to ensure the type's implementation assembly is loaded.

**If a step fails to compile with CS0246 or CS1069**, the assembly hosting that type probably isn't preloaded. Add it to the preload list, and add a `ForceLoad` call if the error message says the type has been forwarded to another assembly.

**I/O redirection**

`Console.Out`, `Console.Error`, and `Console.In` are replaced before invoking the snippet's `Main()` and restored in a `finally` block:

- `CallbackWriter` wraps `Console.Out`/`Error` and forwards each completed line to `onOutputLine`
- `CallbackReader` wraps `Console.In` and calls `onInputRequired` each time the snippet calls `Console.ReadLine()`

The WPF layer uses two sentinel strings to distinguish pause requests from genuine input requests:

- `##LESSON_PAUSE##` - the snippet called `GenericFunctions.Pause()`, which writes this sentinel; the UI shows the Continue button instead of an input dialog
- `##LESSON_CLEAR##` - the snippet called a clear helper; the UI clears the output pane

**Pause / Continue mechanics**

When `GenericFunctions.Pause()` is called, it calls `Console.ReadLine()` internally. The `CallbackReader` sees this and invokes `onInputRequired`. The WPF layer's `onInputRequired` handler checks whether `_pausePending` is true. If it is, it shows the Continue button and blocks the runner thread on a `ManualResetEventSlim` until the user clicks Continue. This keeps the snippet paused mid-execution without holding the UI thread.

### ExternalRunner

Used for steps with `launchMode: external` in their frontmatter. Compiles the source to a temporary `.exe` on disk, writes a minimal `app.config` to select the correct .NET Framework version, and launches the executable as a subprocess.

Uses the **net48 reference assemblies from the SDK** (found under `%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8`) rather than the assemblies loaded in the runner process. This gives accurate net48 compilation without the forwarding issues that affect `SnippetRunner`. Falls back to the current process assemblies if the reference pack isn't installed.

Also scans the NuGet cache and GAC for interop assemblies (`Microsoft.Office.Interop.Excel.dll`, etc.) and adds any it finds to the Roslyn reference set.

Output is streamed via `Process.OutputDataReceived` and `ErrorDataReceived`. The temp directory is deleted in a `finally` block after the process exits.

---

## Adding a new chapter's steps

1. Create a `Lessons/` directory inside the chapter project folder.
2. Add one `.md` file per step, named with a numeric prefix: `01-TopicName.md`, `02-NextTopic.md`, etc.
3. Each file needs YAML frontmatter and a single ` ```csharp ` code block.
4. The source in the code block must be a complete, compilable C# file with its own `Main()`.
5. Run the LessonRunner and select the chapter - steps appear automatically.

## Adding a new frontmatter field

1. Add an `init`-only property to `LessonStep`.
2. Add a `GetXxx` helper to `LessonStepParser` if needed (the existing `GetString`, `GetInt`, `GetBool`, `GetStringList` cover most cases).
3. Wire the new property in `ParseFile`.
