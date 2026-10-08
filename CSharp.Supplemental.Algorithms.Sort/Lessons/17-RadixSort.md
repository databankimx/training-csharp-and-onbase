---
title: "Radix Sort - O(d * (n + k))"
chapter: 0
index: 17
dependencies: []
visualizationAlgorithm: radix-sort
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
        int[] array = GenerateShuffledArray(N, signedRange: true);
        int count = 0;

        var timer = Stopwatch.StartNew();
        int[] sorted = RadixSort(array, ref count);
        timer.Stop();

        bool ok = IsSorted(sorted) && sorted.Length == array.Length;
        Console.WriteLine(ok ? "Result verified: fully sorted." : "Result INCORRECT.");
        PrintReport("Radix sort", N, count, timer.Elapsed);
    }

    // Sorts by individual decimal digit (least significant first), using a stable counting
    // pass per digit position. Effectively O(n) for fixed-width integers since d is constant.
    // Signed input is offset to non-negative before sorting and restored afterward.
    private static int[] RadixSort(int[] array, ref int count)
    {
        if (array.Length == 0) return array;
        int min = array.Min();
        int[] offset = array.Select(v => v - min).ToArray();
        int max = offset.Max();
        for (int place = 1; max / place > 0; place *= 10)
            offset = CountingPassByDigit(offset, place, ref count);
        return offset.Select(v => v + min).ToArray();
    }

    private static int[] CountingPassByDigit(int[] array, int place, ref int count)
    {
        int[] counts = new int[10];
        int[] output = new int[array.Length];
        foreach (int v in array) { count++; counts[(v / place) % 10]++; }
        for (int i = 1; i < 10; i++) counts[i] += counts[i - 1];
        for (int i = array.Length - 1; i >= 0; i--)
        {
            count++;
            int d = (array[i] / place) % 10;
            output[--counts[d]] = array[i];
        }
        return output;
    }

    private static bool IsSorted(int[] a) { for (int i = 1; i < a.Length; i++) if (a[i] < a[i-1]) return false; return true; }

    private static int[] GenerateShuffledArray(int n, bool signedRange = false)
    {
        int[] a;
        if (signedRange)
        {
            int half = n / 2; a = new int[half*2+1]; int v = -half;
            for (int i = 0; i < a.Length; i++) a[i] = v++;
        }
        else { a = new int[n]; for (int i = 0; i < n; i++) a[i] = i; }
        var r = new Random(); for (int i = a.Length-1; i > 0; i--) { int j = r.Next(i+1); (a[i],a[j])=(a[j],a[i]); }
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
