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
using System.Text.RegularExpressions;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch11.Supplemental._01.RegularExpressionsDeepDive
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main lesson used Regex.IsMatch() for a yes/no validity check. This
         *   Supplemental covers everything else regular expressions are good for:
         *   extracting pieces of a match (groups), finding every match in a string (not
         *   just the first), search-and-replace using the matched pieces, case-insensitive
         *   matching, the greedy-vs-lazy quantifier distinction (a genuinely common source
         *   of "why did my pattern match way more than I expected" bugs), and reusing a
         *   compiled Regex instance for anything performance-sensitive.
         */
        #endregion

        #region Constants
        // Regex Timeout - a good idea to avoid catastrophic backtracking in complex patterns
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                BreakingDownTheNamePattern();
                GenericFunctions.Pause();

                ExtractingDataWithGroups();
                GenericFunctions.Pause();

                FindingAllMatches();
                GenericFunctions.Pause();

                ReplacingWithRegex();
                GenericFunctions.Pause();

                UsingRegexOptions();
                GenericFunctions.Pause();

                GreedyVsLazyQuantifiers();
                GenericFunctions.Pause();

                CompiledRegexForPerformance();
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
        // A full breakdown of the exact name pattern used in the main lesson and
        //   CSharp.Ch11.TextbookCode.Ch11RealWorldScenario01
        private static void BreakingDownTheNamePattern()
        {
            const string namePattern = @"^([A-Z][a-z]*[-' ]?)+$";

            Console.WriteLine($"Pattern: {namePattern}");
            Console.WriteLine("Piece by piece:");
            Console.WriteLine("  ^                anchor: must match starting at the very beginning");
            Console.WriteLine("  (...)+           the group repeats one or more times");
            Console.WriteLine("  [A-Z]            exactly one uppercase letter");
            Console.WriteLine("  [a-z]*           zero or more lowercase letters");
            Console.WriteLine("  [-' ]?           an OPTIONAL hyphen, apostrophe, or space");
            Console.WriteLine("  $                anchor: must match all the way to the very end");
            Console.WriteLine();

            string[] candidates = ["Mary", "Mary-Jane", "O'Brien", "Van Der Berg", "mary", "Mary123", ""];
            foreach (string candidate in candidates)
            {
                Console.WriteLine($" - \"{candidate}\" -> {Regex.IsMatch(candidate, namePattern, RegexOptions.Compiled, RegexTimeout)}");
            }
        }

        // Named capture groups: pulling specific PIECES out of a match, not just a yes/no
        private static void ExtractingDataWithGroups()
        {
            const string emailPattern = @"^(?<user>[^@\s]+)@(?<domain>[^@\s]+\.[^@\s]+)$";

            string[] candidates = ["jane.doe@example.com", "not-an-email"];

            foreach (string candidate in candidates)
            {
                Match match = Regex.Match(candidate, emailPattern, RegexOptions.Compiled, RegexTimeout);

                if (match.Success)
                {
                    // Named groups (?<user>...) are read back by that same name, far more
                    //   readable than counting parentheses to find "group 1" vs "group 2"
                    //   in a complex pattern.
                    Console.WriteLine($" - \"{candidate}\" -> user: \"{match.Groups["user"].Value}\", domain: \"{match.Groups["domain"].Value}\"");
                }
                else
                {
                    Console.WriteLine($" - \"{candidate}\" -> no match");
                }
            }
        }

        // Regex.Matches(): finding EVERY match in a string, not just testing the whole
        //   string for one match the way IsMatch()/Match() do
        private static void FindingAllMatches()
        {
            const string phonePattern = @"\d{3}-\d{3}-\d{4}";
            const string text = "Call the office at 555-123-4567, or reach Jane directly at 555-987-6543.";

            MatchCollection matches = Regex.Matches(text, phonePattern, RegexOptions.Compiled, RegexTimeout);

            Console.WriteLine($"Found {matches.Count} phone number(s) in the text:");
            foreach (Match match in matches)
            {
                Console.WriteLine($" - {match.Value} (starting at character {match.Index})");
            }
        }

        // Regex.Replace(): search-and-replace using pieces of what was actually matched
        private static void ReplacingWithRegex()
        {
            const string datePattern = @"(\d{2})/(\d{2})/(\d{4})";
            const string text = "The invoice is dated 08/25/2026.";

            // $1/$2/$3 refer back to the three captured groups (month/day/year), reordered
            //   into ISO 8601 format (year-month-day) in the replacement
            string result = Regex.Replace(text, datePattern, "$3-$1-$2", RegexOptions.Compiled, RegexTimeout);

            Console.WriteLine($"Original: {text}");
            Console.WriteLine($"Replaced: {result}");
        }

        // RegexOptions.IgnoreCase and friends
        private static void UsingRegexOptions()
        {
            const string pattern = "hello";

            Console.WriteLine($"\"Hello World\" matches \"{pattern}\" (case-sensitive, default): {Regex.IsMatch("Hello World", pattern, RegexOptions.Compiled, RegexTimeout )}");
            Console.WriteLine($"\"Hello World\" matches \"{pattern}\" (RegexOptions.IgnoreCase): {Regex.IsMatch("Hello World", pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout )}");
        }

        // Greedy vs. lazy quantifiers: a genuinely common source of "my pattern matched
        //   way more text than I expected" bugs
        private static void GreedyVsLazyQuantifiers()
        {
            const string html = "<b>bold</b> and <i>italic</i>";

            // ".*" is GREEDY, it grabs as MUCH as possible while still letting the overall
            //   pattern succeed, here that means matching from the very first "<" all the
            //   way to the very LAST ">", swallowing "<i>italic</i>" along with it.
            Match greedyMatch = Regex.Match(html, "<.*>", RegexOptions.Compiled, RegexTimeout);
            Console.WriteLine($"Greedy \"<.*>\" matched: \"{greedyMatch.Value}\"");

            // ".*?" is LAZY, it grabs as LITTLE as possible, stopping at the first ">" it
            //   can, matching only "<b>" instead of everything through the final tag.
            Match lazyMatch = Regex.Match(html, "<.*?>", RegexOptions.Compiled, RegexTimeout);
            Console.WriteLine($"Lazy \"<.*?>\" matched: \"{lazyMatch.Value}\"");

            Console.WriteLine($"{Environment.NewLine}Same input, same base pattern, wildly different results, worth remembering: \".*\"");
            Console.WriteLine("defaults to greedy, add \"?\" after a quantifier (*?, +?, ??) to make it lazy instead.");
        }

        // Reusing a compiled Regex instance for anything performance-sensitive
        private static void CompiledRegexForPerformance()
        {
            const string pattern = @"^\d{3}-\d{3}-\d{4}$";
            const string input = "555-123-4567";
            const int iterations = 200_000;

            // The static Regex.IsMatch(string, string) overload internally caches a small,
            //   limited number of recently-used patterns (15, by default), so it's not as
            //   naive as re-parsing the pattern from scratch every single call, but it's
            //   still doing more work per call than a Regex instance you've already built
            //   and hold onto yourself.
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                Regex.IsMatch(input, pattern, RegexOptions.Compiled, RegexTimeout);
            }
            stopwatch.Stop();
            Console.WriteLine($"Regex.IsMatch() static method, {iterations:N0} calls: {stopwatch.ElapsedMilliseconds} ms");

            // A single, reused Regex instance, built once, called many times, RegexOptions.
            //   Compiled additionally has the pattern compiled to actual IL (via
            //   Reflection.Emit) rather than interpreted, worth reaching for specifically
            //   when the SAME pattern will run a large number of times, the compilation
            //   itself has real up-front cost, not worth it for a pattern used once.
            var compiledRegex = new Regex(pattern, RegexOptions.Compiled, RegexTimeout);
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                compiledRegex.IsMatch(input);
            }
            stopwatch.Stop();
            Console.WriteLine($"Reused RegexOptions.Compiled instance, {iterations:N0} calls: {stopwatch.ElapsedMilliseconds} ms");
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
