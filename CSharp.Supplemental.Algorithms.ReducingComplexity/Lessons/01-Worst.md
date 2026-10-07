---
title: "Worst - Trial Division to n - O(n^2)"
chapter: 0
index: 1
dependencies: []
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
        PrintReport("Worst (trial division to n)", Max, count, timer.Elapsed);
    }

    // For every number up to max, check every possible factor from 2 up to the number itself.
    // Checks factors that could never divide evenly - purely because nothing has ruled them out.
    private static List<int> FindPrimes(int max, ref int count)
    {
        var primes = new List<int>();
        for (int n = 2; n <= max; n++)
        {
            bool isPrime = true;
            for (int f = 2; f < n; f++)
            {
                count++;
                if (n % f == 0) { isPrime = false; break; }
            }
            if (isPrime) primes.Add(n);
        }
        return primes;
    }

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        Console.WriteLine($"{name}: {ops:#,0} operations");
        Console.WriteLine($"Elapsed: {elapsed.TotalSeconds:F3} s");
    }
}
```
