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
using CSharp.Supplemental.Algorithms.Shared;
#endregion

namespace CSharp.Supplemental.Algorithms.Search
{
    // Two fundamentally different search strategies - listed worst to best, per convention:
    // Linear Search (O(n)) first, Binary Search (O(log n)) second.
    internal static class Program
    {
        #region Constants
        // Array size for both demos
        private const int N = 10_000;
        #endregion

        #region Main
        private static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Search Algorithms");
                Console.WriteLine("==================");
                Console.WriteLine();
                Console.WriteLine("  1. Linear Search - O(n)");
                Console.WriteLine("  2. Binary Search - O(log n)");
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
                        case "1": DemoLinearSearch(); break;
                        case "2": DemoBinarySearch(); break;
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
                Console.WriteLine("Press any key to return to the menu (or 'V' for a visual walkthrough)...");
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.V)
                {
                    string algoName = choice == "1" ? "linear-search" : choice == "2" ? "binary-search" : null;
                    if (algoName != null) Visualization.Open(algoName);
                }
            }
        }
        #endregion

        #region Linear Search - O(n)
        private static void DemoLinearSearch()
        {
            int[] array = DataGenerator.GenerateShuffledArray(N);

            // The array contains every value from 0 to N-1 exactly once, so a target of N is
            // guaranteed to be absent - forcing the true worst case (a full scan with no early
            // exit), which is exactly what Big-O complexity describes.
            int target = N;
            int count = 0;

            var timer = Stopwatch.StartNew();
            int index = LinearSearch(array, target, ref count);
            timer.Stop();

            Console.WriteLine($"Index: {index} (searched for a value known not to be present, to force the worst case)");
            EfficiencyReport.Print("Linear search", N, count, timer.Elapsed);
        }

        // Algorithm:
        // 1. For i from 0 to n-1
        //    2. If the element at location i is the target, quit (return the index)
        // 3. If we did not find the target element, quit (return failure state)
        private static int LinearSearch(int[] array, int target, ref int count)
        {
            for (int i = 0; i < array.Length; i++)
            {
                count++;
                if (array[i] == target)
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion

        #region Binary Search - O(log n)
        private static void DemoBinarySearch()
        {
            int[] array = DataGenerator.GenerateShuffledArray(N);
            Array.Sort(array); // Binary search requires an ordered array - using the trusted
                                // BCL sort here rather than one of the algorithms this
                                // solution's own Sort Algorithms project builds from scratch,
                                // to avoid a circular dependency between the two projects.

            int target = N; // Same reasoning as Linear Search - guaranteed absent, worst case.
            int count = 0;

            var timer = Stopwatch.StartNew();
            int index = BinarySearch(array, target, 0, array.Length - 1, ref count);
            timer.Stop();

            Console.WriteLine($"Index: {index} (searched for a value known not to be present, to force the worst case)");
            EfficiencyReport.Print("Binary search", N, count, timer.Elapsed);
        }

        // Algorithm (recursive):
        // 1. If the range is empty, quit (return failure state)
        // 2. Otherwise, check the midpoint
        //    3. If it's the target, quit (return the index)
        //    4. Else if it's less than the target, search the upper half
        //    5. Else, search the lower half
        private static int BinarySearch(int[] array, int target, int low, int high, ref int count)
        {
            if (high < low) return -1;

            count++;
            int mid = low + (high - low) / 2;

            if (array[mid] == target) return mid;
            if (array[mid] < target) return BinarySearch(array, target, mid + 1, high, ref count);
            return BinarySearch(array, target, low, mid - 1, ref count);
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
