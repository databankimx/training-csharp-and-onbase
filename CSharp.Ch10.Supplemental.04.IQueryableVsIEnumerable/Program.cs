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
using System.Linq;
using CSharp.Ch10.Supplemental._04.IQueryableVsIEnumerable.Models;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch10.Supplemental._04.IQueryableVsIEnumerable
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Every LINQ query throughout this chapter has actually been one of two genuinely
         *   different things, even though they're written with the exact same syntax:
         *
         * IEnumerable<T> ("LINQ to Objects")
         *   A Where() clause here takes a Func<T, bool>, an ordinary, already-compiled C#
         *   delegate. The filtering happens IN THIS PROCESS, one item at a time, by
         *   literally calling that delegate for each element. This is what every List<T>/
         *   array query in this chapter's main lesson and Supplementals 01-03 has been.
         *
         * IQueryable<T> ("LINQ to Entities", when the source is an EF DbSet<T>)
         *   A Where() clause here takes an Expression<Func<T, bool>> instead, not a
         *   compiled delegate, a DATA STRUCTURE describing the lambda's logic (an
         *   "expression tree"). EF walks that data structure and TRANSLATES it into SQL,
         *   which then runs on the DATABASE SERVER, not in this process at all. This
         *   project's MurphysLaws examples below are IQueryable<T>, same database as
         *   CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework, needs that project's
         *   README.md setup done first.
         *
         * The practical consequence: an IQueryable<T> query can only use logic EF actually
         *   knows how to translate into SQL. A perfectly ordinary C# method that isn't
         *   translatable throws at runtime the moment the query executes, demonstrated
         *   directly below.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                UsingIEnumerable();
                GenericFunctions.Pause();

                UsingIQueryableAgainstDatabase();
                GenericFunctions.Pause();

                UntranslatableExpressionThrows();
                GenericFunctions.Pause();

                AsEnumerableSwitchesToClientSideEvaluation();
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
        // IEnumerable<T>: LINQ to Objects, runs entirely in this process
        private static void UsingIEnumerable()
        {
            using var db = new ExternalDataContext();

            // .ToList() materializes every row from the database RIGHT NOW, the result is
            //   a genuine, in-memory List<MurphysLaw>, an IEnumerable<T>, completely
            //   disconnected from the database from this point on.
            var allLaws = db.MurphysLaws.ToList();

            // This Where() takes a Func<MurphysLaw, bool>, an ordinary compiled delegate.
            //   Filtering happens HERE, in this process, one MurphysLaw object at a time,
            //   there's no SQL involved in this line at all, the data already left the
            //   database when .ToList() ran above.
            var shortLaws = allLaws.Where(law => law.LawText.Length < 60);

            Console.WriteLine("Laws with short text, filtered client-side (IEnumerable<T>):");
            foreach (var law in shortLaws)
            {
                Console.WriteLine($" - {law.LawName}");
            }
        }

        // IQueryable<T>: LINQ to Entities, translated to SQL, runs on the database server
        private static void UsingIQueryableAgainstDatabase()
        {
            using var db = new ExternalDataContext();

            // No .ToList() here. db.MurphysLaws is IQueryable<MurphysLaw>, and this Where()
            //   takes an Expression<Func<MurphysLaw, bool>>, a data structure describing
            //   the lambda, not a compiled delegate. Nothing has hit the database yet.
            var shortLaws = db.MurphysLaws.Where(law => law.LawText.Length < 60);

            // IQueryable<T>.ToString() (on an EF query specifically) prints the ACTUAL SQL
            //   EF translated this expression tree into, worth reading closely: the
            //   LENGTH() check below came directly from ".LawText.Length < 60" above.
            Console.WriteLine("The SQL EF actually generated from the C# expression above:");
            Console.WriteLine(shortLaws.ToString());

            // The query only actually executes NOW, once enumeration starts
            Console.WriteLine($"{Environment.NewLine}Results (filtered server-side, IQueryable<T>):");
            foreach (var law in shortLaws)
            {
                Console.WriteLine($" - {law.LawName}");
            }
        }

        // A perfectly good C# method that EF simply cannot translate into SQL
        private static void UntranslatableExpressionThrows()
        {
            using var db = new ExternalDataContext();

            try
            {
                // IsPalindrome() below is an ordinary C# method, EF has no way to turn an
                //   arbitrary method call into SQL, it can only translate a known set of
                //   patterns (comparisons, string methods it specifically recognizes like
                //   Contains()/StartsWith(), arithmetic, and so on).
                var palindromicLaws = db.MurphysLaws.Where(law => IsPalindrome(law.LawName)).ToList();

                Console.WriteLine($"Found {palindromicLaws.Count} palindromic law name(s) (this line should not print).");
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine("Threw NotSupportedException, exactly as expected:");
                Console.WriteLine($" - {ex.Message}");
                Console.WriteLine($"{Environment.NewLine}EF6 (unlike some newer ORMs, which fall back to evaluating");
                Console.WriteLine("unsupported expressions client-side with a warning) refuses outright here.");
            }
        }

        // Deliberately not something SQL has any equivalent for
        private static bool IsPalindrome(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            #pragma warning disable IDE0305 // ToArray is clearer than collection initializer here, since we want to enumerate the result twice
            string normalized = new string(text.Where(char.IsLetter).ToArray()).ToLowerInvariant();
            return normalized == new string(normalized.Reverse().ToArray());
            #pragma warning restore IDE0305
        }

        // .AsEnumerable() deliberately switches from server-side to client-side evaluation
        //   partway through a query chain
        private static void AsEnumerableSwitchesToClientSideEvaluation()
        {
            using var db = new ExternalDataContext();

            // Everything BEFORE .AsEnumerable() is still IQueryable<T>, translated to SQL,
            //   and runs on the server: the LawText.Length < 60 filter really does happen
            //   in the database. .AsEnumerable() then converts the (already server-filtered)
            //   results into IEnumerable<T>, so IsPalindrome() below, which EF could never
            //   translate on its own, works fine, it's now ordinary C# running against an
            //   already-fetched, in-memory sequence.
            var results = db.MurphysLaws
                .Where(law => law.LawText.Length < 60)   // translated to SQL, runs server-side
                .AsEnumerable()                          // the switch point
                .Where(law => IsPalindrome(law.LawName)); // ordinary C#, runs client-side

            Console.WriteLine("Short laws (server-side filter) with a palindromic name (client-side filter):");
            foreach (var law in results)
            {
                Console.WriteLine($" - {law.LawName}");
            }
            Console.WriteLine($"{Environment.NewLine}(Likely zero results with the seeded sample data, that's fine, the point is");
            Console.WriteLine("that this runs at all, unlike UntranslatableExpressionThrows() above.)");
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
