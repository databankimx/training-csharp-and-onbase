---
title: "O(n log n) - Loglinear Time"
chapter: 0
index: 4
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Linq;

internal static class Program
{
    private const int N = 2_000;

    private static void Main()
    {
        int[] array = GenerateShuffledArray(N);
        int count = 0;

        var timer = Stopwatch.StartNew();
        int[] sorted = MergeSort(array, ref count);
        timer.Stop();

        bool isSorted = true;
        for (int i = 1; i < sorted.Length; i++)
            if (sorted[i] < sorted[i - 1]) { isSorted = false; break; }

        Console.WriteLine(isSorted ? "Result verified: fully sorted." : "Result INCORRECT.");
        PrintReport("MergeSort", N, count, timer.Elapsed);
    }

    // An O(log n) step (halving the array), repeated for every element.
    // Guaranteed O(n log n) regardless of input order.
    private static int[] MergeSort(int[] array, ref int count)
    {
        if (array.Length <= 1) return array;
        int mid    = array.Length / 2;
        int[] left  = MergeSort(array.Take(mid).ToArray(), ref count);
        int[] right = MergeSort(array.Skip(mid).ToArray(), ref count);

        int[] merged = new int[array.Length];
        int i = 0, j = 0, m = 0;
        while (i < left.Length && j < right.Length)
        {
            count++;
            merged[m++] = left[i] <= right[j] ? left[i++] : right[j++];
        }
        while (i < left.Length)  merged[m++] = left[i++];
        while (j < right.Length) merged[m++] = right[j++];
        return merged;
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
