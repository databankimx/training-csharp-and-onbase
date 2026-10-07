---
title: "Preprocessor Directives and Caller Info Attributes"
chapter: 11
index: 1
dependencies: []
---

```csharp
// #define must appear before any real code token -- before using directives,
// before namespace declarations. Comments and other directives are fine before it.
// This symbol is FILE-SCOPED: only this file sees it.
// Compare with TRAINING_BUILD (in the project's .csproj <DefineConstants>),
// which every file in the project sees.
// In the LessonRunner, neither FILE_SCOPED_DEMO nor TRAINING_BUILD is defined,
// so both #if blocks below take the #else branch -- which itself demonstrates the point.
#define FILE_SCOPED_DEMO

using System;
using System.IO;
using System.Runtime.CompilerServices;

internal static class Program
{
    private static void Main()
    {
        // --- #define (file-scoped) vs project-wide DefineConstants ---
#if FILE_SCOPED_DEMO
        Console.WriteLine("FILE_SCOPED_DEMO: defined (via #define at the top of this file).");
#else
        Console.WriteLine("FILE_SCOPED_DEMO: NOT defined -- the #define above was not seen by this compilation unit.");
#endif

#if TRAINING_BUILD
        Console.WriteLine("TRAINING_BUILD: defined (via <DefineConstants> in the .csproj).");
#else
        Console.WriteLine("TRAINING_BUILD: NOT defined -- no .csproj DefineConstants in this compilation.");
#endif

        // In the VS project, both symbols are defined and both #if branches compile in.
        // In the LessonRunner, neither is -- both #else branches compile in instead.
        // The ABSENT branch is absent from the binary, not dead code that never runs.
        Console.WriteLine();

        // --- #region / #endregion ---
        // Zero effect on compiled output. Editor folding only.
        // Unlike every other directive here, they don't decide what compiles,
        // don't affect warnings, do nothing at runtime.
        #region An example region
        Console.WriteLine("#region/#endregion: purely an editor folding aid -- no effect on the compiled program.");
        #endregion

        // --- #pragma warning disable/restore ---
        // Silences a SPECIFIC warning number for a SPECIFIC section of code.
        // Disable narrowly; restore immediately. A project-wide disable silences
        // future genuine mistakes too.
#pragma warning disable CS0219
#pragma warning disable IDE0059
        int intentionallyUnused = 42;
#pragma warning restore IDE0059
#pragma warning restore CS0219
        Console.WriteLine("Declared an intentionally unused variable inside a disable/restore block -- no CS0219 warning.");

        // --- Caller info attributes ---
        // The compiler fills in filePath/lineNumber/memberName at every call site.
        // The caller never passes them explicitly and can't -- the compiler owns them.
        // Location info that was hand-typed as a string goes stale when code moves;
        // these attributes can't.
        Log("Caller info attributes inject location automatically at compile time.");

        // --- Worth knowing, not demonstrated live ---
        Console.WriteLine("\nNot demonstrated live (would break the build or have no runtime effect):");
        Console.WriteLine("  #warning \"msg\"  -- forces a compiler warning at that exact line");
        Console.WriteLine("  #error \"msg\"    -- forces a compile error, stops the build");
        Console.WriteLine("  #line 200 \"Other.cs\" -- makes compiler report subsequent lines as from a different file");
        Console.WriteLine("  #pragma checksum    -- embeds a source file checksum for debugger verification");
    }

    private static void Log(string message,
        [CallerFilePath]   string filePath   = "",
        [CallerLineNumber] int    lineNumber  = 0,
        [CallerMemberName] string memberName = "")
    {
        Console.WriteLine($"[{Path.GetFileName(filePath)}:{lineNumber} in {memberName}()] {message}");
    }
}
```
