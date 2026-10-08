---
title: "Assertions, Preprocessor Directives, Debug and Trace"
chapter: 11
index: 2
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private static void Main()
    {
        // --- Debug.Assert: catches programmer errors, not user input errors ---
        // Compiled out entirely in Release builds. Never put required logic inside one.
        // A failing assert shows an interactive dialog by default -- know this before
        // adding one to code that might run unattended.
        int quantity   = 5;
        decimal price  = 9.99m;
        decimal total  = quantity * price;
        Debug.Assert(total == quantity * price, "Total calculation is inconsistent!");
        Console.WriteLine($"Debug.Assert passed: {total:C} == {quantity} * {price:C}");
        Console.WriteLine("(In a Release build, the Assert line above is absent from the binary entirely.)");

        // --- Preprocessor directives ---
        // #if/#endif decides at COMPILE TIME which branch even exists in the binary.
        // The other branch is not dead code that never runs -- it was never compiled.
        // The runner executes in a context where DEBUG is not defined, so the #else
        // branch compiles in -- which is itself the demonstration.
#if DEBUG
        Console.WriteLine("\n#if DEBUG: this build defines DEBUG -- this branch compiled in.");
#else
        Console.WriteLine("\n#if DEBUG: DEBUG is not defined -- the DEBUG branch above was never compiled.");
#endif

        // --- Debug.WriteLine vs Trace.WriteLine ---
        // Debug.WriteLine: compiled out in Release; in Debug, goes to the debugger's
        //   Output window only -- not the console. Nothing appears from the line below.
        Debug.WriteLine("Debug.WriteLine: only visible in a debugger's Output window (Debug builds only).");

        // Trace.WriteLine: always compiles in. By default goes to Trace.Listeners,
        //   which in a console app starts empty. Adding a listener makes it visible.
        Trace.Listeners.Add(new ConsoleTraceListener());
        Trace.WriteLine("Trace.WriteLine: visible now that a ConsoleTraceListener is registered.");

        Console.WriteLine("\nThe Debug.WriteLine() above did not print here -- by design.");
        Console.WriteLine("The Trace.WriteLine() did, because a listener was explicitly added.");
        Console.WriteLine("See Supplemental.03 for the full listener story.");

        Trace.Listeners.Clear();
    }
}
```
