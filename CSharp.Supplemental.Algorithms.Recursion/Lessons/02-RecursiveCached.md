---
title: "Recursive, Cached - O(n)"
chapter: 0
index: 2
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;

internal static class Program
{
    private const int N = 40;
    private const int KnownF40 = 102_334_155;

    private static void Main()
    {
        int count = 0;
        var timer = Stopwatch.StartNew();
        var cache = new Dictionary<int, int> { { 0, 0 }, { 1, 1 } };
        int result = Fib(N, cache, ref count);
        timer.Stop();

        Console.WriteLine($"f({N}) = {result:#,0}");
        Console.WriteLine(result == KnownF40 ? "Result verified." : "Result INCORRECT.");
        PrintReport("Recursive, cached", N, count, timer.Elapsed);
    }

    // Same recursive shape as naive, but each f(n) is computed only once and cached.
    // O(n) instead of O(2^n), but still pays for recursive call overhead and dictionary
    // lookups - compare to the iterative approaches.
    private static int Fib(int n, Dictionary<int, int> cache, ref int count)
    {
        count++;
        if (cache.TryGetValue(n, out int cached)) return cached;
        int result = Fib(n - 1, cache, ref count) + Fib(n - 2, cache, ref count);
        cache[n] = result;
        return result;
    }

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        Console.WriteLine($"{name}: {ops:#,0} operations for n={n}");
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds:F3} ms");
    }
}
```
