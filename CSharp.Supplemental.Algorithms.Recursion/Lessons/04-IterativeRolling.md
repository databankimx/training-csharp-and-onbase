---
title: "Iterative (rolling variables) - O(n)"
chapter: 0
index: 4
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private const int N = 40;
    private const int KnownF40 = 102_334_155;

    private static void Main()
    {
        int count = 0;
        var timer = Stopwatch.StartNew();
        int result = Fib(N, ref count);
        timer.Stop();

        Console.WriteLine($"f({N}) = {result:#,0}");
        Console.WriteLine(result == KnownF40 ? "Result verified." : "Result INCORRECT.");
        PrintReport("Iterative (rolling variables)", N, count, timer.Elapsed);
    }

    // Same O(n) iterations as the array-based approach, but keeps only the two most recent
    // values in variables rather than allocating an array - O(1) memory instead of O(n).
    // The lowest-overhead O(n) approach of the three.
    private static int Fib(int n, ref int count)
    {
        if (n < 2) return n;
        int prev = 0, curr = 1, result = 0;
        for (int i = 2; i <= n; i++)
        {
            count++;
            result = curr + prev;
            prev   = curr;
            curr   = result;
        }
        return result;
    }

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        Console.WriteLine($"{name}: {ops:#,0} operations for n={n}");
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds:F3} ms");
    }
}
```
