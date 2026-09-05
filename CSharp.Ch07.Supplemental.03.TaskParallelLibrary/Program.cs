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
using System.Threading;
using System.Threading.Tasks;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S125 // Allow commented out code in lessons
namespace CSharp.Ch07.Supplemental._03.TaskParallelLibrary
{
    internal static class Program
    {
        #region Private Constants
        // Number of times to process
        private const int NumberOfIterations = 32;
        #endregion

        #region Private Globals
        // Execution Timer
        private static Stopwatch sw;
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons (Task)
                // Perform operations sequentially (without Tasks)
                RunSequential();
                GenericFunctions.Pause();

                // Perform operations in parallel (with Tasks)
                RunTasks();
                GenericFunctions.Pause();

                // Perform operations in parallel (with Tasks) and ensure a correct result
                RunTasksCorrected();
                GenericFunctions.Pause();

                // Demonstrate using the TaskScheduler
                UsingScheduler();
                GenericFunctions.Pause();
                #endregion

                #region Chapter Lessons (Parallel)
                // Demonstrate a Parallel For loop
                RunParallelFor();
                GenericFunctions.Pause();

                // Demonstrate a Parallel For loop
                RunParallelForCorrected();
                GenericFunctions.Pause();
                #endregion

                #region Chapter Lessons (Continuation)
                // Run a set of tasks sequentially for comparison
                SequentialSteps();
                GenericFunctions.Pause();

                // Demonstrate Task Continuation
                StepsWithContinuation();
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

        #region Chapter Notes (Task)
        /*
         * The Task Parallel Library (TPL) introduces the "Task" as a unit of asynchronous work
         * This uses the ThreadPool but further abstracts the inner workings of the threads from the developer
         * It is, as the name implies, a library of functionality to enable processing multiple tasks in parallel
         *
         * TPL is the first introduction of robust asynchronous processing
         *
         * TPL Introduces TAP (Task-based Asynchronous Pattern) to replace the older models:
         * https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap
         *  - APM (Asynchronous Programming Model)
         *    https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm
         *    - Built on the IAsyncResult interface
         *    - Requires implementation of Begin<MethodName> and End<MethodName> methods
         *  - EAP (Event-based Asynchronous Pattern)
         *    https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap
         *    - Requires implementation of a <MethodName>Async method
         *    - Leveraged events, event handlers, and delegates
         *
         * TPL also introduces the "async" and "await" keywords (which further abstract Task creation)
         *
         * Using a task takes ont of two forms:
         *     Task                 // used when no return value is needed
         *     Task<TResult>        // used when a return is needed (specifies the return type expected)
         *
         * TPL also makes more complex threading designs relatively easy...
         *
         * The following methods are available from a Task:
         * - ContinueWith       // creates a new Task that starts when this Task completes
         * - Delay              // creates a Task that becomes completed after the specified delay
         * - Run                // adds a work request to the ThreadPool and returns the Task for that work
         * - Wait               // waits for the specified Task to complete (compare to the event handlers we used in ThreadPool)
         * - WaitAll            // waits for ALL specified Task(s) to complete
         * - WaitAny            // waits for the next Task among the specified Task(s) to complete
         * - WhenAll            // creates a Task that becomes completed after ALL specified Task(s) complete
         * - WhenAny            // creates a Task that becomes completed after any one of the specified Task(s) complete
         *
         * The following properties are exposed by a Task:
         * - CurrentId          // ID of the currently executing Task
         * - Exception          // when an unhandled AggregateException halts a Task, this exposes the exception
         * - Factory            // TaskFactory object for generating new Task(s)
         * - ID                 // ID of a specific Task instance
         * - IsCanceled         // becomes TRUE if the Task ends due to being canceled
         * - IsCompleted        // becomes TRUE when the Task completes
         * - IsFaulted          // becomes TRUE when the Task completes due to an unhandled exception
         * - Status             // returns the Task status
         * - Result             // gets the value returned by the Task's asynchronous operation
         *
         * TaskFactory exposes the following methods:
         * - ContinueWhenAll    // creates a new Task that starts when ALL specified Task(s) complete
         * - ContinueWhenAny    // creates a new Task that starts when ANY ONE of the specified Task(s) completes
         * - FromAsync          // wraps a Task around an asynchronous process built on the APM model
         * - StartNew           // creates and starts a new Task
         *
         * Because of this variety of functionality, there are multiple ways to generate a new Task
         * 1. Create a task directly (var t = new Task(<delegate>)) and start it (t.Start())
         * 2. Use the factory (TaskFactory.StartNew)
         * 3. Use the shorthand (Task.Run) which wraps TaskFactory.StartNew
         * 4. Use one of the continuation methods
         *    Task.WhenAll | .WhenAny
         *    TaskFactory.ContinueWhenAll | .ContinueWhenAny
         *
         * When using TaskFactory.StartNew, you can specify a number of options in addition to the method to run
         *   plus a cancellation token and scheduler. The options (which are bit-flags) include:
         * - PreferFairness     // sets a compiler hint to prefer (but not guarantee) first set, first run
         * - LongRunning        // sets a compiler hint to create extra threads to accomodate long-running Task(s)
         * - AttachedToParent   // indicates that the newly created Task is attached to the parent Task
         * - DenyChildAttach    // indicates that no child Task(s) can attach to the specified Task
         * - HideScheduler      // indicates that the scheduler specified for this Task should not be used for child Task(s)
         */
        #endregion

