---
title: "Best - Sieve of Eratosthenes - O(n log log n)"
chapter: 0
index: 5
dependencies: []
browserUrl: "../../CSharp.Supplemental.Algorithms.Visualizations/sieve.html"
---

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;

internal static class Program
{
    private const int Max = 100_000;
    private const int KnownPrimeCount = 9_592;

    private static void Main()
    {
        int count = 0;
        var timer = Stopwatch.StartNew();
        var primes = FindPrimes(Max, ref count);
        timer.Stop();

        Console.WriteLine($"Found {primes.Count:#,0} primes below {Max:#,0}.");
        Console.WriteLine(primes.Count == KnownPrimeCount ? "Result verified." : "Result INCORRECT.");
        PrintReport("Best (Sieve of Eratosthenes)", Max, count, timer.Elapsed);
        Console.WriteLine("Note: textbook-correct complexity is O(n log log n), not simply O(n).");
    }

    // A completely different strategy: assume everything is prime, then cross off every
    // multiple of each prime as it's found. Whatever isn't crossed off when done is prime.
    // O(n log log n) - log log n grows so slowly it's nearly invisible in practice.
    private static List<int> FindPrimes(int max, ref int count)
    {
        var isComposite = new bool[max + 1];
        var primes      = new List<int>();

        for (int n = 2; n <= max; n++)
        {
            count++;
            if (isComposite[n]) continue;
            primes.Add(n);
            for (long m = (long)n * n; m <= max; m += n)
                isComposite[m] = true;
        }
        return primes;
    }

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        Console.WriteLine($"{name}: {ops:#,0} operations");
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds:F3} ms");
    }
}
```
