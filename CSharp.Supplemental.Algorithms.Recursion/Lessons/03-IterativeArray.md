---
title: "Iterative (array-based) - O(n)"
chapter: 0
index: 3
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
        PrintReport("Iterative (array-based)", N, count, timer.Elapsed);
    }

    // Builds a full array of every f(0) through f(n), each computed once from the two before it.
    // O(n), no recursion, but allocates an O(n) array up front.
    private static int Fib(int n, ref int count)
    {
        var values = new int[n + 1];
        values[0] = 0;
        if (n >= 1) values[1] = 1;
        for (int i = 2; i <= n; i++)
        {
            count++;
            values[i] = values[i - 1] + values[i - 2];
        }
        return values[n];
    }

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        Console.WriteLine($"{name}: {ops:#,0} operations for n={n}");
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds:F3} ms");
    }
}
```
