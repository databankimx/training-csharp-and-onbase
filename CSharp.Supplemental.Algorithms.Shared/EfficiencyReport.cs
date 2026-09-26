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
#endregion

namespace CSharp.Supplemental.Algorithms.Shared
{
    /// <summary>
    /// Shared "guess the Big-O class from a measured operation count" reporter, used by every
    /// algorithm demo project under the Algorithms solution folder. Ported from a pattern that
    /// was copy-pasted, near-identically, into every single algorithm file in the source
    /// material - one copy here instead of six-plus.
    /// </summary>
    public static class EfficiencyReport
    {
        /// <summary>
        /// Print a report guessing the Big-O complexity class of an algorithm from its
        /// measured operation count against a given input size, along with reference values
        /// for each major complexity class at that same input size.
        /// </summary>
        /// <param name="algorithmName">Display name of the algorithm being reported on</param>
        /// <param name="n">Input size the algorithm was run against</param>
        /// <param name="operationCount">Measured number of "significant" operations performed</param>
        /// <param name="elapsed">
        /// Measured wall-clock time the algorithm took to run, if available. Purely
        /// informational - actual elapsed time depends heavily on hardware, JIT warm-up, and
        /// what else is running on the machine at the time, none of which Big-O notation
        /// accounts for. The operation count above is what the complexity estimate is actually
        /// based on; this is just a second, complementary data point.
        /// </param>
        public static void Print(string algorithmName, int n, int operationCount, TimeSpan? elapsed = null)
        {
            try
            {
                Console.WriteLine($"{algorithmName} performed {operationCount:#,0} operations against an input of {n:#,0}.");
                if (elapsed.HasValue)
                {
                    Console.WriteLine($"Elapsed time: {FormatElapsed(elapsed.Value)}");
                }

                double logN = Math.Log(n, 2);
                double nLogN = n * logN;
                // n^1.5 (n * sqrt(n)) sits strictly between n log n and n squared - trial
                // division up to the square root of a number (rather than up to the number
                // itself, or half of it) lands exactly here, and it is NOT the same thing as
                // n log n, even though the two can look deceptively similar at a glance.
                double nToOneAndHalf = Math.Pow(n, 1.5);
                double nSquared = (double)n * n;

                string bigO;
                string name;

                if (operationCount > nSquared)
                {
                    bigO = "2ⁿ";
                    name = "exponential";
                }
                else if (operationCount > nToOneAndHalf)
                {
                    bigO = "n²";
                    name = "quadratic (polynomial)";
                }
                else if (operationCount > nLogN)
                {
                    bigO = "n^1.5 (n√n)";
                    name = "between loglinear and quadratic";
                }
                else if (operationCount > n)
                {
                    bigO = "n log n";
                    name = "loglinear";
                }
                else if (operationCount > logN)
                {
                    bigO = "n";
                    name = "linear";
                }
                else if (operationCount > 1)
                {
                    bigO = "log n";
                    name = "logarithmic";
                }
                else
                {
                    bigO = "1";
                    name = "constant";
                }

                Console.WriteLine($"Estimated complexity: O({bigO}) - {name}");
                Console.WriteLine("Reference values at this input size:");
                Console.WriteLine($" · log n    = {logN:#,0.##}");
                Console.WriteLine($" · n        = {n:#,0}");
                Console.WriteLine($" · n log n  = {nLogN:#,0}");
                Console.WriteLine($" · n^1.5    = {nToOneAndHalf:#,0}");
                Console.WriteLine($" · n²       = {nSquared:#,0}");
                Console.WriteLine($" · 2ⁿ, n!   = omitted - too large to calculate for any realistic n");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error generating efficiency report!", ex);
            }
        }

        // Formats a TimeSpan as whichever unit (seconds/milliseconds/microseconds/nanoseconds)
        // makes the number readable, picking the largest unit that still shows at least 1.
        // Computed from Ticks (1 tick = 100 ns) rather than TimeSpan.TotalMicroseconds/
        // TotalNanoseconds, since those properties don't exist before .NET 7 and this library
        // targets net48.
        private static string FormatElapsed(TimeSpan elapsed)
        {
            if (elapsed.TotalSeconds >= 1) return $"{elapsed.TotalSeconds:0.####} s";
            if (elapsed.TotalMilliseconds >= 1) return $"{elapsed.TotalMilliseconds:0.####} ms";

            double microseconds = elapsed.Ticks / 10.0;
            if (microseconds >= 1) return $"{microseconds:0.####} µs";

            double nanoseconds = elapsed.Ticks * 100.0;
            return $"{nanoseconds:0} ns";
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
