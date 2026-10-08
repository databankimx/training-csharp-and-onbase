---
title: "Quick Sort - O(n^2) worst case, O(n log n) average"
chapter: 0
index: 9
dependencies: []
visualizationAlgorithm: quick-sort
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
        QuickSort(array, 0, array.Length - 1, ref count);
        timer.Stop();

        bool ok = IsSorted(array);
        Console.WriteLine(ok ? "Result verified: fully sorted." : "Result INCORRECT.");
        PrintReport("Quick sort", N, count, timer.Elapsed);
    }

    // Picks a pivot (last element), partitions the array into less-than and greater-than,
    // then recursively sorts each partition. O(n^2) worst case on already-sorted input
    // with this pivot choice, but O(n log n) average on random data.
    private static void QuickSort(int[] array, int low, int high, ref int count)
    {
        if (low < high)
        {
            int pivotIndex = Partition(array, low, high, ref count);
            QuickSort(array, low, pivotIndex - 1, ref count);
            QuickSort(array, pivotIndex + 1, high, ref count);
        }
    }

    private static int Partition(int[] array, int low, int high, ref int count)
    {
        int pivot = array[high];
        int i     = low - 1;
        for (int j = low; j < high; j++)
        {
            count++;
            if (array[j] <= pivot)
            {
                i++;
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
        (array[i + 1], array[high]) = (array[high], array[i + 1]);
        return i + 1;
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