        #region Lesson Methods (Task)
        // Perform operations sequentially (without Tasks)
        private static void RunSequential()
        {
            Console.WriteLine("Processing sequentially...");

            Initialize();

            double result = 0d;

            for (int i = 0; i < NumberOfIterations; i++) result += Ch07SharedFunctions.DoIntensiveCalculations();

            Console.WriteLine($"Result: {result}");

            LogAndReset();
        }

        // Perform operations in parallel (with Tasks)
        private static void RunTasks()
        {
            Console.WriteLine("Processing with Task...");

            Initialize();

            double result = 0d;

            var tasks = new Task[NumberOfIterations];

            for (int i = 0; i < NumberOfIterations; i++)
            {
                tasks[i] = Task.Run(() => result += Ch07SharedFunctions.DoIntensiveCalculations());
            }

            Console.WriteLine($"{Environment.NewLine}Result: {result}");
            Console.WriteLine("We got the wrong result!");

            LogAndReset();
        }

        // Perform operations in parallel (with Tasks) and ensure a correct result
        private static void RunTasksCorrected()
        {
            Console.WriteLine("Processing with Task<double>...");

            Initialize();

            double result = 0d;

            // Let's make sure the tasks can return a value
            var tasks = new Task<double>[NumberOfIterations];

            for (int i = 0; i < NumberOfIterations; i++)
                // Note: We are only executing the method in the Task - we'll get the value later
                tasks[i] = Task.Run(Ch07SharedFunctions.DoIntensiveCalculations);
            // Note: This method group is equivalent to the explicit delegate call:

            //       tasks[i] = Task.Run(() => Ch07SharedFunctions.DoIntensiveCalculations());

            // We can wait for all the tasks to complete
            // Task.WaitAll(tasks);
            // But that is optional here, because "Wait" is implicit when we call Task<T>.Result below

            // Now, extract the data from the tasks' Result properties.
            foreach (var task in tasks)result += task.Result;
            // Although we haven't yet covered LINQ, this could be expressed as this LINQ query
            // result += tasks.Sum(task => task.Result);

            Console.WriteLine($"{Environment.NewLine}Result: {result}");

            LogAndReset();
        }

        // Demonstrate using the TaskScheduler
        private static void UsingScheduler()
        {
            /*
             * Section Notes:
             *
             * The primary use of the TaskScheduler (for the programmer at least) is for ensuring that
             *   Task(s) that need to update the UI (in forms or WFP) are executed by the UI thread.
             *
             * To do this, you pass a TaskScheduler argument to the StartNew or ContinueWith method that spawns the task
             *
             */
            Console.WriteLine("Example in separate Windows form.");
            new ParentForm().ShowDialog();
        }
        #endregion

        #region Chapter Notes (Parallel)
        /*
         * TPL also introduces the "Parallel" class as a mechanism for further abstracting multi-threaded processes.
         *
         * Parallel adds a layer on top of the "Task" object permitting ease of code implementation
         *   This takes the abstraction of threading one step further
         *
         * The following static methods are available from the "Parallel" class
         * - For                // enables the implementation of a "for" loop where the iterations run in parallel
         * - ForEach            // enables the implementation of a "foreach" loop where the iterations run in parallel
         *   - In "For" and "ForEach" loops, you can specify the ParallelLoopState, which exposes these methods:
         *     - Stop           // Stops all loop iterations
         *     - Break          // Stops all iterations higher than the current one
         * - Invoke             // accepts an array of delegates and executes them in parallel
         * * Note:  None of these methods guarantee parallel threads; they attempt this based on the state of the ThreadPool
         *      
         * - Syntax Examples:
         *    Parallel.For(min, max, delegate)
         *    Parallel.For(min, max, init, delegate_body, finally)
         */
        #endregion

