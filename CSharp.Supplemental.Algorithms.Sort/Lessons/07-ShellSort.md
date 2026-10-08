---
title: "Shell Sort - O(n^2) worst case, faster in practice"
chapter: 0
index: 7
dependencies: []
visualizationAlgorithm: shell-sort
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
        int[] sorted = ShellSort(array, ref count);
        timer.Stop();

        bool ok = IsSorted(sorted) && sorted.Length == array.Length;
        Console.WriteLine(ok ? "Result verified: fully sorted." : "Result INCORRECT.");
        PrintReport("Shell sort", N, count, timer.Elapsed);
    }

    // Generalizes Insertion Sort by comparing elements a decreasing "gap" apart, letting
    // far-apart out-of-order elements move into place in large jumps before the final
    // gap-1 pass. O(n^2) worst case but a much smaller practical constant factor.
    private static int[] ShellSort(int[] array, ref int count)
    {
        int n = array.Length;
        int k = (int)Math.Log(n, 2);
        int interval = (int)Math.Pow(2, k - 1);

        while (interval > 0)
        {
            for (int i = interval; i < n; i++)
            {
                int temp = array[i];
                int j    = i;
                while (j >= interval)
                {
                    count++;
                    if (array[j - interval] <= temp) break;
                    array[j] = array[j - interval];
                    j -= interval;
                }
                array[j] = temp;
            }
            k--;
            interval = (int)Math.Pow(2, k - 1);
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
