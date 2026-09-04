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
using System.Threading;
using System.Threading.Tasks;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S125 // Commented code permitted in lessons
namespace CSharp.Ch07.Supplemental._08.LockFreeAlternatives
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Locking (Monitor, Mutex, Semaphore - see CSharp.Ch07.Supplemental.07.Locking) is both
         *   dangerous (deadlocks, if used carelessly) and resource-intensive. Sometimes you just need
         *   to perform one simple operation (like incrementing a number) and make sure it happens
         *   atomically, without a full lock's overhead.
         *
         * For that, .NET offers the Interlocked class (System.Threading), a set of static methods
         *   that perform simple operations as a single, uninterruptible (atomic) step, no scheduler
         *   context switch can happen in the middle of one.
         *
         * Interlocked Methods (the common ones):
         * - Increment(ref int/long)               Adds 1, returns the new value
         * - Decrement(ref int/long)                Subtracts 1, returns the new value
         * - Add(ref int/long, value)                Adds "value", returns the new value
         * - Exchange(ref T, value)                  Sets the variable to "value", returns the OLD value
         * - CompareExchange(ref T, value, comparand) If the variable currently equals "comparand",
         *                                            sets it to "value". Always returns the ORIGINAL
         *                                            value, regardless of whether the swap happened.
         * - Read(ref long)                          Atomically reads a 64-bit value (only actually
         *                                            necessary on 32-bit platforms, where a plain read
         *                                            of a 64-bit value isn't guaranteed atomic)
         *
         * A quick refresher: the "lock" keyword is shorthand for Monitor.Enter/Exit wrapped in a
         *   try/finally (see CSharp.Ch07.Supplemental.07.Locking for the explicit version):
         *
         *     object syncObject = new object();
         *     lock (syncObject)
         *     {
         *         // Code updating some shared data
         *     }
         *
         * When to reach for which:
         * - Interlocked: a single, simple update to one variable (a counter, a flag, a reference swap)
         * - Monitor/lock: anything more involved, multiple related fields that need to stay consistent
         *   together, or a critical section spanning more than one operation
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Compare an unprotected counter to one protected with Interlocked.Increment
                CompareUnprotectedToInterlocked();
                GenericFunctions.Pause();

                // Demonstrate Interlocked.Add and Interlocked.Decrement
                UsingAddAndDecrement();
                GenericFunctions.Pause();

                // Demonstrate Interlocked.Exchange
                UsingExchange();
                GenericFunctions.Pause();

                // Demonstrate Interlocked.CompareExchange
                UsingCompareExchange();
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
        // Run the same "many threads increment a shared counter" scenario two ways: unprotected
        //   (a real race condition, same shape as CSharp.Ch07.Supplemental.05.RaceConditions) and
        //   protected with Interlocked.Increment (no lock needed at all).
        private static void CompareUnprotectedToInterlocked()
        {
            const int threadCount = 100;
            const int incrementsPerThread = 1000;

            Console.WriteLine("Unprotected counter (expect this to come out wrong)...");
            int unprotectedCounter = 0;
            RunManyIncrementingThreads(threadCount, incrementsPerThread, () => unprotectedCounter++);
            Console.WriteLine($"Expected: {threadCount * incrementsPerThread}, Actual: {unprotectedCounter}");

            Console.WriteLine($"{Environment.NewLine}Interlocked-protected counter (expect this to always be correct)...");
            int protectedCounter = 0;
            RunManyIncrementingThreads(threadCount, incrementsPerThread, () => Interlocked.Increment(ref protectedCounter));
            Console.WriteLine($"Expected: {threadCount * incrementsPerThread}, Actual: {protectedCounter}");
        }

        // Demonstrate Interlocked.Add (adding an arbitrary value, not just 1) and Interlocked.Decrement
        private static void UsingAddAndDecrement()
        {
            int total = 0;

            Console.WriteLine("Adding 10, 20, and 30 from three different threads using Interlocked.Add...");
            Parallel.Invoke(
                () => Interlocked.Add(ref total, 10),
                () => Interlocked.Add(ref total, 20),
                () => Interlocked.Add(ref total, 30));
            Console.WriteLine($"Total (should always be 60): {total}");

            Console.WriteLine($"{Environment.NewLine}Decrementing 100 times from four threads using Interlocked.Decrement...");
            int remaining = 100;
            Parallel.Invoke(
                () => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); },
                () => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); },
                () => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); },
                () => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); });
            Console.WriteLine($"Remaining (should always be 0): {remaining}");
        }

        // Demonstrate Interlocked.Exchange: atomically replace a value and get the old one back
        private static void UsingExchange()
        {
            string currentLeader = "Nobody";

            // Exchange returns the value that was there BEFORE the swap, useful for knowing what
            //   you just replaced without a separate (non-atomic) read-then-write.
            string previousLeader = Interlocked.Exchange(ref currentLeader, "Alice");
            Console.WriteLine($"Leader was '{previousLeader}', now '{currentLeader}'");

            previousLeader = Interlocked.Exchange(ref currentLeader, "Bob");
            Console.WriteLine($"Leader was '{previousLeader}', now '{currentLeader}'");
        }

        // Demonstrate Interlocked.CompareExchange: "set this value, but only if it still equals what
        //   I expect it to be" - the building block behind most genuinely lock-free algorithms
        private static void UsingCompareExchange()
        {
            int flag = 0;

            // CompareExchange(ref flag, 1, 0): "if flag is currently 0, set it to 1". Returns the
            //   ORIGINAL value regardless, so comparing the return value against what you expected
            //   tells you whether your swap actually happened.
            int originalValue = Interlocked.CompareExchange(ref flag, 1, 0);
            bool weSetIt = originalValue == 0;
            Console.WriteLine($"First attempt: flag was {originalValue} before, is {flag} now. We set it: {weSetIt}");

            // Try again: flag is now 1, so this attempt (which also expects 0) will NOT change it.
            originalValue = Interlocked.CompareExchange(ref flag, 1, 0);
            weSetIt = originalValue == 0;
            Console.WriteLine($"Second attempt: flag was {originalValue} before, is {flag} now. We set it: {weSetIt}");

            // This pattern (read the current value, compute a new one, CompareExchange it in, retry
            //   if something else changed it in the meantime) is exactly how a lock-free "increment"
            //   could be built from CompareExchange alone, though Interlocked.Increment already does
            //   this for you when a plain increment is all you need.
        }
        #endregion

        #region Helper Functions
        // Spawn threadCount threads, each calling incrementAction incrementsPerThread times, and
        //   wait for all of them to finish.
        private static void RunManyIncrementingThreads(int threadCount, int incrementsPerThread, Action incrementAction)
        {
            var threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < incrementsPerThread; j++) incrementAction();
                });
                threads[i].Start();
            }

            foreach (var thread in threads) thread.Join();
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