        #region Lesson Methods (Parallel)
        // Execute the work using a Parallel For loop
        private static void RunParallelFor()
        {
            Console.WriteLine("Processing with Parallel For loop...");

            Initialize();

            double result = 0d;

            // Call the delegate multiple times in parallel using a For loop
            Parallel.For(0, NumberOfIterations,
                i => result += Ch07SharedFunctions.DoIntensiveCalculations());

            Console.WriteLine($"{Environment.NewLine}Result: {result}");
            Console.WriteLine("We got the wrong result!");

            LogAndReset();
        }

        // Execute the work using a Parallel For loop, correcting the result synchronization error
        private static void RunParallelForCorrected()
        {
            Console.WriteLine("Processing with Parallel For<TLocal> loop...");

            Initialize();

            double result = 0d;

            // Call the delegate multiple times in parallel using a For loop while stashing interim results
            // Leveraging the init/body/finally overload
            Parallel.For(0, NumberOfIterations,
                // Interim result = 0d
                () => 0d,

                //    result += Utils.CommonFunctions.DoIntensiveCalculations();
                (i, state, interimResult) => interimResult + Ch07SharedFunctions.DoIntensiveCalculations(),

                // Final step after the calculations 
                // we add the result to the final result
                (lastInterimResult) => result += lastInterimResult
            );

            Console.WriteLine($"{Environment.NewLine}Result: {result}");
            Console.WriteLine("This time we got the right result!");

            LogAndReset();
        }
        #endregion

        #region Lesson Methods (Continuation)
        // Run a set of tasks sequentially for comparison
        private static void SequentialSteps()
        {
            Console.WriteLine("Running steps sequentially...");
            Step(1);
            Step(2);
            Step(3);
        }

        // Demonstrate Task Continuation
        private static void StepsWithContinuation()
        {

            // Scenario 1: Steps 1, 2, and 3 are all independent
            Console.WriteLine("Steps 1, 2, and 3 are all independent");
            Parallel.Invoke(
                () => Step(1),
                () => Step(2),
                () => Step(3));
            GenericFunctions.Pause();

            // Scenario 2: Steps 1 and 2 are independent, but step 3 depends on step 1 completing
            Console.WriteLine("Steps 1 and 2 are independent, but step 3 depends on step 1 completing");
            Task task1 = Task.Run(() => Step(1));
            Task task2 = Task.Run(() => Step(2));
            // Here, task3 only begins as a continuation of task1
            Task task3 = task1.ContinueWith(antecedent => Step(3));
            // We don't have to wait for task 1, since task 3 only starts after it has finished
            Task.WaitAll(task2, task3);
            GenericFunctions.Pause();

            // Scenario 3: Steps 1 and 2 are independent, but step 3 depends on both steps 1 and 2 completing
            Console.WriteLine("Steps 1 and 2 are independent, but step 3 depends on both steps 1 and 2 completing");
            task1 = Task.Run(() => Step(1));
            task2 = Task.Run(() => Step(2));
            task3 = Task.Factory.ContinueWhenAll([task1, task2], antecedent => Step(3));
            // We only need to wait for task 3, since both tasks 1 and 2 implicitly wait before task 3 can begin
            task3.Wait();
            GenericFunctions.Pause();

            // Scenario 4: Steps 1 and 2 are independent, but step 3 depends on either step 1 or 2 completing
            Console.WriteLine("Steps 1 and 2 are independent, but step 3 depends on either step 1 or 2 completing");
            task1 = Task.Run(() => Step(1));
            task2 = Task.Run(() => Step(2));
            task3 = Task.Factory.ContinueWhenAny([task1, task2], antecedent => Step(3));
            // We don't know which task continues with task 3, so we wait for them all
            Task.WaitAll(task1, task2, task3);
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

        // Work simulator
        private static void Step(int num, int seconds = 2)
        {
            Console.WriteLine($"Step {num} start...");
            Thread.Sleep(seconds * 1000);
            Console.WriteLine($"Step {num} end...");
        }
        #endregion
    }
}
#pragma warning restore S125

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
