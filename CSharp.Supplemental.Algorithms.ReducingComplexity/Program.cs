#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using CSharp.Supplemental.Algorithms.Shared;
#endregion

namespace CSharp.Supplemental.Algorithms.ReducingComplexity
{
    // Five progressively-optimized ways to find every prime below a given max, worst to best,
    // plus a sixth approach (Miller-Rabin) that answers a genuinely different question -
    // "is this one specific number prime" rather than "find every prime below N" - included to
    // make the point that reducing complexity doesn't always mean optimizing the same
    // algorithm further; sometimes it means recognizing you're solving a smaller problem than
    // the one you started with.
    internal static class Program
    {
        #region Constants
        private const int Max = 100_000; // Large enough that "Worst" is genuinely, noticeably
                                          // slow (several seconds) - that contrast is the point
                                          // of this whole lesson.
        #endregion

        #region Main
        private static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Reducing Complexity - Finding Primes (worst to best)");
                Console.WriteLine("=======================================================");
                Console.WriteLine();
                Console.WriteLine("  1. Worst - trial division up to n");
                Console.WriteLine("  2. Bad - trial division up to n/2");
                Console.WriteLine("  3. Ok - trial division up to √n");
                Console.WriteLine("  4. Good - trial division up to √n, skipping even numbers");
                Console.WriteLine("  5. Best - Sieve of Eratosthenes");
                Console.WriteLine("  6. Miller-Rabin (a different question - see Lesson.md)");
                Console.WriteLine();
                Console.WriteLine("  X. Exit");
                Console.WriteLine();
                Console.Write("Choice: ");

                string choice = Console.ReadLine()?.Trim() ?? "";
                if (string.Equals(choice, "X", StringComparison.OrdinalIgnoreCase)) return;

                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1": RunDemo("Worst (trial division to n)", Worst); break;
                        case "2": RunDemo("Bad (trial division to n/2)", Bad); break;
                        case "3": RunDemo("Ok (trial division to √n)", Ok); break;
                        case "4": RunDemo("Good (trial division to √n, odds only)", Good); break;
                        case "5": RunDemo("Best (Sieve of Eratosthenes)", Best); break;
                        case "6": RunMillerRabinDemo(); break;
                        default:
                            Console.WriteLine("That's not a valid choice.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    while (ex != null)
                    {
                        Console.WriteLine(ex);
                        ex = ex.InnerException;
                    }
                }

