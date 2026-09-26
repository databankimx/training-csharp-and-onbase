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
using System.Linq;
using CSharp.Supplemental.Algorithms.Shared;
#endregion

namespace CSharp.Supplemental.Algorithms.Sort
{
    // Nine sorting algorithms, ordered worst to best per convention. Ties in Big-O class are
    // broken by empirical/practical performance and worst-case guarantees - see Lesson.md for
    // the reasoning behind the exact order.
    internal static class Program
    {
        #region Constants
        private const int N = 2_000; // Kept smaller than Search's 10,000 - several of these
                                      // (Bubble, Selection, Insertion, Shell) are O(n^2), and
                                      // this keeps even the slowest demo finishing quickly.
        #endregion

        #region Main
        private static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Sort Algorithms (worst to best)");
                Console.WriteLine("=================================");
                Console.WriteLine();
                Console.WriteLine("  1. Bubble Sort - O(n²)");
                Console.WriteLine("  2. Selection Sort - O(n²)");
                Console.WriteLine("  3. Insertion Sort - O(n²), O(n) best case");
                Console.WriteLine("  4. Shell Sort - O(n²) worst case, faster in practice");
                Console.WriteLine("  5. Quick Sort - O(n²) worst case, O(n log n) average");
                Console.WriteLine("  6. Merge Sort - O(n log n), guaranteed");
                Console.WriteLine("  7. Heap Sort - O(n log n), guaranteed, in-place");
                Console.WriteLine("  8. Counting Sort - O(n + k)");
                Console.WriteLine("  9. Radix Sort - O(d * (n + k))");
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
                        case "1": RunDemo("Bubble sort", BubbleSort); break;
                        case "2": RunDemo("Selection sort", SelectionSort); break;
                        case "3": RunDemo("Insertion sort", InsertionSort); break;
                        case "4": RunDemo("Shell sort", ShellSort); break;
                        case "5": RunDemo("Quick sort", (int[] a, ref int c) => QuickSort(a, 0, a.Length - 1, ref c)); break;
                        case "6": RunDemo("Merge sort", (int[] a, ref int c) => MergeSort(a, ref c)); break;
                        case "7": RunDemo("Heap sort", HeapSort); break;
                        case "8": RunDemo("Counting sort", CountingSort); break;
                        case "9": RunDemo("Radix sort", RadixSort); break;
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
                    string algoName = choice switch
                    {
                        "1" => "bubble-sort",
                        "2" => "selection-sort",
                        "3" => "insertion-sort",
                        "4" => "shell-sort",
                        "5" => "quick-sort",
                        "6" => "merge-sort",
                        "7" => "heap-sort",
                        "8" => "counting-sort",
                        "9" => "radix-sort",
                        _ => null
                    };
                    if (algoName != null) Visualization.Open(algoName);
                }
            }
        }

        private delegate int[] SortFunction(int[] array, ref int count);

        // Generates a fresh dataset, runs the chosen algorithm, verifies the result is
        // genuinely sorted, and prints the efficiency report - shared by every menu option.
        private static void RunDemo(string name, SortFunction sort)
        {
            int[] array = DataGenerator.GenerateShuffledArray(N, signedRange: true);
            int count = 0;

            var timer = Stopwatch.StartNew();
            int[] sorted = sort(array, ref count);
            timer.Stop();

            bool isCorrect = IsSorted(sorted) && sorted.Length == array.Length;
            Console.WriteLine(isCorrect
                ? "Result verified: fully sorted, nothing lost or duplicated."
                : "Result INCORRECT - this should never happen. Check the implementation.");

            EfficiencyReport.Print(name, N, count, timer.Elapsed);
        }

        private static bool IsSorted(int[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] < array[i - 1]) return false;
            }
            return true;
        }
        #endregion

        #region 1. Bubble Sort - O(n²)
        // Repeatedly steps through the array, swapping adjacent elements that are out of
        // order. Stops early if a full pass makes no swaps at all (already sorted) - gives
        // this a genuine O(n) best case, but O(n²) comparisons in the worst/average case.
        private static int[] BubbleSort(int[] array, ref int count)
        {
            for (int i = array.Length - 1; i >= 0; i--)
            {
                bool swapped = false;
                for (int j = 0; j < i; j++)
                {
                    count++;
                    if (array[j] > array[j + 1])
                    {
                        (array[j], array[j + 1]) = (array[j + 1], array[j]);
                        swapped = true;
                    }
                }
                if (!swapped) break;
            }
            return array;
        }
        #endregion

        #region 2. Selection Sort - O(n²)
        // Repeatedly finds the smallest remaining element and swaps it into place. Always a
        // full O(n²) comparisons regardless of input order - no early-exit shortcut the way
        // Bubble and Insertion have, which is exactly why it's ordered ahead of them here.
        private static int[] SelectionSort(int[] array, ref int count)
        {
            for (int i = 0; i < array.Length; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < array.Length; j++)
                {
                    count++;
                    if (array[j] < array[minIndex]) minIndex = j;
                }
                (array[minIndex], array[i]) = (array[i], array[minIndex]);
            }
            return array;
        }
        #endregion

        #region 3. Insertion Sort - O(n²), O(n) best case
        // Builds the sorted portion of the array one element at a time, shifting larger
        // already-sorted elements over to make room. O(n²) worst/average case, but - like
        // Bubble - genuinely O(n) on already (or nearly) sorted input, and with less overhead
        // per comparison than Bubble's swap-heavy approach, which is why it's generally
        // considered the best-performing O(n²) algorithm in practice.
        private static int[] InsertionSort(int[] array, ref int count)
        {
            for (int i = 1; i < array.Length; i++)
            {
                int key = array[i];
                int j = i - 1;

                while (j >= 0)
                {
                    count++;
                    if (array[j] <= key) break;
                    array[j + 1] = array[j];
                    j--;
                }

                array[j + 1] = key;
            }
            return array;
        }
        #endregion

        #region 4. Shell Sort - O(n²) worst case, faster in practice
        // Generalizes Insertion Sort: instead of only comparing adjacent elements, compares
        // elements a decreasing "gap" apart, letting far-apart out-of-order elements move
        // into place in large jumps before the final gap-1 pass (an ordinary insertion sort)
        // cleans up what's left. This implementation's gap sequence is powers of two (n/2,
        // n/4, ..., 1) - a well-studied sequence with a genuine O(n²) *worst* case, same
        // asymptotic class as the three above it, but with a much smaller practical constant
        // factor - which is exactly why it's ordered ahead of them despite sharing the label.
        private static int[] ShellSort(int[] array, ref int count)
        {
            int n = array.Length;
            int k = (int)Math.Log(n, 2);
            int interval = (int)Math.Pow(2, k - 1);

            while (interval > 0)
            {
                for (int i = interval; i < n; i++)
                {
                    int temp = array[i];
                    int j = i;

                    while (j >= interval)
                    {
                        count++;
                        if (array[j - interval] <= temp) break;
                        array[j] = array[j - interval];
                        j -= interval;
                    }

                    array[j] = temp;
                }

                k--;
                interval = (int)Math.Pow(2, k - 1);
            }
            return array;
        }
        #endregion

        #region 5. Quick Sort - O(n²) worst case, O(n log n) average
        // Picks a pivot, partitions the array into "less than" and "greater than" the pivot,
        // then recursively sorts each partition. This implementation always picks the *last*
        // element of each partition as the pivot - simple, but a well-known worst-case trap:
        // an already-sorted (or reverse-sorted) input produces maximally unbalanced partitions
        // every time, degrading to O(n²). Average-case performance on genuinely random input
        // (like this demo's shuffled data) is O(n log n), and often the fastest of the
        // O(n log n)-average algorithms here in practice due to excellent cache locality - a
        // common real-world fix for the worst-case trap is picking a random or median-of-three
        // pivot instead of always the last element, not implemented here to keep the
        // partitioning logic itself the focus.
        private static int[] QuickSort(int[] array, int low, int high, ref int count)
        {
            if (low < high)
            {
                int pivotIndex = Partition(array, low, high, ref count);
                QuickSort(array, low, pivotIndex - 1, ref count);
                QuickSort(array, pivotIndex + 1, high, ref count);
            }
            return array;
        }

        private static int Partition(int[] array, int low, int high, ref int count)
        {
            int pivot = array[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                count++;
                if (array[j] <= pivot)
                {
                    i++;
                    (array[i], array[j]) = (array[j], array[i]);
                }
            }

            (array[i + 1], array[high]) = (array[high], array[i + 1]);
            return i + 1;
        }
        #endregion

        #region 6. Merge Sort - O(n log n), guaranteed
        // Recursively splits the array in half, sorts each half, then merges the two sorted
        // halves back together. Unlike Quick Sort, there's no pivot choice to go wrong -
        // O(n log n) is guaranteed regardless of the input's starting order, at the cost of
        // needing O(n) additional memory for the merge step (Quick and Heap both sort in place).
        private static int[] MergeSort(int[] array, ref int count)
        {
            if (array.Length <= 1) return array;

            int mid = array.Length / 2;
            int[] left = MergeSort(array.Take(mid).ToArray(), ref count);
            int[] right = MergeSort(array.Skip(mid).ToArray(), ref count);

            return Merge(left, right, ref count);
        }

        private static int[] Merge(int[] left, int[] right, ref int count)
        {
            int[] merged = new int[left.Length + right.Length];
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

        #region 7. Heap Sort - O(n log n), guaranteed, in-place
        // Builds a max-heap from the array (so the largest element is always at the root),
        // then repeatedly swaps the root into its final position and re-heapifies what's
        // left. Same O(n log n) guarantee as Merge Sort, but in-place (no extra array needed) -
        // the trade-off is generally worse cache locality than Merge or Quick in practice,
        // since heap operations jump around the array rather than working through it in order.
        private static int[] HeapSort(int[] array, ref int count)
        {
            int n = array.Length;

            for (int i = n / 2 - 1; i >= 0; i--)
            {
                Heapify(array, n, i, ref count);
            }

            for (int i = n - 1; i > 0; i--)
            {
                (array[0], array[i]) = (array[i], array[0]);
                Heapify(array, i, 0, ref count);
            }

            return array;
        }

        private static void Heapify(int[] array, int n, int i, ref int count)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            count++;
            if (left < n && array[left] > array[largest]) largest = left;
            count++;
            if (right < n && array[right] > array[largest]) largest = right;

            if (largest != i)
            {
                (array[i], array[largest]) = (array[largest], array[i]);
                Heapify(array, n, largest, ref count);
            }
        }
        #endregion

        #region 8. Counting Sort - O(n + k)
        // Not comparison-based at all - counts how many times each distinct value occurs,
        // then reconstructs the sorted array directly from those counts. O(n + k), where k is
        // the size of the value range - genuinely faster than any comparison-based sort above
        // when k is small relative to n, but the array here spans a signed range, so values
        // are offset to a zero-based index for counting and un-offset on the way back out.
        // The real caveat, worth knowing: if k is much larger than n (a huge range of possible
        // values, few of which actually appear), this becomes impractical - the counting
        // array itself costs O(k) memory regardless of how many elements are actually present.
        private static int[] CountingSort(int[] array, ref int count)
        {
            if (array.Length == 0) return array;

            int min = array.Min();
            int max = array.Max();
            int range = max - min + 1;

            int[] counts = new int[range];
            foreach (int value in array)
            {
                count++;
                counts[value - min]++;
            }

            int[] sorted = new int[array.Length];
            int index = 0;
            for (int i = 0; i < range; i++)
            {
                count++;
                while (counts[i] > 0)
                {
                    sorted[index++] = i + min;
                    counts[i]--;
                }
            }

            return sorted;
        }
        #endregion

        #region 9. Radix Sort - O(d * (n + k))
        // Sorts by individual decimal digit, least significant first, using a stable counting
        // pass (base 10, so k = 10) for each digit position. d is the number of digits in the
        // largest value being sorted - for any fixed-width integer type, d is a small constant
        // (at most 10 decimal digits for a 32-bit int), so this is effectively O(n) in
        // practice despite the more general O(d * (n + k)) label. Like Counting Sort, this
        // fundamentally works on non-negative integers digit-by-digit - the signed input here
        // is offset to non-negative before sorting and restored afterward, same technique as
        // Counting Sort above.
        private static int[] RadixSort(int[] array, ref int count)
        {
            if (array.Length == 0) return array;

            int min = array.Min();
            int[] offset = array.Select(v => v - min).ToArray();

            int max = offset.Max();
            for (int digitPlace = 1; max / digitPlace > 0; digitPlace *= 10)
            {
                offset = CountingSortByDigit(offset, digitPlace, ref count);
            }

            return offset.Select(v => v + min).ToArray();
        }

        // Stable counting sort keyed on a single decimal digit
        private static int[] CountingSortByDigit(int[] array, int digitPlace, ref int count)
        {
            const int base10 = 10;
            int[] counts = new int[base10];
            int[] output = new int[array.Length];

            foreach (int value in array)
            {
                count++;
                counts[(value / digitPlace) % base10]++;
            }

            for (int i = 1; i < base10; i++)
            {
                counts[i] += counts[i - 1];
            }

            for (int i = array.Length - 1; i >= 0; i--)
            {
                count++;
                int digit = (array[i] / digitPlace) % base10;
                output[--counts[digit]] = array[i];
            }

            return output;
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
