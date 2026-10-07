---
title: "StringBuilder vs String Concatenation"
chapter: 0
index: 2
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        const int iterations = 100_000;

        // String concatenation: each += allocates a new string, copies everything already
        // accumulated into it, then throws the old one away.
        // Total allocations grow quadratically - O(n^2) in bytes allocated.
        var sw = Stopwatch.StartNew();
        string concatenated = "";
        for (int i = 0; i < iterations; i++)
            concatenated += "x";
        sw.Stop();

        Console.WriteLine($"String concatenation ({iterations:N0} iterations): {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Result length: {concatenated.Length:N0} characters");
        Console.WriteLine();

        // StringBuilder: writes into an existing buffer. The buffer grows (doubles) only
        // when it fills up. Pre-sizing eliminates even those internal reallocations.
        // ToString() is called once at the end.
        sw.Restart();
        var sb = new StringBuilder(capacity: iterations);
        for (int i = 0; i < iterations; i++)
            sb.Append('x');
        string built = sb.ToString();
        sw.Stop();

        Console.WriteLine($"StringBuilder ({iterations:N0} appends): {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Result length: {built.Length:N0} characters");
        Console.WriteLine();
        Console.WriteLine("Both results are identical - the difference is only in how the work was done.");
        Console.WriteLine();
        Console.WriteLine("Guidelines:");
        Console.WriteLine("  Use + freely for small, bounded concatenations (a few fixed parts).");
        Console.WriteLine("  Use StringBuilder when concatenation happens in a loop or on runtime data.");
        Console.WriteLine("  Use string.Join when assembling a collection with a constant separator.");
    }
}
```
