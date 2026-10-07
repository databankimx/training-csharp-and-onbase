---
title: "Linear Search - O(n)"
chapter: 0
index: 1
dependencies: []
visualizationAlgorithm: linear-search
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private const int N = 10_000;

    private static void Main()
    {
        int[] array = GenerateShuffledArray(N);

        // The array contains every value from 0 to N-1 exactly once, so a target of N is
        // guaranteed to be absent - forcing the true worst case (a full scan with no early
        // exit), which is exactly what Big-O complexity describes.
        int target = N;
        int count  = 0;

        var timer = Stopwatch.StartNew();
        int index = LinearSearch(array, target, ref count);
        timer.Stop();

        Console.WriteLine($"Index: {index} (searched for a value known not to be present - worst case)");
        PrintReport("Linear search", N, count, timer.Elapsed);
    }

    // Algorithm:
    // 1. For i from 0 to n-1
    //    2. If the element at location i is the target, quit (return the index)
    // 3. If we did not find the target element, quit (return failure state)
    private static int LinearSearch(int[] array, int target, ref int count)
    {
        for (int i = 0; i < array.Length; i++)
        {
            count++;
            if (array[i] == target) return i;
        }
        return -1;
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
