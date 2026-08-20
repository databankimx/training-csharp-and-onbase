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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CSharp.Ch07.Supplemental._01.ThreadPoolExample.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch07.Supplemental._01.ThreadPoolExample
{
    internal static class Program
    {
        #region Private Members
        // Random number generator
        #pragma warning disable S2245 // Not concerned about cryptographic security in this example
        private static readonly Random Rand = new Random();
        #pragma warning restore S2245

        // List of active threads being tracked
        private static readonly List<ThreadTracker> Threads = new List<ThreadTracker>();

        // Number of threads to spawn
        private const int NumberOfThreads = 5;

        // Longest period (in seconds) to sleep (simulating work)
        private const int MaxSleep = 5;

        // Timer for thread completions
        private static Stopwatch sw;
        #endregion

        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                Console.WriteLine("Creating list of thread trackers...");
                CreateThreads();
                GenericFunctions.Pause();

                Console.WriteLine("Running operations in thread pool...");
                RunThreaded();
                GenericFunctions.Pause();

                Console.WriteLine("Running operations sequentially...");
                RunSequential();
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
        // Create and stage the tracking objects for the thread pool
        private static void CreateThreads()
        {
            try
            {
                for (int i = 1; i <= NumberOfThreads; i++)
                {
                    var thread = new ThreadTracker
                    {
                        Id = i,
                        Handle = new EventWaitHandle(false, EventResetMode.AutoReset),
                        SleepTime = Rand.Next(1, MaxSleep)
                    };
                    Console.WriteLine($"Created thread #{thread.Id}, which will execute for {thread.SleepTime} seconds...");
                    Threads.Add(thread);
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error creating threads!", ex);
            }
        }

        // Execute threads sequentially
        private static void RunSequential()
        {
            sw = Stopwatch.StartNew();
            try
            {
                foreach (var thread in Threads)
                {
                    Nap(thread);
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error running sequentially!", ex);
            }
            finally
            {
                Console.WriteLine($"Total run-time: {(double)sw.ElapsedMilliseconds / 1000} seconds...");
            }
        }

        // Execute threads in parallel
        private static void RunThreaded()
        {
            // Pre-populate the thread pool 
            ThreadPool.SetMinThreads(NumberOfThreads, NumberOfThreads);

            sw = Stopwatch.StartNew();
            try
            {
                // Add the threads to the thread pool
                foreach (var thread in Threads)
                {
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        Nap(thread);
                    });
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error running threads...", ex);
            }
            finally
            {
                // Wait for each thread to complete and raise it's event
                foreach (var thread in Threads)
                {
                    thread.Handle.WaitOne();
                    Console.WriteLine($"End thread {thread.Id}");
                }

                Console.WriteLine($"Total run-time: {(double)sw.ElapsedMilliseconds / 1000} seconds...");
            }
        }

        // Sleep to simulate work
        private static void Nap(ThreadTracker thread)
        {
            Console.WriteLine($"Starting thread {thread.Id}...");
            Thread.Sleep(thread.SleepTime * 1000);
            Console.WriteLine($"Thread {thread.Id} waited {thread.SleepTime} seconds...");
            thread.Handle.Set();
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
