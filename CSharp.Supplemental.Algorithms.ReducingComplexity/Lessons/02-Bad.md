---
title: "Bad - Trial Division to n/2 - O(n^2)"
chapter: 0
index: 2
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
        PrintReport("Bad (trial division to n/2)", Max, count, timer.Elapsed);
    }

    // Stops checking factors at n/2 - no factor larger than half a number can divide it evenly.
    // Still O(n^2) (dividing the inner bound by 2 doesn't change complexity class), but a
    // real, measurable constant-factor improvement over Worst.
    private static List<int> FindPrimes(int max, ref int count)
    {
        var primes = new List<int>();
        for (int n = 2; n <= max; n++)
        {
            bool isPrime = n == 2;
            if (n > 2)
            {
                isPrime = true;
                for (int f = 2; f <= n / 2; f++)
                {
                    count++;
                    if (n % f == 0) { isPrime = false; break; }
                }
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
