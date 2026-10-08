---
title: "Merge Sort - O(n log n), guaranteed"
chapter: 0
index: 11
dependencies: []
visualizationAlgorithm: merge-sort
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

        bool ok = IsSorted(sorted) && sorted.Length == array.Length;
        Console.WriteLine(ok ? "Result verified: fully sorted." : "Result INCORRECT.");
        PrintReport("Merge sort", N, count, timer.Elapsed);
    }

    // Recursively splits the array in half, sorts each half, then merges the two sorted halves.
    // O(n log n) is guaranteed regardless of input order, at the cost of O(n) extra memory.
    private static int[] MergeSort(int[] array, ref int count)
    {
        if (array.Length <= 1) return array;
        int mid    = array.Length / 2;
        int[] left  = MergeSort(array.Take(mid).ToArray(), ref count);
        int[] right = MergeSort(array.Skip(mid).ToArray(), ref count);
        return Merge(left, right, ref count);
    }

    private static int[] Merge(int[] left, int[] right, ref int count)
    {
        int[] merged = new int[left.Length + right.Length];
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

    private static bool IsSorted(int[] a) { for (int i = 1; i < a.Length; i++) if (a[i] < a[i-1]) return false; return true; }

    private static int[] GenerateShuffledArray(int n)
    {
        var a = new int[n]; for (int i = 0; i < n; i++) a[i] = i;
        var r = new Random(); for (int i = n-1; i > 0; i--) { int j = r.Next(i+1); (a[i],a[j])=(a[j],a[i]); }
        return a;
    }

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        double logN = Math.Log(n, 2), nLogN = n*logN, nSqrt = Math.Pow(n,1.5), nSq = (double)n*n;
        string bigO = ops>nSq?"2^n":ops>nSqrt?"n^2":ops>nLogN?"n^1.5":ops>n?"n log n":ops>logN?"n":ops>1?"log n":"1";
        Console.WriteLine($"{name}: {ops:#,0} operations against {n:#,0} elements");
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds:F3} ms  |  Estimated: O({bigO})");
    }
}
```
