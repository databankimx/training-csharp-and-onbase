---
title: "Good - Trial Division to sqrt(n), Odds Only - O(n^1.5)"
chapter: 0
index: 4
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
        PrintReport("Good (trial division to sqrt(n), odds only)", Max, count, timer.Elapsed);
    }

    // Same sqrt(n) bound as Ok, but skips even factors after handling 2 as a special case.
    // Any even factor above 2 would mean an odd number has an even divisor, which is impossible.
    // Still O(n^1.5) - checking half as many factors doesn't change complexity class - but a
    // real constant-factor improvement over Ok.
    private static List<int> FindPrimes(int max, ref int count)
    {
        var primes = new List<int>();
        if (max >= 2) primes.Add(2);
        for (int n = 3; n <= max; n += 2)
        {
            bool isPrime = true;
            int limit    = (int)Math.Sqrt(n);
            for (int f = 3; f <= limit; f += 2)
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
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds:F3} ms");
    }
}
```
