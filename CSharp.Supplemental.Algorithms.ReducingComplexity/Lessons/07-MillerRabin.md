---
title: "Miller-Rabin - A Different Question"
chapter: 0
index: 6
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Numerics;

internal static class Program
{
    private static void Main()
    {
        // 2^61 - 1, a known Mersenne prime - too large for exhaustive trial division,
        // but well-documented and independently verifiable.
        BigInteger mersennePrime = BigInteger.Pow(2, 61) - 1;
        // The same number plus 2 - composite (divisible by 3), used to confirm the test
        // correctly rejects a nearby non-prime too.
        BigInteger nearbyComposite = mersennePrime + 2;

        int count = 0;
        var timer = Stopwatch.StartNew();
        bool primeResult    = IsProbablyPrime(mersennePrime,   20, ref count);
        bool compositeResult = IsProbablyPrime(nearbyComposite, 20, ref count);
        timer.Stop();

        Console.WriteLine($"{mersennePrime} (2^61 - 1): {(primeResult ? "probably prime" : "composite")} - expected prime, {(primeResult ? "correct" : "INCORRECT")}");
        Console.WriteLine($"{nearbyComposite} (2^61 + 1): {(compositeResult ? "probably prime" : "composite")} - expected composite, {(!compositeResult ? "correct" : "INCORRECT")}");
        Console.WriteLine($"Operations: {count:#,0}  |  Elapsed: {timer.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine();
        Console.WriteLine("Miller-Rabin answers 'is this ONE number prime' rather than 'find every prime below N'.");
        Console.WriteLine("Each of 20 rounds cuts false-positive probability by at least 75%.");
        Console.WriteLine("Probabilistic, not deterministic - false positives are possible but astronomically unlikely.");
    }

    // Miller-Rabin primality test. Writes n-1 as 2^r * d (d odd), then for each witness
    // checks whether that witness proves n composite. If no witness does, n is 'probably prime'.
    private static bool IsProbablyPrime(BigInteger n, int rounds, ref int count)
    {
        if (n < 2) return false;
        if (n == 2 || n == 3) return true;
        if (n.IsEven) return false;

        BigInteger d = n - 1;
        int r = 0;
        while (d.IsEven) { d /= 2; r++; }

        var rng = new Random();
        for (int i = 0; i < rounds; i++)
        {
            count++;
            BigInteger a = RandomBigInteger(2, n - 2, rng);
            BigInteger x = BigInteger.ModPow(a, d, n);
            if (x == 1 || x == n - 1) continue;

            bool composite = true;
            for (int j = 0; j < r - 1; j++)
            {
                count++;
                x = BigInteger.ModPow(x, 2, n);
                if (x == n - 1) { composite = false; break; }
            }
            if (composite) return false;
        }
        return true;
    }

    private static BigInteger RandomBigInteger(BigInteger min, BigInteger max, Random rng)
    {
        byte[] bytes = max.ToByteArray();
        BigInteger result;
        do
        {
            rng.NextBytes(bytes);
            bytes[bytes.Length - 1] &= 0x7F;
            result = new BigInteger(bytes);
        } while (result < min || result > max);
        return result;
    }
}
```
