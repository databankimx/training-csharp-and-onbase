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
using System.Threading.Tasks;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch07.Supplemental._04.Asynchronicity
{
    internal static class Program
    {
        #region Private Globals
        // Execution Timer
        private static Stopwatch sw;

        private static int counter;
        #endregion

        #region Lesson Notes
        /*
         * Starting from C# 5.0, the "async" and "await" keywords were added to create a consistent,
         *   simple coding pattern for asynchronicity
         *
         * When a method is marked with the "async" keyword, it is limited to three return types:
         * - void
         * - Task
         * - Task<T>
         *
         * The reason for the return type being a Task rather than a normal value is to allow the method
         *   to be called with await
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                // Run sequentially for comparison
                SynchronousCalls();
                GenericFunctions.Pause();

                // Demonstrate an asynchronous method
                WaitingForAsyncTasks();
                GenericFunctions.Pause();

                // Demonstrate use of async - await
                // This syntax is a little funny - resynchronizing an async...
                bool done = Task.Run(async () => await AwaitingAsyncTasks()).Result;
                while (!done) Thread.Sleep(1);
                GenericFunctions.Pause();
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
        // Run sequentially for comparison
        private static void SynchronousCalls()
        {
            Initialize();
            counter = 0;
            Console.WriteLine(SimulateWork());
            Console.WriteLine(SimulateWork());
            LogAndReset();
        }

        // Demonstrate an asynchronous method
        private static void WaitingForAsyncTasks()
        {
            Initialize();
            counter = 0;
            Task[] tasks = [SimulateWorkAsync(), SimulateWorkAsync()];
            Task.WaitAll(tasks);
            LogAndReset();
        }

        // Demonstrate use of async - await
        private static async Task<bool> AwaitingAsyncTasks()
        {
            Initialize();
            counter = 0;
            Task<double>[] tasks = [SimulateWorkAwait(), SimulateWorkAwait()];
            foreach (var task in tasks) await task;
            LogAndReset();
            return true;
        }
        #endregion

        #region Helper Functions
        // Initialize the stopwatch
        private static void Initialize()
        {
            sw ??= new Stopwatch();
            sw.Start();
        }

        // Write the elapsed time and reset the stopwatch
        private static void LogAndReset()
        {
            if (sw == null) return;
            sw.Stop();
            Console.WriteLine($"Time Elapsed: {sw.Elapsed:c}");
            sw.Reset();
        }
        #endregion

        #region Naive Async Examples
        // Work Simulator
        private static double SimulateWork()
        {
            int instance = ++counter;
            Console.WriteLine($"Start {instance}...");
            Thread.Sleep(2000);
            Console.WriteLine($"Stop {instance}...");
            return 2.0d;
        }

        // Work Simulator (Asynchronous)
        private static Task<double> SimulateWorkAsync()
        {
            // You can create asynchronicity by returning a Task
            return Task.Run(SimulateWork);
        }

        // Work Simulator (async)
        private static async Task<double> SimulateWorkAwait()
        {
            // In a method using the "async" keyword, you return the result of the Task
            return await Task.Run(SimulateWork);

            // Note: Could have done this, but it would be silly
            // `return await SimulateWorkAsync();`
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
