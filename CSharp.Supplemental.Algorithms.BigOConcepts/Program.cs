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
using System.Linq;
using CSharp.Supplemental.Algorithms.Shared;
#endregion

namespace CSharp.Supplemental.Algorithms.BigOConcepts
{
    // The four foundational complexity classes from Lesson.md, each with a small, genuinely
    // runnable example rather than just a code snippet on a page. The four dedicated projects
    // in this Algorithms series (Search, Sort, Recursion, Reducing Complexity) each go deep on
    // one of these; this one just introduces what the classes actually mean, side by side.
    internal static class Program
    {
        #region Constants
        private const int N = 2_000;
        #endregion

        #region Main
        private static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Big-O Complexity - The Foundational Classes");
                Console.WriteLine("=============================================");
                Console.WriteLine();
                Console.WriteLine("  1. O(1) - Constant Time");
                Console.WriteLine("  2. O(n) - Linear Time");
                Console.WriteLine("  3. O(log n) - Logarithmic Time");
                Console.WriteLine("  4. O(n log n) - Loglinear Time");
                Console.WriteLine("  5. O(n²) - Quadratic Time");
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
                        case "1": DemoConstant(); break;
                        case "2": DemoLinear(); break;
                        case "3": DemoLogarithmic(); break;
                        case "4": DemoLoglinear(); break;
                        case "5": DemoQuadratic(); break;
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
        #endregion

        #region 1. O(1) - Constant Time
        // Accessing an array by index costs exactly the same regardless of how large the
        // array is - one operation, every time.
        private static void DemoConstant()
        {
            int[] array = DataGenerator.GenerateShuffledArray(N);
            int count = 0;

            var timer = Stopwatch.StartNew();
            int first = GetFirstElement(array, ref count);
            timer.Stop();

            Console.WriteLine($"First element: {first}");
            EfficiencyReport.Print("GetFirstElement", N, count, timer.Elapsed);
        }

        private static int GetFirstElement(int[] array, ref int count)
        {
            count++; // Exactly one operation, no matter how large "array" is
            return array[0];
        }
        #endregion

        #region 2. O(n) - Linear Time
        // Visiting every element once - the cost grows in direct proportion to the input size.
        private static void DemoLinear()
        {
            int[] array = DataGenerator.GenerateShuffledArray(N);
            int count = 0;

            var timer = Stopwatch.StartNew();
            long sum = SumAllElements(array, ref count);
            timer.Stop();

            Console.WriteLine($"Sum of all {N:#,0} elements: {sum:#,0}");
            EfficiencyReport.Print("SumAllElements", N, count, timer.Elapsed);
        }

        private static long SumAllElements(int[] array, ref int count)
        {
            long sum = 0;
            foreach (int value in array)
            {
                count++;
                sum += value;
            }
            return sum;
        }
        #endregion

        #region 3. O(log n) - Logarithmic Time
        // Repeatedly halving the remaining search space - see the dedicated Search Algorithms
        // project for the full treatment (worst-case forcing, elapsed timing, and a comparison
        // against Linear Search side by side).
        private static void DemoLogarithmic()
        {
            int[] array = DataGenerator.GenerateShuffledArray(N);
            Array.Sort(array);
            int target = array[N / 3]; // An arbitrary value known to be present
            int count = 0;

            var timer = Stopwatch.StartNew();
            int index = BinarySearch(array, target, 0, array.Length - 1, ref count);
            timer.Stop();

            Console.WriteLine($"Found {target} at index {index}");
            EfficiencyReport.Print("BinarySearch", N, count, timer.Elapsed);
        }

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

        #region 4. O(n log n) - Loglinear Time
        // An O(log n) step (halving the array), repeated for every element - see the dedicated
        // Sort Algorithms project for Merge Sort, Heap Sort, and Quick Sort, all of which live
        // in (or, for Quick Sort, average out to) this class.
        private static void DemoLoglinear()
        {
            int[] array = DataGenerator.GenerateShuffledArray(N);
            int count = 0;

            var timer = Stopwatch.StartNew();
            int[] sorted = MergeSort(array, ref count);
            timer.Stop();

            bool isSorted = true;
            for (int i = 1; i < sorted.Length; i++)
            {
                if (sorted[i] < sorted[i - 1]) { isSorted = false; break; }
            }
            Console.WriteLine(isSorted ? "Result verified: fully sorted." : "Result INCORRECT.");
            EfficiencyReport.Print("MergeSort", N, count, timer.Elapsed);
        }

        private static int[] MergeSort(int[] array, ref int count)
        {
            if (array.Length <= 1) return array;
            int mid = array.Length / 2;
            int[] left = MergeSort(array.Take(mid).ToArray(), ref count);
            int[] right = MergeSort(array.Skip(mid).ToArray(), ref count);

            int[] merged = new int[array.Length];
            int i = 0, j = 0, m = 0;
            while (i < left.Length && j < right.Length)
            {
                count++;
                merged[m++] = left[i] <= right[j] ? left[i++] : right[j++];
            }
            while (i < left.Length) merged[m++] = left[i++];
            while (j < right.Length) merged[m++] = right[j++];
            return merged;
        }
        #endregion

        #region 5. O(n²) - Quadratic Time
        // A loop inside a loop, both traversing the same array - the cost grows with the
        // square of the input size. Counts how many times each value in the array repeats.
        private static void DemoQuadratic()
        {
            // A deliberately small, mostly-duplicate array - this demo's point is the nested
            // loop shape itself, not a large realistic dataset the way the others use.
            int[] array = { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 };
            int count = 0;

            var timer = Stopwatch.StartNew();
            int[] duplicateCounts = CountDuplicates(array, ref count);
            timer.Stop();

            for (int i = 0; i < array.Length; i++)
            {
                Console.WriteLine($"{array[i]} appears {duplicateCounts[i]} more time(s) elsewhere in the array");
            }
            EfficiencyReport.Print("CountDuplicates", array.Length, count, timer.Elapsed);
        }

        private static int[] CountDuplicates(int[] array, ref int count)
        {
            var result = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                int duplicates = 0;
                for (int j = 0; j < array.Length; j++)
                {
                    count++;
                    if (j != i && array[j] == array[i]) duplicates++;
                }
                result[i] = duplicates;
            }
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
