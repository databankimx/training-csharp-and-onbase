---
title: "Recursive (naive) - O(2^n)"
chapter: 0
index: 1
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
        PrintReport("Recursive (naive)", N, count, timer.Elapsed);
    }

    // f(n) = f(n-1) + f(n-2), computed by direct recursion with no memoization.
    // Each call spawns two more all the way down - exponential work, O(2^n).
    // Expect several seconds at n=40.
    private static int Fib(int n, ref int count)
    {
        count++;
        if (n < 2) return n;
        return Fib(n - 1, ref count) + Fib(n - 2, ref count);
    }

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        Console.WriteLine($"{name}: {ops:#,0} operations for n={n}");
        Console.WriteLine($"Elapsed: {elapsed.TotalSeconds:F3} s");
    }
}
```
