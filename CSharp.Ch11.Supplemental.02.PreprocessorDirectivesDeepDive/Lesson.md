# Chapter 11 Supplemental 02: Preprocessor Directives Deep Dive

## What This Is

The main lesson showed `#if DEBUG`/`#else`/`#endif` choosing which code gets compiled. This project covers the rest: defining your own symbols (both file-scoped and project-scoped), `#pragma warning` for silencing specific warnings, caller info attributes, and a few directives that are worth knowing but can't be demonstrated live without breaking the build.

What's being extended here is the full compile-time toolbox. The main lesson showed the most common directive. This project shows the complete set and introduces caller info attributes - which are not preprocessor directives technically, but are the closest C# has to C's `__FILE__` and `__LINE__` macros, and belong in the same mental bucket.

Note: `#define FILE_SCOPED_DEMO` appears at the very top of `Program.cs`, before any `using` directives. That placement is required - a `#define` must appear before any real code token in the file.

---

## How to Write This Program

### Mini-Program 1: #define and Project-Wide Symbols

Clear `Main()` and write:

```csharp
#if FILE_SCOPED_DEMO
Console.WriteLine("FILE_SCOPED_DEMO is defined (via #define at the top of this file only).");
#else
Console.WriteLine("FILE_SCOPED_DEMO is NOT defined.");
#endif

#if TRAINING_BUILD
Console.WriteLine("TRAINING_BUILD is defined (via this project's .csproj, every file sees it).");
#else
Console.WriteLine("TRAINING_BUILD is NOT defined.");
#endif

Console.WriteLine("\nCompare this project's .csproj <DefineConstants> against the #define at the");
Console.WriteLine("top of this file to see exactly where each symbol comes from.");

GenericFunctions.Pause();
```

Run it. Both branches were compiled in because both symbols are genuinely defined - `FILE_SCOPED_DEMO` via `#define` in this file, `TRAINING_BUILD` via `<DefineConstants>` in the `.csproj`.

A `#define` symbol is file-scoped only. `Other.cs` in the same project does not see `FILE_SCOPED_DEMO`. `<DefineConstants>` in the `.csproj` applies across every file in the project, the same way `DEBUG` does in Debug builds.

`#undef` removes a previously defined symbol for the remainder of the file - useful if a project-wide symbol needs to be suppressed for one specific file.

### Mini-Program 2: #region / #endregion

Clear `Main()` and write:

```csharp
#region An Example Nested Region
Console.WriteLine("This line lives inside a #region -- purely for code folding in an editor.");
#endregion

Console.WriteLine("#region/#endregion have ZERO effect on the compiled program.");
Console.WriteLine("Unlike every other directive in this file, they don't decide what compiles,");
Console.WriteLine("they don't affect warnings, they do nothing at runtime.");
Console.WriteLine("They're a readability and navigation aid for whoever's editing the file.");

GenericFunctions.Pause();
```

Run it. Nothing changes about the output based on the region. Regions are an editor feature, not a compiler feature.

### Mini-Program 3: #pragma warning

Clear `Main()` and write:

```csharp
// Without the #pragma below, this line generates CS0219:
// "The variable 'intentionallyUnused' is assigned but its value is never used."
#pragma warning disable CS0219
#pragma warning disable S1481
#pragma warning disable IDE0059
int intentionallyUnused = 42;
#pragma warning restore IDE0059
#pragma warning restore S1481
#pragma warning restore CS0219

Console.WriteLine("Declared an intentionally unused variable inside a #pragma warning disable/restore block.");
Console.WriteLine("It compiles cleanly -- the warning was suppressed for that specific section only.");
Console.WriteLine("\nBest practice: disable as narrowly as possible (one or two lines) and");
Console.WriteLine("restore immediately. Disabling for an entire file or project risks silently");
Console.WriteLine("hiding a genuine future mistake the warning would have caught.");

GenericFunctions.Pause();
```

Run it. The warning doesn't appear in the build output because it was disabled for exactly the lines that needed it and restored immediately after.

`#pragma warning disable CSXXXX` silences a specific warning number - not all warnings, just that one. Multiple warning numbers can be comma-separated on one line. The `restore` brings normal behavior back. A warning silenced project-wide stays silenced for every future occurrence too, including a genuine mistake that same warning would have caught months from now.

