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

namespace CSharp.Ch10.Supplemental._03.CustomLinqExtensionMethods
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Every LINQ operator used throughout this chapter, Where(), Select(), OrderBy(),
         *   all of them, is JUST an extension method over IEnumerable<T>. There's nothing
         *   magic about them beyond that, which means writing your own is entirely
         *   reasonable when you have a genuinely reusable query shape that isn't already
         *   covered by the built-in set. See CustomLinqExtensions.cs for the actual
         *   implementations discussed here.
         *
         * DistinctByCustom() and ChunkCustom() specifically aren't just teaching exercises,
         *   .NET only added DistinctBy()/Chunk() to its own LINQ starting in .NET 6, this
         *   project targets net48 (like the rest of this training set), so those built-ins
         *   genuinely don't exist here. Writing them by hand is the actual, practical fix,
         *   not just a demonstration of how they'd theoretically work.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                UsingWhereCustom();
                GenericFunctions.Pause();

                UsingDistinctByCustom();
                GenericFunctions.Pause();

                UsingChunkCustom();
                GenericFunctions.Pause();

                UsingMedian();
                GenericFunctions.Pause();

                DemonstrateEagerValidationGotcha();
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
        // WhereCustom(): a hand-built version of Where(), used exactly like the real thing
        private static void UsingWhereCustom()
        {
            var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // Chains with real LINQ operators exactly like a built-in one would, since it's
            //   just another extension method returning IEnumerable<T>
            var result = numbers.WhereCustom(n => n % 2 == 0).OrderByDescending(n => n);

            Console.WriteLine("Even numbers, via WhereCustom(), chained with real OrderByDescending():");
            foreach (int n in result)
            {
                Console.WriteLine($" - {n}");
            }
        }

        // DistinctByCustom(): keep only the first item per distinct key
        private static void UsingDistinctByCustom()
        {
            var books = new List<(string Title, string Genre)>
            {
                ("1984", "Dystopian"),
                ("Fahrenheit 451", "Dystopian"),
                ("The Hobbit", "Fantasy"),
                ("Brave New World", "Dystopian"),
                ("The Name of the Wind", "Fantasy")
            };

            var oneBookPerGenre = books.DistinctByCustom(b => b.Genre);

            Console.WriteLine("One book per Genre (first occurrence wins):");
            foreach (var (Title, Genre) in oneBookPerGenre)
            {
                Console.WriteLine($" - {Genre}: {Title}");
            }
        }

        // ChunkCustom(): batch a sequence into fixed-size groups
        private static void UsingChunkCustom()
        {
            var numbers = Enumerable.Range(1, 10);

            Console.WriteLine("Numbers 1-10, chunked into groups of 3:");
            foreach (int[] chunk in numbers.ChunkCustom(3))
            {
                Console.WriteLine($" - [{string.Join(", ", chunk)}]");
            }
        }

        // Median(): a custom aggregate operator, immediate rather than deferred
        private static void UsingMedian()
        {
            var oddCount = new List<int> { 5, 3, 1, 4, 2 };
            var evenCount = new List<int> { 5, 3, 1, 4 };

            Console.WriteLine($"Median of [5, 3, 1, 4, 2]: {oddCount.Median()}");
            Console.WriteLine($"Median of [5, 3, 1, 4]: {evenCount.Median()}");
        }

        // The eager-validation gotcha: WhereCustom() vs BadWhereCustom()
        private static void DemonstrateEagerValidationGotcha()
        {
            List<int> nullSource = null;

            Console.WriteLine("Calling WhereCustom(null, ...) (validated eagerly, outside the iterator):");
            try
            {
#pragma warning disable S1481 // Intentionally keeping the variable unused, just to demonstrate the exception behavior
                var query = nullSource.WhereCustom(n => n > 0);
#pragma warning restore S1481
                Console.WriteLine(" - No exception yet, this line printed, which is WRONG for this demo,");
                Console.WriteLine("   the exception below should have already happened.");
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine(" - Threw immediately, exactly when WhereCustom() was called, before any");
                Console.WriteLine("   enumeration happened at all. This is the correct, expected behavior.");
            }

            Console.WriteLine($"{Environment.NewLine}Calling BadWhereCustom(null, ...) (validated inside the iterator method):");
            var badQuery = nullSource.BadWhereCustom(n => n > 0);
            Console.WriteLine(" - No exception yet, even though the source really is null, because");
            Console.WriteLine("   BadWhereCustom()'s body (including its validation) hasn't run at all yet,");
            Console.WriteLine("   it's a yield-return method, its body only starts executing once enumerated.");

            try
            {
                foreach (int n in badQuery) { /* Intentionally empty */ }
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine(" - NOW it throws, only once the foreach loop actually started pulling");
                Console.WriteLine("   values out, potentially far away from where badQuery was originally");
                Console.WriteLine("   created, making the real bug harder to track down.");
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
