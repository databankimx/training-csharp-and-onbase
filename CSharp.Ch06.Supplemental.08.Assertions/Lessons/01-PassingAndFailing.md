---
title: "Passing and Failing Assertions Side by Side"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Linq;

internal static class Program
{
    private static void Main()
    {
        int[] scores = [72, 88, 95, 61, 84, 91, 78, 55, 99, 73];
        const int maxPossibleScore = 100;

        // A passing assertion is completely invisible at runtime
        Console.WriteLine("Checking that no score exceeds the maximum...");
        Debug.Assert(scores.Max() <= maxPossibleScore,
            $"Found a score above {maxPossibleScore}!");
        Console.WriteLine("...passed silently, as expected.\n");

        // A failing assertion stops execution and (outside a debugger) shows a dialog.
        // Running in the LessonRunner: the DefaultTraceListener writes to the output
        // and execution continues -- the dialog only appears in a standalone process.
        Console.WriteLine("Checking that scores has more than 10 entries (it doesn't)...");
        Debug.Assert(scores.Length > 10,
            $"Expected more than 10 scores, but found {scores.Length}.");
        Console.WriteLine("...execution resumed after the assertion.\n");

        // Key points:
        // - A failing assertion interrupts but does NOT unwind the stack.
        //   Execution resumes from the same point on Ignore.
        // - Always supply a message. Include the actual value alongside the expectation.
        // - Debug.Assert is [Conditional("DEBUG")] -- compiled out of Release entirely.
        Console.WriteLine("Debug.Assert: development aid, not a production guard.");
    }
}
```
