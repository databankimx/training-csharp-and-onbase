---
title: "Wrapping Exceptions - Always Pass the Inner"
chapter: 6
index: 2
dependencies: []
---

```csharp
using System;

internal class TrainingException : Exception
{
    public TrainingException(string message) : base(message) { }
    public TrainingException(string message, Exception inner) : base(message, inner) { }
}

internal static class Program
{
    private static void Initialize()
    {
        try
        {
            // Simulate a configuration failure
            throw new InvalidOperationException("Config file missing.");
        }
        catch (Exception ex)
        {
            // CORRECT: pass ex as the inner exception -- the original is preserved
            throw new TrainingException("Error initializing!", ex);

            // WRONG (don't run this):
            // throw new TrainingException("Error initializing!");
            // ^ The original stack trace, message, and type are gone forever.
        }
    }

    private static void Main()
    {
        try
        {
            Initialize();
        }
        catch (TrainingException ex)
        {
            Console.WriteLine($"Outer: {ex.Message}");
            Console.WriteLine($"Inner: {ex.InnerException?.GetType().Name} - {ex.InnerException?.Message}");

            // throw;      re-throws preserving the original stack trace
            // throw ex;   re-throws but RESETS the stack trace to this line -- avoid
        }
    }
}
```
