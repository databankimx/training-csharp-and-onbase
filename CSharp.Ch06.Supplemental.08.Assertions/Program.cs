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
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S125 // Allow commented code in lessons
namespace CSharp.Ch06.Supplemental._08.Assertions
{
    // Default class for console executable
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Using an assertion allows the developer to pause execution in Visual Studio (or in a debug build)
         *   and view the stack trace
         *
         * The basic syntax is:
         *     Debug.Assert(condition);
         *     Debug.Assert(condition, message);
         *
         * When debugging, if an assertion occurs, but its condition is not met, the program halts,
         *   and the stack trace is displayed
         *
         * In a release build, all Assert statements are ignored, Debug.Assert calls are compiled out
         *   entirely because the Debug class's methods carry [Conditional("DEBUG")].
         *
         * IMPORTANT: this project runs interactively. When a failing assertion below actually fires,
         *   .NET's default trace listener shows a real Windows "Assertion Failed" dialog with
         *   Abort/Retry/Ignore buttons, you'll need to click one (Ignore is the safe choice) to let
         *   the program continue. That's expected, not a bug, it's the actual, unmodified behavior
         *   Debug.Assert produces outside of a debugger.
         *
         * Assertions vs. Exceptions:
         *   These solve different problems and are not interchangeable.
         *     - An assertion checks something that should be IMPOSSIBLE if your code is correct, an
         *       internal invariant, a programmer error. It exists purely to catch bugs during
         *       development. It should never fire in correctly-written, bug-free code, and because
         *       it's compiled out of release builds, it must never be relied on to guard against
         *       anything that could legitimately happen at runtime (bad user input, a missing file,
         *       a network failure).
         *     - An exception handles a condition that CAN legitimately happen at runtime, even in
         *       correct code, invalid input, a resource that isn't available, a network call that
         *       fails. Exceptions are never compiled out, and calling code is expected to handle them.
         */
        #endregion

        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                BasicAssertions();
                GenericFunctions.Pause();

                AssertionsVersusExceptions();
                GenericFunctions.Pause();

                DebugAssertVersusTraceAssert();
                GenericFunctions.Pause();

                AssertingAnInternalInvariant();
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
        // The basic syntax: a passing assertion (silent) and a failing one (halts and shows a dialog)
        private static void BasicAssertions()
        {
            Console.WriteLine("1. Basic assertions");
            Console.WriteLine("--------------------");

            int[] scores = [88, 92, 79, 95, 84];
            const int maxPossibleScore = 100;

            Console.WriteLine("Checking that no score exceeds the maximum possible score...");
            // This condition is true, so nothing happens, execution continues silently.
            Debug.Assert(scores.Max() <= maxPossibleScore, "Found a score above the maximum possible score!");
            Console.WriteLine("...passed silently, as expected.\n");

            Console.WriteLine("Checking that the scores array has more than 10 entries (it doesn't)...");
            Console.WriteLine("A real assertion dialog is about to appear, click Ignore to continue.");
            // This condition is false. In a Debug build, this halts execution and shows a dialog
            // with the message below and a stack trace. In a Release build, this line is compiled
            // out entirely, nothing happens at all, not even a check.
            Debug.Assert(scores.Length > 10, $"Expected more than 10 scores, but found {scores.Length}.");
            Console.WriteLine("...execution resumed after the assertion.\n");
        }

        // Demonstrates why assertions and exceptions are not interchangeable
        private static void AssertionsVersusExceptions()
        {
            Console.WriteLine("2. Assertions vs. exceptions");
            Console.WriteLine("-----------------------------");

            Console.WriteLine("Calling ApplyDiscount(50m, 0.1m) with valid input...");
            Console.WriteLine($"Result: {ApplyDiscount(50m, 0.1m):C}\n");

            Console.WriteLine("Calling ApplyDiscount(50m, 1.5m) with an invalid, out-of-range discount...");
            try
            {
                ApplyDiscount(50m, 1.5m);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"Caught the expected exception: {ex.Message}\n");
            }
        }

        // A public-facing method: the discount percentage comes from outside this code (a caller,
        //   a form field, a config file), so it can legitimately be wrong. That's an exception's job.
        private static decimal ApplyDiscount(decimal price, decimal discountPercentage)
        {
            if (discountPercentage < 0 || discountPercentage > 1)
                throw new ArgumentOutOfRangeException(nameof(discountPercentage), discountPercentage, "Discount percentage must be between 0 and 1.");

            decimal discounted = price * (1 - discountPercentage);

            // This is an internal invariant, not input validation. If the math above is correct and
            //   discountPercentage was already validated, discounted can never legitimately be
            //   negative. If this ever fires, it means there's a bug in THIS method, not bad input.
            Debug.Assert(discounted >= 0, "Discounted price should never be negative given validated input.");

            return discounted;
        }

        // Debug.Assert is compiled out of Release builds. Trace.Assert is not, it remains active
        //   as long as the TRACE symbol is defined, which is the default for both Debug and Release
        //   configurations in this solution.
        private static void DebugAssertVersusTraceAssert()
        {
            Console.WriteLine("3. Debug.Assert vs. Trace.Assert");
            Console.WriteLine("----------------------------------");

            Console.WriteLine("Debug.Assert(false, ...) only fires in a Debug build, [Conditional(\"DEBUG\")]");
            Console.WriteLine("  compiles the call out entirely in Release, the check itself never runs there.");
            Console.WriteLine();
            Console.WriteLine("Trace.Assert(false, ...) fires whenever TRACE is defined, Debug AND Release alike.");
            Console.WriteLine("A real assertion dialog is about to appear, click Ignore to continue.");
            Trace.Assert(1 + 1 == 3, "Deliberately false, to demonstrate Trace.Assert firing regardless of build configuration.");
            Console.WriteLine("...execution resumed after the Trace.Assert.\n");
        }

        // A more realistic example: asserting a precondition that this method's own logic depends on
        private static void AssertingAnInternalInvariant()
        {
            Console.WriteLine("4. Asserting an internal invariant");
            Console.WriteLine("-------------------------------------");

            int[] sortedNumbers = [3, 7, 12, 19, 25, 41, 58];

            Console.WriteLine($"Searching for 25 in a pre-sorted array using binary search...");
            int index = BinarySearch(sortedNumbers, 25);
            Console.WriteLine($"Found at index {index}.\n");
        }

        // Binary search only works correctly if its input is actually sorted. That's a precondition
        //   this method relies on but doesn't (and shouldn't) re-verify with an exception on every
        //   call, sorting the array to check it would defeat the point of binary search's speed. An
        //   assertion documents the assumption and catches a violation during development without
        //   costing anything in a release build.
        private static int BinarySearch(int[] sortedArray, int target)
        {
            Debug.Assert(IsSorted(sortedArray), "BinarySearch requires a sorted array, but the input was not sorted.");

            int low = 0;
            int high = sortedArray.Length - 1;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                if (sortedArray[mid] == target) return mid;
                if (sortedArray[mid] < target) low = mid + 1;
                else high = mid - 1;
            }

            return -1;
        }

        // Only used by the assertion above; this is intentionally simple, not production-grade.
        private static bool IsSorted(int[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] < array[i - 1]) return false;
            }
            return true;
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
