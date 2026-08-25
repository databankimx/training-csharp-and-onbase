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
#endregion

namespace CSharp.Ch10.Supplemental._03.CustomLinqExtensionMethods
{
    /// <summary>
    /// Custom LINQ-style extension methods over IEnumerable&lt;T&gt;, written the exact same
    /// way the real LINQ operators are: extension methods, most of them using yield return
    /// for deferred execution (see CSharp.Ch10.Supplemental.01.DeferredExecution).
    ///
    /// DistinctByCustom() and ChunkCustom() specifically fill a real gap: DistinctBy() and
    /// Chunk() were only added to .NET's own LINQ in .NET 6, they don't exist at all on
    /// net48 (this project's target), so building them here isn't just illustrative, it's
    /// genuinely necessary if this codebase wants that functionality.
    /// </summary>
    public static class CustomLinqExtensions
    {
        #region WhereCustom
        /// <summary>
        /// Re-implements Where(), demonstrating the eager-validation / deferred-execution
        /// split every well-behaved LINQ operator uses, see ChapterNotes in Program.cs
        /// </summary>
        public static IEnumerable<T> WhereCustom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            // Validated HERE, in the public method, which runs immediately when called,
            //   not deferred like the rest of the method's work.
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return WhereCustomIterator(source, predicate);
        }

        // The actual deferred work lives in a SEPARATE method using yield return. Splitting
        //   it out like this is what makes the validation above run eagerly, a yield
        //   return method's body doesn't execute AT ALL until the first MoveNext() call,
        //   see BadWhereCustom() below for what goes wrong without this split.
        private static IEnumerable<T> WhereCustomIterator<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
#pragma warning disable S3267
            foreach (T item in source)
            {
                if (predicate(item)) yield return item;
            }
#pragma warning restore S3267
        }

        /// <summary>
        /// The same idea as WhereCustom(), WITHOUT the eager-validation split, kept here
        /// deliberately to demonstrate the gotcha in Program.cs's
        /// DemonstrateEagerValidationGotcha(). Don't write real code this way.
        /// </summary>
#pragma warning disable S4456
        public static IEnumerable<T> BadWhereCustom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
#pragma warning restore S4456
        {
            // This method itself uses yield return, so NONE of its body, including these
            //   validation checks, runs until the caller actually starts enumerating the
            //   result. Calling BadWhereCustom(null, ...) does NOT throw here, the
            //   ArgumentNullException only fires later, whenever a foreach loop (or
            //   .ToList(), etc.) finally starts pulling values out.
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

#pragma warning disable S3267
            foreach (T item in source)
            {
                if (predicate(item)) yield return item;
            }
#pragma warning restore S3267
        }
        #endregion

        #region DistinctByCustom
        /// <summary>
        /// Keeps only the first element for each distinct key, everything after the first
        /// occurrence of a given key is dropped. Not available on net48's LINQ (added in
        /// .NET 6 as DistinctBy()).
        /// </summary>
        public static IEnumerable<T> DistinctByCustom<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return DistinctByCustomIterator(source, keySelector);
        }

        private static IEnumerable<T> DistinctByCustomIterator<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            // HashSet<TKey>.Add() returns false if the value was already present, exactly
            //   the check needed here: yield the item only the FIRST time its key is seen.
            var seenKeys = new HashSet<TKey>();
#pragma warning disable S3267
            foreach (T item in source)
            {
                if (seenKeys.Add(keySelector(item))) yield return item;
            }
#pragma warning restore S3267
        }
        #endregion

        #region ChunkCustom
        /// <summary>
        /// Splits a sequence into fixed-size batches (the last batch may be smaller). Not
        /// available on net48's LINQ (added in .NET 6 as Chunk()).
        /// </summary>
        public static IEnumerable<T[]> ChunkCustom<T>(this IEnumerable<T> source, int size)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size), "Chunk size must be greater than zero.");

            return ChunkCustomIterator(source, size);
        }

        private static IEnumerable<T[]> ChunkCustomIterator<T>(IEnumerable<T> source, int size)
        {
            var buffer = new List<T>(size);
            foreach (T item in source)
            {
                buffer.Add(item);
                if (buffer.Count == size)
                {
                    yield return buffer.ToArray();
                    buffer.Clear();
                }
            }

            // Final, possibly-smaller batch, if anything's left over
            if (buffer.Count > 0) yield return buffer.ToArray();
        }
        #endregion

        #region Median
        /// <summary>
        /// An aggregate-style operator, like Average()/Sum(), IMMEDIATE rather than
        /// deferred, it has to see the whole sequence to produce its one answer, there's
        /// no way to yield results one at a time the way WhereCustom()/ChunkCustom() do.
        /// </summary>
        public static double Median(this IEnumerable<int> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var sorted = source.OrderBy(n => n).ToList();
            if (sorted.Count == 0) throw new InvalidOperationException("Sequence contains no elements.");

            int mid = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2.0
                : sorted[mid];
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