### Mini-Program 4: Caller Info Attributes

Clear `Main()` and write:

```csharp
Log("This message shows exactly where it was logged from, automatically.");

GenericFunctions.Pause();
```

Add the `Log` helper - the compiler fills in the attributed parameters automatically at every call site:

```csharp
private static void Log(string message,
    [CallerFilePath]   string filePath   = "",
    [CallerLineNumber] int    lineNumber  = 0,
    [CallerMemberName] string memberName = "")
{
    Console.WriteLine($"[{Path.GetFileName(filePath)}:{lineNumber} in {memberName}()] {message}");
}
```

Run it. The output includes the filename, line number, and calling method - automatically injected by the compiler at the call site. The caller never passes these explicitly, and can't - they're filled in by the compiler, not by caller code.

This is how you implement a logging helper that knows where it was called from without requiring every call site to pass `nameof(...)` by hand. Location info that was hand-typed as a string can go stale when code moves; these attributes can't.

### Mini-Program 5: Worth Knowing, Not Demonstrated Live

Clear `Main()` and write:

```csharp
Console.WriteLine("#warning \"message\"");
Console.WriteLine("  Forces a compiler WARNING at that exact line, every build.");
Console.WriteLine("  Not demonstrated live here because it would emit a warning from");
Console.WriteLine("  this shared training solution every time it's compiled.");

Console.WriteLine("\n#error \"message\"");
Console.WriteLine("  Forces a compile ERROR, stops the build entirely.");
Console.WriteLine("  Useful for: #if !SOME_REQUIRED_SYMBOL");
Console.WriteLine("              #error \"You must define SOME_REQUIRED_SYMBOL to build this.\"");
Console.WriteLine("              #endif");

Console.WriteLine("\n#line 200 \"Other.cs\"");
Console.WriteLine("  Makes the compiler report SUBSEQUENT lines as if they came from");
Console.WriteLine("  line 200 of \"Other.cs\". Used by code generators so errors in");
Console.WriteLine("  GENERATED code point back to the ORIGINAL source that produced it.");

Console.WriteLine("\n#pragma checksum");
Console.WriteLine("  Embeds a checksum for a source file, used by debuggers to verify");
Console.WriteLine("  the source matches what was actually compiled.");

GenericFunctions.Pause();
```

Run it. These are real directives that can't be demonstrated live in a shared training solution without breaking the build or having no runtime-observable effect.

---

## Try It Yourself

Run `UsingCallerInfoAttributes()` and look at the printed output - it names the exact file, line number, and method that called `Log()`, all filled in automatically by the compiler. Then try calling `Log()` from a different method and confirm the reported location changes automatically.

---

## Summary: Which Directive Does What

| Directive | Affects | Runtime observable? |
|---|---|---|
| `#if`/`#elif`/`#else`/`#endif` | What code compiles | No - losing branch is absent from binary |
| `#define` / `#undef` | Symbol visibility (file-scoped) | No |
| `<DefineConstants>` in .csproj | Symbol visibility (project-wide) | No |
| `#region` / `#endregion` | Nothing | No - editor only |
| `#pragma warning disable/restore` | Which warnings appear | No |
| `[CallerFilePath/LineNumber/MemberName]` | Values injected at call site | Yes - real runtime values |
| `#warning` | Emits a build warning | No |
| `#error` | Stops the build | No |
| `#line` | Compiler error reporting | No - tooling only |

---

## Takeaways

- `#define` is file-scoped. `<DefineConstants>` in the `.csproj` applies project-wide. Both feed into `#if`/`#elif`/`#else`/`#endif`.
- `#define` must appear before any real code token (before `using` directives).
- `#undef` removes a previously defined symbol for the remainder of the file.
- `#region`/`#endregion` affect only the editor, never the compiled output.
- `#pragma warning disable CSXXXX` / `restore CSXXXX` suppresses a specific warning number for a specific section. Disable narrowly; restore immediately.
- `[CallerFilePath]`, `[CallerLineNumber]`, `[CallerMemberName]` inject call-site information automatically at compile time. The caller never passes them explicitly.
- `#warning` forces a build warning. `#error` stops the build. Both are useful for enforcing build requirements.
- `#line` and `#pragma checksum` are tooling directives used by code generators.
