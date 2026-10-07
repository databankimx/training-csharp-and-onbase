---
title: "Selection Sort - O(n^2)"
chapter: 0
index: 3
dependencies: []
visualizationAlgorithm: selection-sort
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
        int[] sorted = SelectionSort(array, ref count);
        timer.Stop();

        bool ok = IsSorted(sorted) && sorted.Length == array.Length;
        Console.WriteLine(ok ? "Result verified: fully sorted." : "Result INCORRECT.");
        PrintReport("Selection sort", N, count, timer.Elapsed);
    }

    // Repeatedly finds the smallest remaining element and swaps it into place.
    // Always a full O(n^2) comparisons regardless of input order - no early-exit shortcut.
    private static int[] SelectionSort(int[] array, ref int count)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int minIndex = i;
            for (int j = i + 1; j < array.Length; j++)
            {
                count++;
                if (array[j] < array[minIndex]) minIndex = j;
            }
            (array[minIndex], array[i]) = (array[i], array[minIndex]);
        }
        return array;
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
