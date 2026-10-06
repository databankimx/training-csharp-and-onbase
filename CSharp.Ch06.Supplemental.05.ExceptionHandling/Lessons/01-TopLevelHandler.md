---
title: "Unhandled Exception vs. Top-Level Handler"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void ThrowsSomething()
    {
        string s = null;
        Console.WriteLine(s.Length); // NullReferenceException
    }

    private static void Main()
    {
        // Without a handler, the runtime terminates the process on its own terms:
        // stack trace blasted to stderr, no cleanup, exit code set by the runtime.
        // With a handler, YOUR code responds: log it, clean up, exit meaningfully.

        try
        {
            ThrowsSomething();
            Console.WriteLine("This line never runs.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            // In a real program, log ex.StackTrace here
        }
        finally
        {
            // finally runs on EVERY path: normal, exception, or early return.
            // This is the correct place for cleanup.
            Console.WriteLine("Cleanup runs regardless.");
        }

        Console.WriteLine("Program exits cleanly.");
    }
}
```
