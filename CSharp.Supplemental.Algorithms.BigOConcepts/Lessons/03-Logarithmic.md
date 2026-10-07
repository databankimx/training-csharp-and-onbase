---
title: "O(log n) - Logarithmic Time"
chapter: 0
index: 3
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
        Array.Sort(array);
        int target = array[N / 3];
        int count = 0;

        var timer = Stopwatch.StartNew();
        int index = BinarySearch(array, target, 0, array.Length - 1, ref count);
        timer.Stop();

        Console.WriteLine($"Found {target} at index {index}");
        PrintReport("BinarySearch", N, count, timer.Elapsed);
    }

    // Repeatedly halving the remaining search space - each step eliminates half of what's left.
    private static int BinarySearch(int[] array, int target, int low, int high, ref int count)
    {
        if (high < low) return -1;
        count++;
        int mid = low + (high - low) / 2;
        if (array[mid] == target) return mid;
        if (array[mid] < target) return BinarySearch(array, target, mid + 1, high, ref count);
        return BinarySearch(array, target, low, mid - 1, ref count);
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
