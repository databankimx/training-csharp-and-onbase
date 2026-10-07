---
title: "O(1) - Constant Time"
chapter: 0
index: 1
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private const int N = 2_000;

    private static void Main()
    {
        int[] array = GenerateShuffledArray(N);
        int count = 0;

        var timer = Stopwatch.StartNew();
        int first = GetFirstElement(array, ref count);
        timer.Stop();

        Console.WriteLine($"First element: {first}");
        PrintReport("GetFirstElement", N, count, timer.Elapsed);
    }

    // Accessing an array by index costs exactly the same regardless of how large the
    // array is - one operation, every time.
    private static int GetFirstElement(int[] array, ref int count)
    {
        count++;
        return array[0];
    }

    private static int[] GenerateShuffledArray(int n)
    {
        var array = new int[n];
        for (int i = 0; i < n; i++) array[i] = i;
        var rng = new Random();
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        return array;
    }

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        double logN   = Math.Log(n, 2);
        double nLogN  = n * logN;
        double nSqrt  = Math.Pow(n, 1.5);
        double nSq    = (double)n * n;

        string bigO = ops > nSq ? "2^n" : ops > nSqrt ? "n^2" : ops > nLogN ? "n^1.5"
                    : ops > n   ? "n log n" : ops > logN ? "n" : ops > 1 ? "log n" : "1";

        Console.WriteLine($"{name}: {ops:#,0} operations against {n:#,0} elements");
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine($"Estimated complexity: O({bigO})");
    }
}
```
