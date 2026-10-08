---
title: "O(n^2) - Quadratic Time"
chapter: 0
index: 5
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private static void Main()
    {
        // A deliberately small, mostly-duplicate array - the point is the nested loop shape,
        // not a large realistic dataset.
        int[] array = { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 };
        int count = 0;

        var timer = Stopwatch.StartNew();
        int[] duplicateCounts = CountDuplicates(array, ref count);
        timer.Stop();

        for (int i = 0; i < array.Length; i++)
            Console.WriteLine($"{array[i]} appears {duplicateCounts[i]} more time(s) elsewhere in the array");

        PrintReport("CountDuplicates", array.Length, count, timer.Elapsed);
    }

    // A loop inside a loop, both traversing the same array.
    // The cost grows with the square of the input size.
    private static int[] CountDuplicates(int[] array, ref int count)
    {
        var result = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            int duplicates = 0;
            for (int j = 0; j < array.Length; j++)
            {
                count++;
                if (j != i && array[j] == array[i]) duplicates++;
            }
            result[i] = duplicates;
        }
        return result;
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
