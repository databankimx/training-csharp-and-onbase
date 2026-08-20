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
using System.Diagnostics;
using System.Threading;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S125    // Allow commented code for lessons
#pragma warning disable S1192   // Ignore string literals for lessons
namespace CSharp.Ch07.MultithreadingAndAsynchronousProcessing
{
    /*
     * Chapter Notes:
     *
     * Because most modern CPUs have multiple cores, they are able to execute multiple operations simultaneously
     *
     * In order to take advantage of this, in C# you have the capability to create separate paths of execution
     *   for multiple processes. These separate paths are called "threads"
     *
     * The main advantage to the introduction of multi-threaded programming is that the developer can offload
     *   processes that take a long time to separate threads, preventing the user perceiving that the
     *   application has frozen while work is ongoing.
     *
     * You may hear multithreading described as the "fork/join pattern," which describes a process like this:
     * 1. A process is running but needs to perform some time-consuming work
     * 2. The process "forks," spawning a separate process to do that work, while the original process continues
     * 3. Once the main process reaches a point where the results from the forked process is required, it must wait
     *    for the thread to complete.
     * 4. After the forked thread completes its work, it and the main process "join," and subsequent processing continues
     * *  Note: It is possible to fork off multiple threads at the same time
     *
     * Side-Note: Threads in Windows
     * - Your application cannot dominate all of the available CPU cores, or the OS and other applications would stop working
     * - So Windows implements a thread scheduler, which controls the use of the CPU cores for processing
     *   The process works as follows:
     * 1. Every thread gets a priority assigned when it is created. A created thread is not automatically started;
     *    you have to do that.
     * 2. When a thread is started, it will be added on a queue with all the threads that can be run.
     * 3. The scheduler takes the thread with the highest priority on the queue, and it starts to run it.
     * 4. If several threads have the same priority, the scheduler schedules them in circular order (round robin).
     * 5. When the time allotted is up, the scheduler suspends the thread, adding it at the end of the queue.
     *    After that, it picks up a new thread to run.
     * 6. If there is no other thread with higher priority than the one just interrupted, that thread executes again.
     * 7. When a thread is blocked because it has to wait for an I/O operation, or for some other reasons such as locking,
     *    the thread will be removed from the queue and another thread will be scheduled to run.
     * 8. When the reason for blocking ends, the thread is added back in the queue to get a chance to run.
     * 9. When a thread finishes its work, the scheduler can pick another thread to run.
     *
     * Several threads are automatically created when a .NET application runs
     * - Garbage Collector
     * - Finalizer
     * - Main
     * - UI (if one exists - Forms, WPF, WUP, etc. - Does not spawn in a console application)
     *
     * There are a number of different threading models that have been added to the .NET framework over the years
     * - BackgroundWorker
     * - Thread
     * - ThreadPool
     * - Task Parallel Library (TPL)
     * - async/await
     *
     * Note that while you can (and should) create background threads, when all non-background threads (including Main)
     *   complete, the application ends, even if non-terminated background threads were still executing.
     *
     * There is some non-trivial overhead (thread-switching and memory use) involved in threading, so only use
     *   a multi-threaded design when there is an advantage because offloaded work is time-consuming.
     */

    internal static class Program
    {
        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                CodeLabUsingThreads();
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

        #region Code Labs
        // Perform task using selected threading method
        private static void CodeLabUsingThreads()
        {
            bool exiting = false;
            while (!exiting)
            {

                // Here we call different methods for different ways of running our application. 
                string userEntry = "";
                string[] validOptions = ["s", "t", "p", "e", "x"];
                while (string.IsNullOrEmpty(userEntry) || Array.IndexOf(validOptions, userEntry.Substring(0, 1).ToLower()) < 0)
                {
                    Console.Clear();
                    Console.WriteLine("Select a processing mode:");
                    Console.WriteLine("[S]equential | [T]hreaded | [P]ooled | [E]ventsInPool | E[x]it");
                    userEntry = Console.ReadLine();
                    if (string.IsNullOrEmpty(userEntry) || Array.IndexOf(validOptions, userEntry.Substring(0, 1).ToLower()) < 0)
                    {
                        Console.WriteLine($"[{userEntry}] is not a valid option!");
                        Console.WriteLine("Press <ENTER> to continue...");
                        Console.ReadLine();
                    }
                }

                // We are using Stopwatch to time the code
                var sw = Stopwatch.StartNew();

                switch (userEntry)
                {
                    case "t":
                        RunWithThreads();
                        break;
                    case "p":
                        RunInThreadPool();
                        break;
                    case "e":
                        RunInThreadPoolWithEvents();
                        break;
                    case "x":
                        exiting = true;
                        continue;
                    //case "s":
                    default:
                        RunSequential();
                        break;
                }

                // Print the time it took to run the application.
                Console.WriteLine("We're done in {0}!", sw.Elapsed);

                // Ch07SharedFunctions.WaitForKeyWhenDebugging() only pauses when a debugger
                //     is attached, which meant this result was invisible when run normally
                //     (including via LessonRunner), Console.Clear() at the top of the next
                //     loop iteration wiped it out instantly. Pause unconditionally instead.
                GenericFunctions.Pause();
            }
        }
        #endregion

