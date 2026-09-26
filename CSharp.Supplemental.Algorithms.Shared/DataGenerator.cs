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
    /// Generates test data for the search/sort demos. Rather than shipping a static, pre-baked
    /// data file (the source material's demos each read from a shared ~52 KB text file of
    /// integers), every value from 0 to n-1 (or, for a signed range, -n/2 to n/2) is generated
    /// and then shuffled - a genuine random permutation of a *complete* range, not just n
    /// random numbers that might happen to repeat or leave gaps. That means the sorted result
    /// is always fully predictable (every consecutive integer in the range, no more, no less),
    /// which matters for validating a sort's correctness, and gives binary search a guaranteed
    /// non-duplicate, evenly-distributed array to search once sorted.
    /// </summary>
    public static class DataGenerator
    {
        /// <summary>
        /// Generate a shuffled array containing every integer in the requested range exactly
        /// once.
        /// </summary>
        /// <param name="n">Number of elements to generate</param>
        /// <param name="signedRange">
        /// When false (the default), generates every integer from 0 to n-1. When true,
        /// generates every integer from -(n/2) to n/2 instead (inclusive of both ends, so the
        /// array will have n+1 elements when n is even) - useful for demos where it matters
        /// that the data includes negative values.
        /// </param>
        public static int[] GenerateShuffledArray(int n, bool signedRange = false)
        {
            int[] array = signedRange
                ? BuildSignedRange(n)
                : BuildUnsignedRange(n);

            Shuffle(array);
            return array;
        }

        // Every integer from 0 to n - 1
        private static int[] BuildUnsignedRange(int n)
        {
            var array = new int[n];
            for (int i = 0; i < n; i++)
            {
                array[i] = i;
            }
            return array;
        }

        // Every integer from -(n / 2) to (n / 2), inclusive of both ends
        private static int[] BuildSignedRange(int n)
        {
            int half = n / 2;
            var array = new int[half * 2 + 1];
            int value = -half;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
                value++;
            }
            return array;
        }

        // Fisher-Yates shuffle - every permutation of the array is equally likely, and it
        // runs in O(n), so shuffling a large generated range doesn't itself distort whatever
        // complexity is actually being measured afterward.
        private static void Shuffle(int[] array)
        {
            var random = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
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
