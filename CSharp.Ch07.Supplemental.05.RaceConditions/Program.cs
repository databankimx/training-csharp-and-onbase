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

#region Directives
using System;
using System.Threading;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S125 // Allow commented out code in lessons
namespace CSharp.Ch07.Supplemental._05.RaceConditions
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * A race condition occurs when two threads try to update the same data.
         * 
         * Example:
         * Assume that you have one variable called sharedData and two threads, and both of them want to run the
         *   following instruction:
         *     sharedData++ (which is executed by the CPU in the following way:)
         *   - Read sharedData in a register.
         *   - Add 1 to the value in the register.
         *   - Write the new value from the register back into sharedData variable.
         * 
         * Why is that important to know? Because if it would have been only one instruction, the CPU would have
         *   executed that once, and no error can be introduced here. That is called atomic operation. But when
         *   you have a multi-threaded application, the scheduler can interrupt the current thread at any time,
         *   and that might result in an error.
         * 
         * Here's how:
         * - sharedData has an initial value of 0.
         * - The first thread runs the first instruction, reading the value 0.
         * - The second thread runs the first instruction, reading the value 0.
         *   - On a single-core machine, this can happen when the scheduler interrupts the first thread and
         *     schedules the second thread.
         *   - In a multi-core machine this is a common situation because the threads can be scheduled on different cores.
         * - The first thread increments the value to 1.
         * - The first thread writes back the value 1 into sharedData.
         * - The second thread increments the value to 1. Now the value should have been 2, but the value that the
         *   second thread has is the "old" value of 0.
         * - The second thread writes back the value 1 into sharedData.
         * * Result: The change implemented by the first thread is lost
         *
         * Because of this vulnerability, it is critical to ensure that only one thread can access a shared resource
         *   at any given time.
         *
         * Strategies to avoid a race condition (in order of most- to least-favorable) include:
         * - Don't share the resource at all
         * - Make the data read-only
         * - Isolate the data in smaller modules
         * - Use synchronization mechanisms
         *
         * For this chapter, we will look at the last option: mechanisms to synchronize the data
         *
         * There are two mechanisms for this:
         *
         * - Synchronization Events
         *   - These can be in one of two states:
         *     - signaled       (flag raised)
         *     - non-signaled   (flag lowered)
         *   - Usage:
         *     - When the thread needs to access the shared resource, it checks the state of the event
         *       - signaled:     continue the process
         *       - non-signaled: block the process until the event is signaled
         *     - On completion, a thread signals the event, allowing the next waiting thread access
         *
         *   - Two types of synchronization events:
         *
         *     - EventWaitHandle
         *       Provides a mechanism to track when a thread signals completion
         *         METHODS
         *       - Reset()          Sets the state to non-signaled
         *       - Set()            Sets the state to signaled
         *       - WaitOne()        Blocks the current thread until the event is signaled
         *         PROPERTIES
         *       - EventResetMode   Determines if the event automatically resets or must be reset manually
         *
         *       - There are two child classes that create an EventWaitHandle and set the EventResetMode themselves:
         *         - AutoResetEvent
         *         - ManualResetEvent
         *
         *     - CountdownEvent
         *       Provides a mechanism to track a group of threads and block until they all signal completion
         *         METHODS
         *       - AddCount()       Adds one to the pending count
         *       - TryAddCount()    Adds one to the pending count is possible, otherwise false
         *       - Signal()         Indicates completion of an item (decrements the count)
         *       - Reset()          Reset to the initial could (or a specified count)
         *       - Wait()           Blocks the thread until the countdown becomes set
         *         PROPERTIES
         *       - InitialCount     Number of signals set when the countdown was created
         *       - CurrentCount     Remaining signals in the countdown
         *       - WaitHandle       Built-in wait handle for the set event
         *       - IsSet            Returns whether or not the countdown is set (count = 0)
         *
         * - Barriers
         *
         *   - Barrier
         *     Provides a means of grouping threads to rejoin at specified conditions
         *     METHODS
         *     - AddParticipant()       Adds a process to the barrier
         *     - AddParticipants()      Adds multiple processes to the barrier
         *     - RemoveParticipant()    Removes a process from the barrier
         *     - RemoveParticipants()   Removes multiple processes from the barrier
         *     - SignalAndWait()        Indicates that a process has reached the barrier and will await the others
         *     PROPERTIES
         *     - CurrentPhaseNumber     Identifies the barrier's current phase
         *     - ParticipantCount       Number of processes participating in the barrier
         *     - ParticipantsRemaining  Number of participating processes that have not yet reached the barrier
         *
         *      Note: Barriers will be covered in the next project: CSharp.Ch07.Supplemental.06.Barriers
         */
        #endregion

        #region Private Globals
        // This is the shared resource we will use to demonstrate the race condition
        private static int sharedRegister;
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Demonstrate a race condition occurring
                RaceCondition();
                GenericFunctions.Pause();

                // Prevent race condition by synchronizing using an EventWaitHandle
                UsingEventWaitHandle();
                GenericFunctions.Pause();

                // Prevent race condition by synchronizing using a CountdownEvent
                UsingCountdownEvent();
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

        #region Chapter Lessons
        // Demonstrate a race condition occurring
        private static void RaceCondition()
        {
            Console.WriteLine("Not Synchronizing Threads");
            Console.WriteLine("-------------------------");
            sharedRegister = 0;
            Console.WriteLine("Shared Register:");
            Console.WriteLine($"Start Value: {sharedRegister}\n");

            foreach (int n in new [] {1, 2})
            {
                var thread = new Thread(() => UpdateSharedResource(n));
                thread.Start();
            }
            // This is a cheat way of resynchronizing after the threads.
            // Don't do this in production code
            Thread.Sleep(200);

            Console.WriteLine("\nShared Register:");
            Console.WriteLine($"Expected Value: 2\nActual Value:  {sharedRegister}");
        }

        // Prevent race condition by synchronizing using an EventWaitHandle
        private static void UsingEventWaitHandle()
        {
            Console.WriteLine("Using an EventWaitHandle");
            Console.WriteLine("------------------------");
            sharedRegister = 0;
            Console.WriteLine("Shared Register:");
            Console.WriteLine($"Start Value: {sharedRegister}\n");

            foreach (int n in new[] { 1, 2 })
            {
                var done = new EventWaitHandle(false, EventResetMode.AutoReset);
                // Equivalent to:
                //   var done = new AutoResetEvent(false);

                ThreadPool.QueueUserWorkItem(x =>
                {
                    UpdateSharedResourceWithEvent(n, done);
                });

                // This defeats the point of being multi-threaded, but by forcing the threads to be synchronous (sequential),
                //   we avoid race conditions on the shared resource
                done.WaitOne();
            }

            Console.WriteLine("\nShared Register:");
            Console.WriteLine($"Expected Value: 2\nActual Value:  {sharedRegister}");
        }

        // Prevent race condition by synchronizing using a CountdownEvent
        private static void UsingCountdownEvent()
        {
            Console.WriteLine("Using a CountdownEvent");
            Console.WriteLine("----------------------");
            sharedRegister = 0;
            Console.WriteLine("Shared Register:");
            Console.WriteLine($"Start Value: {sharedRegister}\n");

            // Track two pending worker threads, one signal per thread when it finishes.
            var countdown = new CountdownEvent(2);

            foreach (int n in new[] { 1, 2 })
            {
                ThreadPool.QueueUserWorkItem(x => UpdateSharedResourceWithCountdown(n, countdown));
            }

            // Block the calling thread until BOTH workers have signaled completion.
            countdown.Wait();

            Console.WriteLine("\nShared Register:");
            Console.WriteLine($"Expected Value: 2\nActual Value:  {sharedRegister}");
        }
        #endregion

        #region Helper Functions
        // Demonstrate synchronizing completion with a CountdownEvent.
        //
        // Important: a CountdownEvent by itself only tells the calling thread WHEN both
        //   background threads have finished, it does nothing to stop them from touching
        //   sharedRegister at the same time WHILE they're running. That's a different
        //   problem (mutual exclusion, not completion tracking), which is why this uses
        //   Interlocked.Increment instead of UpdateSharedResource()'s read/sleep/write
        //   pattern, Interlocked.Increment performs the read-add-write sequence as a
        //   single atomic CPU operation, so there's no window for the race described in
        //   the Chapter Notes to actually occur. CountdownEvent gets you correct timing
        //   (you know when it's safe to read the final result); Interlocked gets you
        //   correct data (the increments themselves can't be lost). Both are needed here.
        private static void UpdateSharedResourceWithCountdown(int num, CountdownEvent countdown)
        {
            Console.WriteLine($"Start thread {num}...");
            Thread.Sleep(100);
            Interlocked.Increment(ref sharedRegister);
            Console.WriteLine($"Thread {num} incremented shared register...");
            Console.WriteLine($"End thread {num}...");
            countdown.Signal();
        }

        // Demonstrate synchronizing with an EventWaitHandle
        private static void UpdateSharedResourceWithEvent(int num, EventWaitHandle done)
        {
            UpdateSharedResource(num);
            done.Set();
            // Note: There could be additional work here, but the Set() method is used to indicate that
            //         the thread is done using the shared resource
        }

        // Method to modify the shared resource in a way that can cause a race condition
        private static void UpdateSharedResource(int num)
        {
            Console.WriteLine($"Start thread {num}...");
            int s = sharedRegister;
            Thread.Sleep(100);
            s++;
            sharedRegister = s;
            Console.WriteLine($"Thread {num} incremented shared register...");
            Console.WriteLine($"End thread {num}...");
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
