---
title: "Debug.Assert vs. Trace.Assert"
chapter: 6
index: 3
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private static void Main()
    {
        // Debug.Assert  -- [Conditional("DEBUG")]  -- compiled out of Release builds
        // Trace.Assert  -- [Conditional("TRACE")]  -- active in Debug AND Release by default
        //
        // Both behave identically when they fire. The difference is purely when they're compiled in.

        Console.WriteLine("Debug.Assert: active in Debug builds only.");
        Debug.Assert(1 + 1 == 2, "Basic arithmetic works.");   // passes silently
        Console.WriteLine("...passed.\n");

        Console.WriteLine("Trace.Assert: active in Debug AND Release.");
        Trace.Assert(1 + 1 == 2, "Basic arithmetic works.");   // passes silently
        Console.WriteLine("...passed.\n");

        // The [Conditional] trap -- applies to BOTH:
        // The entire call site is removed, including the arguments.
        // Anything with side effects inside an assertion disappears in Release:
        //
        //   Debug.Assert(TryInitialize());      // TryInitialize() never runs in Release
        //   Debug.Assert(list.Remove(item));    // the removal never happens in Release
        //
        // Rule: an assertion must OBSERVE, never DO.
        //
        // Use Trace.Assert when you want a check that survives into a shipped Release build.
        // Use Debug.Assert for development-time aids -- cheap to sprinkle, free in production.
        Console.WriteLine("Rule: assertions must observe, never do.");
        Console.WriteLine("      Debug for development aids; Trace to survive into Release.");
    }
}
```
