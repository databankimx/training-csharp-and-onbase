---
title: "Formulaic - Binet's Formula - O(1)"
chapter: 0
index: 6
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private const int N = 40;
    private const int KnownF40 = 102_334_155;

    private static void Main()
    {
        int count = 0;
        var timer = Stopwatch.StartNew();
        int result = Fib(N, ref count);
        timer.Stop();

        Console.WriteLine($"f({N}) = {result:#,0}");
        Console.WriteLine(result == KnownF40 ? "Result verified." : "Result INCORRECT.");
        PrintReport("Formulaic (Binet)", N, count, timer.Elapsed);
        Console.WriteLine("Note: relies on floating-point arithmetic - loses precision at large n.");
    }

    // Binet's formula: f(n) = (phi^n - (1-phi)^n) / sqrt(5), where phi = (1 + sqrt(5)) / 2.
    // A fixed number of arithmetic operations regardless of n - genuinely O(1).
    private static int Fib(int n, ref int count)
    {
        count++;
        if (n < 2) return n;
        double phi    = (1 + Math.Sqrt(5)) / 2;
        double result = (Math.Pow(phi, n) - Math.Pow(1 - phi, n)) / Math.Sqrt(5);
        return (int)Math.Round(result);
    }

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        Console.WriteLine($"{name}: {ops:#,0} operations for n={n}");
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds:F3} ms");
    }
}
```
