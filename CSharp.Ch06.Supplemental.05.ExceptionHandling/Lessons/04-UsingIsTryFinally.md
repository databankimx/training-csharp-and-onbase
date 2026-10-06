---
title: "using Is try/finally - What the Compiler Generates"
chapter: 6
index: 4
dependencies: []
---

```csharp
using System;

internal class DisposableClass : IDisposable
{
    public string Name { get; set; } = "";
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Console.WriteLine($"{Name}: Dispose() called");
    }
}

internal static class Program
{
    private static void Main()
    {
        // Path 1: using block -- the compiler expands this to the try/finally below
        using (var fred = new DisposableClass { Name = "Fred" })
        {
            Console.WriteLine($"{fred.Name}: inside using block");
        } // Dispose() called here, even if an exception is thrown inside

        Console.WriteLine();

        // Path 2: the exact try/finally the compiler generates from a using block
        DisposableClass lamont = null;
        try
        {
            lamont = new DisposableClass { Name = "Lamont" };
            Console.WriteLine($"{lamont.Name}: inside try block");
        }
        finally
        {
            // Null check is necessary: if the constructor threw, lamont is still null
            lamont?.Dispose();
        }

        // Prefer using -- it handles the null check and is immediately recognizable.
        // Write the try/finally by hand only when the resource lifetime doesn't fit a block.
    }
}
```