                Console.WriteLine();
                Console.WriteLine(choice == "5"
                    ? "Press any key to return to the menu (or 'V' for a visual walkthrough)..."
                    : "Press any key to return to the menu...");
                var key = Console.ReadKey(true);
                if (choice == "5" && key.Key == ConsoleKey.V)
                {
                    Visualization.OpenSieve();
                }
            }
        }

        private delegate List<int> PrimeFunction(int max, ref int count);

        // Runs the chosen "find every prime below max" algorithm, verifies the result against
        // a trusted count of primes below 100,000 (a well-documented value: 9,592), and prints
        // the efficiency report.
        private static void RunDemo(string name, PrimeFunction findPrimes)
        {
            int count = 0;

            var timer = Stopwatch.StartNew();
            List<int> primes = findPrimes(Max, ref count);
            timer.Stop();

            bool isCorrect = primes.Count == KnownPrimeCountBelow100000;
            Console.WriteLine($"Found {primes.Count:#,0} primes below {Max:#,0}.");
            Console.WriteLine(isCorrect
                ? "Result verified against the known prime count below 100,000."
                : "Result INCORRECT - this should never happen. Check the implementation.");

            EfficiencyReport.Print(name, Max, count, timer.Elapsed);
        }

        // The number of primes below 100,000 is a well-documented, independently verifiable
        // value - used the same way Recursion's f(40) is, to confirm every approach agrees.
        private const int KnownPrimeCountBelow100000 = 9_592;
        #endregion

        #region 1. Worst - O(n²)
        // For every number up to max, check every possible factor from 2 up to the number
        // itself. The most exhaustive possible approach - checks factors that could never
        // possibly divide evenly (any factor above half the number, for a start) purely
        // because nothing here has ruled them out yet.
        private static List<int> Worst(int max, ref int count)
        {
            var primes = new List<int>();
            for (int n = 2; n <= max; n++)
            {
                bool isPrime = true;
                for (int factor = 2; factor < n; factor++)
                {
                    count++;
                    if (n % factor == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime) primes.Add(n);
            }
            return primes;
        }
        #endregion

        #region 2. Bad - O(n²)
        // Same exhaustive approach, but stops checking factors at n/2 instead of n - no factor
        // larger than half a number can ever divide it evenly, so those checks were always
        // wasted. Still O(n²) overall (dividing the inner bound by a constant 2 doesn't change
        // the complexity class), but a real, measurable constant-factor improvement.
        private static List<int> Bad(int max, ref int count)
        {
            var primes = new List<int>();
            for (int n = 2; n <= max; n++)
            {
                bool isPrime = n == 2;
                if (n > 2)
                {
                    isPrime = true;
                    for (int factor = 2; factor <= n / 2; factor++)
                    {
                        count++;
                        if (n % factor == 0)
                        {
                            isPrime = false;
                            break;
                        }
                    }
                }
                if (isPrime) primes.Add(n);
            }
            return primes;
        }
        #endregion

        #region 3. Ok - O(n^1.5) (n√n) - NOT O(n log n)
        // Stops checking factors at √n instead of n/2 - if n has any factor larger than its
        // own square root, it must also have a corresponding factor smaller than the square
        // root, so checking past that point can never find a factor the earlier checks
        // wouldn't have already caught. This is a genuinely different complexity class from
        // Worst/Bad, not just a smaller constant: roughly n * √n operations total, which is
        // O(n^1.5) (also written O(n√n)) - meaningfully worse than O(n log n), even though the
        // two can look deceptively similar at a glance. See LectureNotes.md for where this
        // mislabeling came from in the source material.
        private static List<int> Ok(int max, ref int count)
        {
            var primes = new List<int>();
            for (int n = 2; n <= max; n++)
            {
                bool isPrime = true;
                int limit = (int)Math.Sqrt(n);
                for (int factor = 2; factor <= limit; factor++)
                {
                    count++;
                    if (n % factor == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime) primes.Add(n);
            }
            return primes;
        }
        #endregion

        #region 4. Good - still O(n^1.5), smaller constant
        // Same √n bound as "Ok", but skips even factors entirely (after handling 2 as a
        // special case) - any even factor above 2 would mean an odd number has an even
        // divisor, which is impossible. Same O(n^1.5) complexity class as "Ok" - checking half
        // as many factors doesn't change the asymptotic class, only the constant - but a real
        // improvement all the same.
        private static List<int> Good(int max, ref int count)
        {
            var primes = new List<int>();
            if (max >= 2) primes.Add(2);
            for (int n = 3; n <= max; n += 2)
            {
                bool isPrime = true;
                int limit = (int)Math.Sqrt(n);
                for (int factor = 3; factor <= limit; factor += 2)
                {
                    count++;
                    if (n % factor == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime) primes.Add(n);
            }
            return primes;
        }
        #endregion

        #region 5. Best - Sieve of Eratosthenes - O(n log log n)
        // A completely different strategy: instead of testing each number for primality one
        // at a time, start by assuming everything is prime, then cross off every multiple of
        // each prime as it's found, working upward. By the time this finishes, whatever hasn't
        // been crossed off is prime. The textbook-correct complexity is O(n log log n) - not
        // simply O(n) as the source material this was adapted from claimed, though log log n
        // grows so slowly that the difference is nearly invisible for any n you'd actually
        // run (log log 100,000 is around 2.4). See LectureNotes.md.
        private static List<int> Best(int max, ref int count)
        {
            var isComposite = new bool[max + 1];
            var primes = new List<int>();

            for (int n = 2; n <= max; n++)
            {
                count++;
                if (isComposite[n]) continue;

                primes.Add(n);
                for (long multiple = (long)n * n; multiple <= max; multiple += n)
                {
                    isComposite[multiple] = true;
                }
            }

            return primes;
        }
        #endregion

        #region 6. Miller-Rabin - a different question entirely
        // Every approach above answers "which numbers below N are prime" by checking every
        // candidate. Miller-Rabin answers a different question: "is this one specific number
        // prime", without needing to know anything about any other number. That's genuinely
        // useful for numbers too large to sieve or trial-divide in any reasonable time - the
        // number used in this demo (a well-known Mersenne prime, 2^61 - 1) would take Worst or
        // Bad an impossibly long time to even approach, and even Best's sieve would need an
        // array with more entries than there is memory on this machine.
        //
        // Miller-Rabin is probabilistic, not deterministic - it can occasionally call a
        // composite number "probably prime" (a false positive), though never the reverse
        // (a true prime is never called composite). Each round with a different witness value
        // cuts the false-positive probability by at least 75%, and this demo runs 20 rounds -
        // enough that a false positive is astronomically unlikely, though not, in principle,
        // impossible. That trade-off (a small, controllable chance of error, in exchange for
        // being able to test numbers no exhaustive method could ever reach) is itself a form
        // of "reducing complexity."
        private static void RunMillerRabinDemo()
        {
            // 2^61 - 1, a known Mersenne prime - too large for exhaustive trial division to
            // approach in any reasonable time, but a well-documented, independently verifiable
            // prime, making it easy to confirm Miller-Rabin gets the right answer.
            BigInteger mersennePrime61 = BigInteger.Pow(2, 61) - 1;
            // The same number plus 2 - composite (divisible by 3), used to confirm Miller-Rabin
            // correctly rejects a nearby non-prime too, not just correctly accepts the prime.
            BigInteger nearbyComposite = mersennePrime61 + 2;

            int count = 0;
            var timer = Stopwatch.StartNew();
            bool primeResult = IsProbablyPrime(mersennePrime61, 20, ref count);
            bool compositeResult = IsProbablyPrime(nearbyComposite, 20, ref count);
            timer.Stop();

            Console.WriteLine($"{mersennePrime61} (2⁶¹ - 1): {(primeResult ? "probably prime" : "composite")} - expected prime, {(primeResult ? "correct" : "INCORRECT")}");
            Console.WriteLine($"{nearbyComposite} (2⁶¹ + 1): {(compositeResult ? "probably prime" : "composite")} - expected composite, {(!compositeResult ? "correct" : "INCORRECT")}");

            EfficiencyReport.Print("Miller-Rabin (20 rounds, both numbers)", 61, count, timer.Elapsed);
        }

        // Miller-Rabin primality test. Writes n-1 as 2^r * d (d odd), then for each of
        // `rounds` witnesses, checks whether that witness proves n composite. If no witness
        // does across all rounds, n is "probably prime."
        private static bool IsProbablyPrime(BigInteger n, int rounds, ref int count)
        {
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n.IsEven) return false;

            BigInteger d = n - 1;
            int r = 0;
            while (d.IsEven)
            {
                d /= 2;
                r++;
            }

            var random = new Random();
            for (int i = 0; i < rounds; i++)
            {
                count++;
                BigInteger a = RandomBigInteger(2, n - 2, random);
                BigInteger x = BigInteger.ModPow(a, d, n);

                if (x == 1 || x == n - 1) continue;

                bool composite = true;
                for (int j = 0; j < r - 1; j++)
                {
                    count++;
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == n - 1)
                    {
                        composite = false;
                        break;
                    }
                }

                if (composite) return false;
            }

            return true;
        }

        // A random BigInteger in the inclusive range [min, max] - System.Random has no native
        // BigInteger support, so this generates enough random bytes to cover the range and
        // retries on the rare occasions that produces a value outside it.
        private static BigInteger RandomBigInteger(BigInteger min, BigInteger max, Random random)
        {
            byte[] bytes = max.ToByteArray();
            BigInteger result;
            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= 0x7F; // force non-negative
                result = new BigInteger(bytes);
            } while (result < min || result > max);
            return result;
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
