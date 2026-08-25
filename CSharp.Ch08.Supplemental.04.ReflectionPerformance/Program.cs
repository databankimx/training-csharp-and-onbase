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
using System.Diagnostics;
using System.Reflection;
using CSharp.Ch08.Supplemental._04.ReflectionPerformance.Models;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch08.Supplemental._04.ReflectionPerformance
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Every project in this chapter, and the main lesson's own Chapter Notes, repeats the
         *   same warning: reflection is resource-intensive, use it deliberately, not by default.
         *   This project puts an actual number behind that warning, timing direct code against
         *   equivalent reflection-based code, over enough iterations that the difference is
         *   impossible to miss.
         *
         * The real finding here isn't "reflection is slow", it's more specific and more useful
         *   than that: the expensive part is usually the LOOKUP (GetProperty(), GetMethod()),
         *   not the actual GetValue()/SetValue()/Invoke() call once you already have the
         *   PropertyInfo/MethodInfo in hand. Looking something up once and reusing that
         *   reference is the single highest-impact optimization available when reflection is
         *   genuinely the right tool for the job.
         */
        #endregion

        #region Private Constants
        private const int Iterations = 1_000_000;
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Compare direct property access to reflection-based access (PropertyInfo cached)
                CompareDirectVsReflectedPropertyAccess();
                GenericFunctions.Pause();

                // Compare direct method calls to reflection-based Invoke() (MethodInfo cached)
                CompareDirectVsReflectedMethodCalls();
                GenericFunctions.Pause();

                // Compare a cached PropertyInfo lookup to looking it up fresh every iteration
                CompareCachedVsUncachedLookup();
                GenericFunctions.Pause();
                #endregion
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
                GenericFunctions.Pause();
            }
            finally
            {
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Lesson Methods
        // Compare direct property access to reflection-based access
        private static void CompareDirectVsReflectedPropertyAccess()
        {
            Console.WriteLine($"Setting a property {Iterations:N0} times: direct vs. reflection (PropertyInfo cached beforehand)...");

            var counter = new Counter();

            var directTimer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                counter.Value = i;
            }
            directTimer.Stop();

            // The PropertyInfo lookup happens ONCE, here, before the timed loop starts.
            PropertyInfo valueProperty = typeof(Counter).GetProperty("Value");

            var reflectedTimer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                valueProperty?.SetValue(counter, i);
            }
            reflectedTimer.Stop();

            PrintComparison("Direct property set", directTimer.Elapsed, "Reflected property set (cached)", reflectedTimer.Elapsed);
        }

        // Compare direct method calls to reflection-based Invoke()
        private static void CompareDirectVsReflectedMethodCalls()
        {
            Console.WriteLine($"{Environment.NewLine}Calling a method {Iterations:N0} times: direct vs. reflection (MethodInfo cached beforehand)...");

            var counter = new Counter();

            var directTimer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                counter.Increment();
            }
            directTimer.Stop();

            // Same principle: the MethodInfo lookup happens ONCE, before the timed loop.
            MethodInfo incrementMethod = typeof(Counter).GetMethod("Increment");

            var reflectedTimer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                incrementMethod?.Invoke(counter, null);
            }
            reflectedTimer.Stop();

            PrintComparison("Direct method call", directTimer.Elapsed, "Reflected method call (cached)", reflectedTimer.Elapsed);
        }

        // Compare a cached PropertyInfo lookup to looking it up fresh every single iteration,
        //   the mistake that actually causes most of reflection's real-world performance cost
        private static void CompareCachedVsUncachedLookup()
        {
            Console.WriteLine($"{Environment.NewLine}Setting a property {Iterations:N0} times: PropertyInfo cached once vs. looked up every iteration...");

            var counter = new Counter();
            var counterType = typeof(Counter);

            PropertyInfo cachedProperty = counterType.GetProperty("Value");

            var cachedTimer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                cachedProperty?.SetValue(counter, i);
            }
            cachedTimer.Stop();

            var uncachedTimer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                // GetProperty("Value") runs fresh on every single iteration here, this is the
                //   pattern to avoid: doing the expensive lookup inside a hot loop.
                counterType.GetProperty("Value")?.SetValue(counter, i);
            }
            uncachedTimer.Stop();

            PrintComparison("Cached PropertyInfo", cachedTimer.Elapsed, "Uncached (re-looked-up every iteration)", uncachedTimer.Elapsed);
        }

        // Print two timing results side by side, with a ratio, and a one-line takeaway
        private static void PrintComparison(string label1, TimeSpan time1, string label2, TimeSpan time2)
        {
            Console.WriteLine($" - {label1}: {time1.TotalMilliseconds:N1} ms");
            Console.WriteLine($" - {label2}: {time2.TotalMilliseconds:N1} ms");

            if (time1.TotalMilliseconds > 0)
            {
                double ratio = time2.TotalMilliseconds / time1.TotalMilliseconds;
                Console.WriteLine($" - {label2} took roughly {ratio:N1}x as long as {label1}.");
            }
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
