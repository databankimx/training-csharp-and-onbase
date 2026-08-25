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
using System.Linq;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch10.Supplemental._01.DeferredExecution
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Most LINQ operators (Where, Select, OrderBy, GroupBy, Join, Skip, Take, Distinct...)
         *   are DEFERRED: writing "var query = collection.Where(...)" doesn't actually run
         *   anything yet, it builds a description of the work to do. That work only actually
         *   happens when the query is ENUMERATED, a foreach loop, a call to ToList()/ToArray(),
         *   or anything else that pulls values out of it one at a time.
         *
         * This has two real, practical consequences worth internalizing, both demonstrated
         *   below: the query sees whatever the source collection looks like AT ENUMERATION
         *   TIME, not at the moment the query was written, and enumerating the SAME query
         *   variable twice runs the underlying work twice, not once.
         *
         * A handful of operators are IMMEDIATE instead: ToList(), ToArray(), ToDictionary(),
         *   Count(), Sum(), Average(), Min(), Max(), First(), Last(), Single(), Any(), All(),
         *   Contains(). Each of these has to walk the whole (or enough of the) source right
         *   then to produce their single answer, so they can't stay deferred the way something
         *   producing a lazy sequence can.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                DeferredExecutionSeesLaterChanges();
                GenericFunctions.Pause();

                MultipleEnumerationReRunsTheQuery();
                GenericFunctions.Pause();

                ForcingImmediateExecution();
                GenericFunctions.Pause();

                ModifyingDuringEnumerationThrows();
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
        // Deferred execution: the query sees whatever the source looks like when it's
        //   actually enumerated, not when it was written
        private static void DeferredExecutionSeesLaterChanges()
        {
            var numbers = new List<int> { 1, 2, 3, 4, 5 };

            // Nothing has run yet, this just describes "even numbers from numbers"
            var evenNumbers = numbers.Where(n => n % 2 == 0);

            // Modifying the source AFTER defining the query, but BEFORE enumerating it
            numbers.Add(6);
            numbers.Add(8);

            Console.WriteLine("evenNumbers, enumerated AFTER adding 6 and 8 to the source list:");
            foreach (int n in evenNumbers)
            {
                Console.WriteLine($" - {n}");
            }
            // 6 and 8 show up, even though they were added after evenNumbers was written,
            //   because the Where() clause didn't actually run until this foreach loop did.
        }

        // Enumerating the same deferred query twice runs the underlying work twice, not once
        private static void MultipleEnumerationReRunsTheQuery()
        {
            var numbers = new List<int> { 1, 2, 3, 4, 5 };

            var evenNumbers = numbers.Where(n =>
            {
                // A side effect purely to make the re-execution visible, real predicates
                //   obviously wouldn't normally print anything.
                Console.WriteLine($"   (evaluating {n})");
                return n % 2 == 0;
            });

            Console.WriteLine("First enumeration:");
            foreach (int n in evenNumbers) Console.WriteLine($" - {n}");

            Console.WriteLine($"{Environment.NewLine}Second enumeration of the SAME query variable:");
            foreach (int n in evenNumbers) Console.WriteLine($" - {n}");

            // Every "(evaluating ...)" line printed TWICE, once per enumeration, the
            //   predicate reran completely both times. Worth internalizing as a real
            //   performance concern: enumerating a deferred query more than once against
            //   an expensive source (a database query, a slow computation) does that
            //   expensive work again each time, not once.
        }

        // ToList()/ToArray() force immediate execution, capturing a snapshot right then
        private static void ForcingImmediateExecution()
        {
            var numbers = new List<int> { 1, 2, 3, 4, 5 };

            // .ToList() forces Where() to run RIGHT NOW, evenNumbersSnapshot is a real,
            //   independent List<int>, not a description of future work anymore.
            var evenNumbersSnapshot = numbers.Where(n => n % 2 == 0).ToList();

            numbers.Add(6);
            numbers.Add(8);

            Console.WriteLine("evenNumbersSnapshot, after adding 6 and 8 to the source list:");
            foreach (int n in evenNumbersSnapshot)
            {
                Console.WriteLine($" - {n}");
            }
            // 6 and 8 do NOT show up here, unlike DeferredExecutionSeesLaterChanges() above,
            //   because ToList() already captured the results before those numbers existed.
        }

        // Modifying a List<T> while a deferred query over it is mid-enumeration throws
        private static void ModifyingDuringEnumerationThrows()
        {
            var numbers = new List<int> { 1, 2, 3, 4, 5 };
            var evenNumbers = numbers.Where(n => n % 2 == 0);

            try
            {
                foreach (int n in evenNumbers)
                {
                    Console.WriteLine($" - {n}");

                    // Modifying "numbers" while "evenNumbers" (built directly over it) is
                    //   still mid-enumeration, List<T>'s enumerator detects this and throws
                    //   rather than risk returning inconsistent or corrupted results.
                    if (n == 2) numbers.Add(100);
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"{Environment.NewLine}Threw as expected: {ex.Message}");
                Console.WriteLine("Modifying a collection while a deferred query over it is still being");
                Console.WriteLine("enumerated is not safe, the fix is to materialize first (.ToList()) if the");
                Console.WriteLine("source needs to change during the loop, or build a separate list of changes");
                Console.WriteLine("to apply after the loop finishes.");
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
