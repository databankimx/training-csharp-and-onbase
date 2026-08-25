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
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
#endregion

namespace CSharp.Ch11.InputValidationDebuggingAndInstrumentation
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * This chapter covers three related but distinct concerns:
         * - Input Validation: making sure data entering your program is well-formed AND
         *     reasonable, two different checks (see UsingSanityChecks() below for why
         *     "well-formed" and "reasonable" aren't the same thing)
         * - Debugging: preprocessor directives and Debug/Trace, tools for understanding
         *     what a program is doing WHILE you're developing it
         * - Instrumenting Applications: logging and profiling, tools for understanding
         *     what a program did or how it performed, often long after the fact, in
         *     production, where a debugger was never attached at all
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                #region Input Validation
                UsingBuiltInFunctions();
                GenericFunctions.Pause();

                UsingStringMethods();
                GenericFunctions.Pause();

                UsingRegularExpressions();
                GenericFunctions.Pause();

                UsingSanityChecks();
                GenericFunctions.Pause();
                #endregion

                #region Managing Data Integrity
                UsingAssertions();
                GenericFunctions.Pause();
                #endregion

                #region Debugging
                PreprocessorDirectivesDemo();
                GenericFunctions.Pause();

                UsingDebugAndTrace();
                GenericFunctions.Pause();
                #endregion

                #region Instrumenting Applications
                LoggingToEventLog();
                GenericFunctions.Pause();

                ProfilingByHand();
                GenericFunctions.Pause();
                #endregion
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

        #region Input Validation
        // Built-in TryParse functions: the standard, safe way to check whether text
        //   represents a valid value of a given type, without risking an exception
        private static void UsingBuiltInFunctions()
        {
            string[] candidates = ["42", "not a number", "3.14", ""];

            Console.WriteLine("int.TryParse():");
            foreach (string candidate in candidates)
            {
                bool isValid = int.TryParse(candidate, out int result);
                Console.WriteLine($" - \"{candidate}\" -> valid: {isValid}, value: {(isValid ? result.ToString() : "n/a")}");
            }

            Console.WriteLine($"{Environment.NewLine}decimal.TryParse():");
            bool priceIsValid = decimal.TryParse("19.99", out decimal price);
            Console.WriteLine($" - \"19.99\" -> valid: {priceIsValid}, value: {price:C}");

            Console.WriteLine($"{Environment.NewLine}DateTime.TryParse():");
            bool dateIsValid = DateTime.TryParse("2026-08-25", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
            Console.WriteLine($" - \"2026-08-25\" -> valid: {dateIsValid}, value: {(dateIsValid ? date.ToShortDateString() : "n/a")}");
        }

        // String methods: checking for blank/whitespace-only input, a common first line
        //   of validation before anything more specific
        private static void UsingStringMethods()
        {
            string[] candidates = ["Hello", "", "   ", null];

            Console.WriteLine("string.IsNullOrEmpty() vs string.IsNullOrWhiteSpace():");
            foreach (string candidate in candidates)
            {
                string display = candidate == null ? "(null)" : $"\"{candidate}\"";
                Console.WriteLine($" - {display} -> IsNullOrEmpty: {string.IsNullOrEmpty(candidate)}, IsNullOrWhiteSpace: {string.IsNullOrWhiteSpace(candidate)}");
            }
            // Worth noticing "   " (whitespace-only): IsNullOrEmpty() says it's fine (it's
            //   not literally empty), IsNullOrWhiteSpace() correctly flags it. Whichever is
            //   "correct" depends on whether whitespace-only input is genuinely acceptable
            //   for the field in question.
        }

        // Regular expressions: validating that text matches an expected SHAPE, not just
        //   that it parses as some type
        private static void UsingRegularExpressions()
        {
            // Matches a capitalized name: one or more words, each starting with a capital
            //   letter, optionally separated by hyphens/apostrophes/spaces (see
            //   CSharp.Ch11.Supplemental.01.RegularExpressionsDeepDive for a full breakdown
            //   of this exact pattern)
            const string namePattern = @"^([A-Z][a-z]*[-' ]?)+$";

            string[] candidates = ["Mary", "Mary-Jane", "O'Brien", "mary", "Mary123"];

            Console.WriteLine($"Testing against pattern: {namePattern}");
            foreach (string candidate in candidates)
            {
                bool isMatch = Regex.IsMatch(candidate, namePattern, RegexOptions.Compiled, TimeSpan.FromSeconds(10));
                Console.WriteLine($" - \"{candidate}\" -> matches: {isMatch}");
            }
        }

        // Sanity checks: validation ISN'T just "is this well-formed", it's also "is this
        //   REASONABLE". "150" is a perfectly valid, parseable integer, it's still worth
        //   questioning as somebody's age.
        private static void UsingSanityChecks()
        {
            int[] ages = [ 25, -5, 150, 0 ];

            const int minReasonableAge = 0;
            const int maxReasonableAge = 120;

            Console.WriteLine("Sanity-checking ages (syntactically valid integers, semantically questionable ones flagged):");
            foreach (int age in ages)
            {
                bool isReasonable = age >= minReasonableAge && age <= maxReasonableAge;
                Console.WriteLine($" - {age}: {(isReasonable ? "reasonable" : "UNUSUAL, worth confirming with the user")}");
            }
            // Worth the distinction: a sanity check failing shouldn't necessarily BLOCK the
            //   input the way a syntax failure should, 0 and 120 are both real, possible
            //   ages, just unusual enough to be worth a second look (a confirmation prompt),
            //   not an outright rejection. See CSharp.Ch11.TextbookCode.Ch11RealWorldScenario01
            //   for a full, interactive example of exactly this "confirm anyway?" pattern.
        }
        #endregion

        #region Managing Data Integrity
        // Assertions: statements that should ALWAYS be true if the program is working
        //   correctly, used to catch bugs during development, not to validate user input
        private static void UsingAssertions()
        {
            int quantity = 5;
            decimal unitPrice = 9.99m;
            decimal total = quantity * unitPrice;

            // Debug.Assert() only does anything in DEBUG builds, it's compiled out
            //   entirely in Release builds (see PreprocessorDirectivesDemo() below for the
            //   #if DEBUG mechanism that makes this possible). This assertion should
            //   ALWAYS pass, if it doesn't, that's a bug in THIS code, not bad user input.
            Debug.Assert(total == quantity * unitPrice, "Total calculation is inconsistent!");

            Console.WriteLine($"Assertion passed: total ({total:C}) matches quantity * unitPrice.");
            Console.WriteLine($"{Environment.NewLine}Note: a FAILING Debug.Assert() shows an interactive \"Assert Failed\" dialog");
            Console.WriteLine("by default in a Debug build, worth knowing before adding one to code that");
            Console.WriteLine("might run unattended, it will hang waiting for someone to dismiss it.");
        }
        #endregion

        #region Debugging
        // Preprocessor directives: instructions to the COMPILER, not the running program,
        //   #if/#endif decide which code even gets compiled in the first place
        private static void PreprocessorDirectivesDemo()
        {
#if DEBUG
            Console.WriteLine("This build defines DEBUG, so this line was compiled in.");
#else
            Console.WriteLine("This build does NOT define DEBUG, so the DEBUG branch above was never compiled at all.");
#endif
            // Worth knowing about, not demonstrated live here since they'd either break
            //   the build or aren't runtime-observable:
            //   #warning "message"   forces a compiler warning at this exact line
            //   #error "message"     forces a compile ERROR, stops the build entirely
            //   #region/#endregion   pure editor folding, zero effect on the compiled output
            //   #pragma warning disable/restore CS0168   silences a specific warning
            //     number for a section of code, then restores normal warning behavior
            //   See CSharp.Ch11.Supplemental.02.PreprocessorDirectivesDeepDive for all of
            //   these actually demonstrated.
        }

        // Debug and Trace: similar-looking APIs with a real, important difference in when
        //   they actually do anything
        private static void UsingDebugAndTrace()
        {
            // Debug.WriteLine() is compiled OUT ENTIRELY in Release builds (same #if DEBUG
            //   mechanism as Debug.Assert() above), and even in Debug builds, by default it
            //   only goes to the DEBUGGER's Output window, not the console, so nothing from
            //   this specific line is visible when this lesson is run outside a debugger.
            Debug.WriteLine("This line only appears in a debugger's Output window (Debug build only).");

            // Trace.WriteLine() ALWAYS compiles in (Debug and Release both), and by
            //   default ALSO only writes to attached listeners (none, by default, in a
            //   console app), not the console. Adding a ConsoleTraceListener explicitly is
            //   what makes the line below actually show up here.
            Trace.Listeners.Add(new ConsoleTraceListener());
            #pragma warning disable S6670 // SonarQube: "Trace.WriteLine() should not be used in production code" (this is a lesson, not production code)
            Trace.WriteLine("This line goes through Trace.Listeners, visible now that a ConsoleTraceListener was added.");
            #pragma warning restore S6670

            Console.WriteLine($"{Environment.NewLine}(The Debug.WriteLine() line above did not print to this console, by design,");
            Console.WriteLine("the Trace.WriteLine() line did, because of the listener just added. See");
            Console.WriteLine("CSharp.Ch11.Supplemental.03.TraceListeners for a full exploration of this.)");
        }
        #endregion

        #region Instrumenting Applications
        // Logging to the Windows Event Log
        private static void LoggingToEventLog()
        {
            const string source = "CSharp.Ch11.InputValidationDebuggingAndInstrumentation";
            const string log = "Application";

            try
            {
                // Creating a NEW event source requires administrator privileges, writing
                //   to one that already exists does not. Wrapped in try/catch since this
                //   lesson shouldn't fail outright just because it wasn't run as admin.
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, log);
                }

                EventLog.WriteEntry(source, "CSharp.Ch11 lesson ran successfully.", EventLogEntryType.Information);
                Console.WriteLine($"Wrote an entry to the \"{log}\" event log under source \"{source}\".");
                Console.WriteLine("Open Windows Event Viewer (eventvwr.msc) -> Windows Logs -> Application to see it.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write to the event log (likely needs administrator privileges to create");
                Console.WriteLine($"the event source the first time): {ex.Message}");
            }
        }

        // Profiling by hand: Stopwatch-based timing, the simplest possible way to answer
        //   "which of these two approaches is actually faster"
        private static void ProfilingByHand()
        {
            const int iterations = 100_000;

            var stopwatch = Stopwatch.StartNew();
            string concatenated = "";
            for (int i = 0; i < iterations; i++)
            {
                #pragma warning disable S1643 // SonarQube: "String concatenation in a loop can be very inefficient" (this is the point of this lesson)
                concatenated += "x";
                #pragma warning restore S1643
            }
            stopwatch.Stop();
            Console.WriteLine($"Concatenated: {concatenated.Length} characters");
            Console.WriteLine($"String concatenation in a loop ({iterations:N0} iterations): {stopwatch.ElapsedMilliseconds} ms");

            stopwatch.Restart();
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < iterations; i++)
            {
                builder.Append("x");
            }
            stopwatch.Stop();
            Console.WriteLine($"Built: {builder.Length} characters");
            Console.WriteLine($"StringBuilder.Append() in a loop ({iterations:N0} iterations): {stopwatch.ElapsedMilliseconds} ms");

            Console.WriteLine($"{Environment.NewLine}StringBuilder should measure noticeably faster, string concatenation in a loop");
            Console.WriteLine("re-allocates a brand new string on every single iteration (strings are immutable),");
            Console.WriteLine("StringBuilder grows an internal buffer instead. See");
            Console.WriteLine("CSharp.Ch11.Supplemental.04.PerformanceCountersAndProfiling for more on both this");
            Console.WriteLine("technique and PerformanceCounter, the system-level equivalent.");
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
