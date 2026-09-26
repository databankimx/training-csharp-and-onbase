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
using CSharp.Supplemental.Algorithms.Shared;
#endregion

namespace CSharp.Supplemental.Algorithms.Recursion
{
    // Six ways to compute the nth Fibonacci number, ordered worst to best. Three of these
    // (RecursiveCached, Iterative, IterativeNonCached) share the same O(n) Big-O class but
    // differ meaningfully in overhead - see Lesson.md for why they're ordered the way they are.
    internal static class Program
    {
        #region Constants
        private const int N = 40; // Large enough that the naive recursive approach's O(2^n)
                                   // cost is dramatically visible (several seconds), small
                                   // enough that it still finishes in reasonable time and the
                                   // result fits comfortably in an int.
        #endregion

        #region Main
        private static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Recursion - Computing Fibonacci Numbers (worst to best)");
                Console.WriteLine("==========================================================");
                Console.WriteLine();
                Console.WriteLine("  1. Recursive (naive) - O(2ⁿ)");
                Console.WriteLine("  2. Recursive, Cached - O(n)");
                Console.WriteLine("  3. Iterative (array-based) - O(n)");
                Console.WriteLine("  4. Iterative (rolling variables) - O(n)");
                Console.WriteLine("  5. Matrix Exponentiation - O(log n)");
                Console.WriteLine("  6. Formulaic (Binet's formula) - O(1)");
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
                        case "1": RunDemo("Recursive (naive)", RunRecursive); break;
                        case "2": RunDemo("Recursive, cached", RunRecursiveCached); break;
                        case "3": RunDemo("Iterative (array-based)", RunIterative); break;
                        case "4": RunDemo("Iterative (rolling variables)", RunIterativeNonCached); break;
                        case "5": RunDemo("Matrix exponentiation", RunMatrixExponentiation); break;
                        case "6": RunDemo("Formulaic", RunFormulaic); break;
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
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
            }
        }

        private delegate int FibFunction(int n, ref int count);

        // Runs the chosen algorithm against N, verifies the result against a trusted
        // reference value, and prints the efficiency report - shared by every menu option.
        private static void RunDemo(string name, FibFunction fib)
        {
            int count = 0;

            var timer = Stopwatch.StartNew();
            int result = fib(N, ref count);
            timer.Stop();

            bool isCorrect = result == KnownFibonacci40;
            Console.WriteLine($"f({N}) = {result:#,0}");
            Console.WriteLine(isCorrect
                ? "Result verified against a known correct value."
                : "Result INCORRECT - this should never happen. Check the implementation.");

            EfficiencyReport.Print(name, N, count, timer.Elapsed);
        }

        // f(40), independently verifiable and small enough to just state directly, used to
        // confirm every one of the six approaches below actually agrees with the others.
        private const int KnownFibonacci40 = 102_334_155;
        #endregion

        #region 1. Recursive (naive) - O(2ⁿ)
        // f(n) = f(n-1) + f(n-2), computed by direct recursion with no memory of anything
        // already computed. Each call spawns two more, all the way down - the number of calls
        // grows exponentially with n (closer to O(φⁿ) in the tightest analysis, but O(2ⁿ) is
        // the standard, still-accurate-enough Big-O classification, since φ < 2 and both are
        // "exponential" for classification purposes).
        private static int RunRecursive(int n, ref int count)
        {
            count++;
            if (n < 2) return n;
            return RunRecursive(n - 1, ref count) + RunRecursive(n - 2, ref count);
        }
        #endregion

        #region 2. Recursive, Cached - O(n)
        // Same recursive shape as above, but remembers every f(n) already computed in a
        // dictionary, so each distinct n only actually gets computed once. O(n) instead of
        // O(2ⁿ) - but still pays for recursive call overhead and dictionary lookups on every
        // single call, which is what puts it behind the two iterative approaches below despite
        // sharing their Big-O class.
        private static int RunRecursiveCached(int n, ref int count)
        {
            var cache = new Dictionary<int, int> { { 0, 0 }, { 1, 1 } };
            return RecursiveCached(n, cache, ref count);
        }

        private static int RecursiveCached(int n, Dictionary<int, int> cache, ref int count)
        {
            count++;
            if (cache.TryGetValue(n, out int cached)) return cached;
            int result = RecursiveCached(n - 1, cache, ref count) + RecursiveCached(n - 2, cache, ref count);
            cache[n] = result;
            return result;
        }
        #endregion

        #region 3. Iterative (array-based) - O(n)
        // Builds a full array of every f(0) through f(n), each entry computed once from the
        // two before it - O(n), no recursion at all, but still pays for allocating and
        // populating an O(n) array up front.
        private static int RunIterative(int n, ref int count)
        {
            var values = new int[n + 1];
            values[0] = 0;
            if (n >= 1) values[1] = 1;
            for (int i = 2; i <= n; i++)
            {
                count++;
                values[i] = values[i - 1] + values[i - 2];
            }
            return values[n];
        }
        #endregion

        #region 4. Iterative (rolling variables) - O(n)
        // Same idea as the array-based version, but only ever remembers the two most recent
        // values instead of the whole history - still O(n) iterations, but O(1) memory instead
        // of O(n), and no array allocation at all. The lowest-overhead of the three O(n)
        // approaches here, which is why it's ordered ahead of both.
        private static int RunIterativeNonCached(int n, ref int count)
        {
            if (n < 2) return n;
            int secondLast = 0;
            int last = 1;
            int result = 0;
            for (int i = 2; i <= n; i++)
            {
                count++;
                result = last + secondLast;
                secondLast = last;
                last = result;
            }
            return result;
        }
        #endregion

        #region 5. Matrix Exponentiation - O(log n)
        // Fibonacci numbers satisfy a neat matrix identity:
        //
        //   [F(n+1)  F(n)  ]   [1 1]ⁿ
        //   [F(n)    F(n-1)] = [1 0]
        //
        // So raising that 2x2 matrix to the nth power gives F(n) directly. Matrix
        // multiplication for a fixed 2x2 size is O(1) (four multiplications, two additions,
        // always), and exponentiation by squaring computes the nth power in O(log n)
        // multiplications rather than n - the same halving-the-problem-each-step idea Binary
        // Search uses. That makes this the only approach here, besides the O(1) formula below,
        // that's asymptotically better than every O(n) approach above it.
        private static int RunMatrixExponentiation(int n, ref int count)
        {
            if (n < 2) return n;

            long[,] baseMatrix = { { 1, 1 }, { 1, 0 } };
            long[,] result = MatrixPower(baseMatrix, n - 1, ref count);
            return (int)result[0, 0];
        }

        // Exponentiation by squaring: repeatedly squares the base matrix, multiplying it into
        // the running result only on the power's set bits - the same bit-by-bit technique the
        // Sort Algorithms project's Radix Sort uses on individual digits, just applied to a
        // binary exponent instead.
        private static long[,] MatrixPower(long[,] matrix, int power, ref int count)
        {
            long[,] result = { { 1, 0 }, { 0, 1 } }; // Identity matrix
            long[,] baseMatrix = matrix;

            while (power > 0)
            {
                count++;
                if ((power & 1) == 1)
                {
                    result = MatrixMultiply(result, baseMatrix);
                }
                baseMatrix = MatrixMultiply(baseMatrix, baseMatrix);
                power >>= 1;
            }

            return result;
        }

        private static long[,] MatrixMultiply(long[,] a, long[,] b)
        {
            return new long[,]
            {
                { a[0, 0] * b[0, 0] + a[0, 1] * b[1, 0], a[0, 0] * b[0, 1] + a[0, 1] * b[1, 1] },
                { a[1, 0] * b[0, 0] + a[1, 1] * b[1, 0], a[1, 0] * b[0, 1] + a[1, 1] * b[1, 1] }
            };
        }
        #endregion

        #region 6. Formulaic - O(1)
        // Binet's formula computes f(n) directly from n, no iteration or recursion of any
        // kind: f(n) = (φⁿ - (1-φ)ⁿ) / √5, where φ (the golden ratio) = (1 + √5) / 2. A fixed
        // number of arithmetic operations regardless of how large n is - genuinely O(1),
        // though it's worth knowing this relies on floating-point math and loses precision at
        // large enough n, unlike every integer-only approach above it.
        private static int RunFormulaic(int n, ref int count)
        {
            count++;
            if (n < 2) return n;
            double phi = (1 + Math.Sqrt(5)) / 2;
            double result = (Math.Pow(phi, n) - Math.Pow(1 - phi, n)) / Math.Sqrt(5);
            return (int)Math.Round(result);
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