        #region Code Lab Helper Functions
        // Performs process sequentially without any additional threading
        private static void RunSequential()
        {
            double result = 0d;

            // Call the function to read data from I/O
            result += Ch07SharedFunctions.SimulateReadDataFromIo();

            // Add the result of the second calculation 
            result += Ch07SharedFunctions.DoIntensiveCalculations();

            // Print the result
            Console.WriteLine("The result is {0}", result);
        }

        // Performs multi-threaded process using manually spawned threads
        private static void RunWithThreads()
        {
            /*
             * Section Notes:
             *
             * The process to create and use a thread looks like this:
             *
             * 1. Create the new thread and assign the work it will perform
             * 2. Start the thread (FORK)
             * 3. Do other work in the main process until the thread's results are needed
             * 4. Wait for the thread to finish (JOIN)
             * 5. Continue working in the main process
             *
             * Threads created in this fashion are foreground threads
             */

            double result = 0d;

            // Create the thread to read from I/O
            // Note that our callback here is an anonymous delegate
            var thread = new Thread(() => result = Ch07SharedFunctions.SimulateReadDataFromIo());

            // Start the thread
            thread.Start();

            // Save the result of the calculation into another variable
            double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

            // Wait for the thread to finish
            thread.Join();

            // Calculate the end result
            result += result2;

            // Print the result
            Console.WriteLine("The result is {0}", result);
        }

        // Performs multi-threaded process using the thread pool
        private static void RunInThreadPool()
        {
            /*
             * Section Notes:
             *
             * Using the TreadPool abstracts parts of thread creation and maintenance by creating a pool of
             *   usable threads that can be picked up for processing in the application
             *
             * The ThreadPool process looks like this:
             * 1. The program adds a work item to the thread pool
             * 2. If there is an idle thread, the work item is executed there
             * 3. If not (and assuming we're using fewer that the maximum available threads), a new background thread is created
             *    and the work item executed in the new thread
             *
             * An item is added to the pool as follows:
             *    ThreadPool.QueueUserWorkItem(delegate [, state]);
             *
             * Note: Because these are abstracted background threads, you cannot interrupt or join them or set their priority
             *       Pool threads persist for reuse until the application ends.
             */

            Console.WriteLine("Starting thread...");
            double result = 0d;

            // Create a work item to read from I/O
            ThreadPool.QueueUserWorkItem(x => result += Ch07SharedFunctions.SimulateReadDataFromIo());

            // Save the result of the calculation into another variable
            double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

            // Wait for the thread to finish
            // Note: Because we have no way to determine when the thread completes, we will get the wrong result here
            // Note: We will explore a way to indicate that a pooled thread finished executing in the next method

            // Calculate the end result
            // Note: The bad result occurs because this calculation takes place before the longer-running thread is complete,
            //       and at this point, result is still zero (0)
            result += result2;

            // Print the result
            Console.WriteLine("The result is {0}", result);
        }

        // Performs multi-threaded process while raising events to track thread completion
        private static void RunInThreadPoolWithEvents()
        {
            /*
             * Section Notes:
             *
             * Because you cannot directly interact with pooled threads, there is no "Join" method to use
             *   to await thread completion.
             *
             * This can be overcome by having the pooled thread trigger an event that can be monitored from the calling method.
             */

            double result = 0d;

            // We use this event to signal when the thread is done executing.
            var calculationDone = new EventWaitHandle(false, EventResetMode.AutoReset);
            
            // Add a work item to read from I/O to the thread pool
            // Note: This is an example of a modified closure. It's desirable for our example,
            //       but it could be very problematic in a real-world scenario
            ThreadPool.QueueUserWorkItem(x => {
                result += Ch07SharedFunctions.SimulateReadDataFromIo();

                // This call triggers the event that tells the waiting method that the thread has completed
                calculationDone.Set();
            });

            // Save the result of the calculation into another variable
            double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

            // The event handler exposes a method that allow us to wait until the thread sets it
            // This gives us a way to monitor thread completion behind the abstraction
            calculationDone.WaitOne();

            // Calculate the end result
            // Now we obtain a correct value, because we waited until the background thread completed
            result += result2;

            // Print the result
            Console.WriteLine("The result is {0}", result);
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
