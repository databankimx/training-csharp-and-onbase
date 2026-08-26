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
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch11.Supplemental._04.PerformanceCountersAndProfiling
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main lesson's ProfilingByHand() showed the basic Stopwatch pattern. This
         *   Supplemental goes deeper on hand-profiling (JIT warm-up, why it matters, and
         *   measuring memory alongside time), covers PerformanceCounter (reading the
         *   SAME system-wide counters Windows' own Performance Monitor / Task Manager
         *   read from), and explains, without being able to demonstrate live, when a real
         *   profiler tool is worth reaching for instead of either of these techniques.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                ReadingBuiltInPerformanceCounters();
                GenericFunctions.Pause();

                CreatingACustomPerformanceCounter();
                GenericFunctions.Pause();

                HandProfilingWithWarmup();
                GenericFunctions.Pause();

                MeasuringMemoryAllocation();
                GenericFunctions.Pause();

                WhenToUseARealProfiler();
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
        // PerformanceCounter: reading the SAME system-wide counters Task Manager and
        //   Performance Monitor read from, no admin privileges needed for READING
        //   built-in, already-installed counters
        private static void ReadingBuiltInPerformanceCounters()
        {
            try
            {
                using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                using var memoryCounter = new PerformanceCounter("Memory", "Available MBytes");

                // The very first NextValue() call on a counter like this one often returns
                //   0, it needs a baseline sample to compare against for a rate-based
                //   counter like "% Processor Time", the SECOND call (after a brief pause)
                //   is the one that actually reflects real, current usage.
                cpuCounter.NextValue();
                Thread.Sleep(1000);

                float cpuUsage = cpuCounter.NextValue();
                float availableMemoryMb = memoryCounter.NextValue();

                Console.WriteLine($"System-wide CPU usage: {cpuUsage:F1}%");
                Console.WriteLine($"Available memory: {availableMemoryMb:N0} MB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not read built-in performance counters: {ex.Message}");
                Console.WriteLine("(Some environments restrict access to the performance counter subsystem entirely.)");
            }
        }

        // Custom performance counters: genuinely useful for a real application (exposing
        //   your OWN metrics, like "orders processed per second", to the same Performance
        //   Monitor tooling built-in counters use), but CREATING a new category requires
        //   administrator privileges, same permission boundary as EventLog.CreateEventSource()
        private static void CreatingACustomPerformanceCounter()
        {
            const string categoryName = "CSharp.Ch11.Supplemental.04.Demo";
            const string counterName = "Items Processed";

            try
            {
                if (!PerformanceCounterCategory.Exists(categoryName))
                {
                    var counterData = new CounterCreationDataCollection
                    {
                        new CounterCreationData(counterName, "Number of items processed", PerformanceCounterType.NumberOfItems32)
                    };

                    PerformanceCounterCategory.Create(categoryName, "Demonstration category", PerformanceCounterCategoryType.SingleInstance, counterData);
                }

                using var counter = new PerformanceCounter(categoryName, counterName, readOnly: false);
                counter.RawValue = 0;
                counter.Increment();
                counter.Increment();
                counter.Increment();

                Console.WriteLine($"Created (or reused) a custom performance counter category, current value: {counter.RawValue}");
                Console.WriteLine("Open Windows' Performance Monitor (perfmon.exe), add a counter, and look under");
                Console.WriteLine($"\"{categoryName}\" to see this one alongside every built-in system counter.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not create the custom performance counter category (creating a new category");
                Console.WriteLine($"requires administrator privileges): {ex.Message}");
            }
        }

        // Hand-profiling, done more carefully: the JIT warm-up pitfall
        private static void HandProfilingWithWarmup()
        {
            const int iterations = 50_000;

            // The .NET JIT compiler compiles a method to native code the FIRST time it
            //   actually runs, not ahead of time. That means a method's very first call is
            //   almost always slower than every call after it, purely due to JIT
            //   compilation overhead, nothing to do with the method's own logic. Timing
            //   the FIRST call specifically produces a misleadingly pessimistic number.
            var stopwatch = Stopwatch.StartNew();
            ComputeSomething(iterations: 1);
            stopwatch.Stop();
            Console.WriteLine($"First call (includes JIT compilation overhead): {stopwatch.Elapsed.Ticks / 10.0:F1} microseconds");

            stopwatch.Restart();
            ComputeSomething(iterations: 1);
            stopwatch.Stop();
            Console.WriteLine($"Second call (already JIT-compiled): {stopwatch.Elapsed.Ticks / 10.0:F1} microseconds");

            Console.WriteLine($"{Environment.NewLine}Worth internalizing as a general profiling rule: run whatever you're timing once,");
            Console.WriteLine("throwaway, to \"warm up\" the JIT, THEN start the stopwatch for the timing that");
            Console.WriteLine("actually matters. Skipping warm-up is a genuine, common source of misleading");
            Console.WriteLine("hand-profiling results, especially for something measured only once or twice.");

            stopwatch.Restart();
            ComputeSomething(iterations);
            stopwatch.Stop();
            Console.WriteLine($"{Environment.NewLine}{iterations:N0} iterations, post-warm-up: {stopwatch.ElapsedMilliseconds} ms total, {(double)stopwatch.ElapsedTicks / iterations:F2} ticks/iteration average");
        }

        #pragma warning disable S3241 // Allow unused return value in lesson
        private static long ComputeSomething(int iterations)
        #pragma warning restore S3241
        {
            long total = 0;
            for (int i = 0; i < iterations; i++)
            {
                total += i * i;
            }
            return total;
        }

        // Measuring memory allocation alongside time
        private static void MeasuringMemoryAllocation()
        {
            // GC.GetTotalMemory(forceFullCollection: true) forces a garbage collection
            //   first, so the "before" and "after" readings reflect actual LIVE memory,
            //   not memory that's merely eligible for collection but hasn't been swept yet.
            #pragma warning disable S1215 // Using GetTotalMemory as a lesson topic
            long before = GC.GetTotalMemory(forceFullCollection: true);

            var list = new System.Collections.Generic.List<string>();
            for (int i = 0; i < 10_000; i++)
            {
                list.Add($"Item {i}");
            }

            long after = GC.GetTotalMemory(forceFullCollection: true);
            #pragma warning restore S1215

            Console.WriteLine($"Approximate memory allocated by building the list: {(after - before):N0} bytes");
            Console.WriteLine($"{Environment.NewLine}Worth treating this as approximate, not exact, GC.GetTotalMemory() reflects the");
            Console.WriteLine("WHOLE managed heap, other allocations happening concurrently (background threads,");
            Console.WriteLine("the runtime's own bookkeeping) can shift the number slightly. Good enough to catch");
            Console.WriteLine("a genuinely wasteful allocation pattern, not precise enough for exact byte counting.");

            GC.KeepAlive(list);
        }

        // When hand-profiling isn't enough, and a real profiler tool is worth reaching for
        private static void WhenToUseARealProfiler()
        {
            Console.WriteLine("Stopwatch-based hand profiling (like the methods above) answers a narrow question well:");
            Console.WriteLine("\"is THIS specific piece of code faster than THAT specific piece of code.\" It doesn't");
            Console.WriteLine("answer a broader one: \"where, across my ENTIRE application, is time actually going.\"");
            Console.WriteLine();
            Console.WriteLine("A real profiler (Visual Studio's built-in Performance Profiler, JetBrains dotTrace,");
            Console.WriteLine("or similar) instruments or samples an entire running application and produces a");
            Console.WriteLine("call-tree breakdown: which methods were called how many times, how much CUMULATIVE");
            Console.WriteLine("time each one (and everything it called) actually consumed. Worth reaching for");
            Console.WriteLine("specifically when the QUESTION itself is \"why is this slow,\" not yet \"is A or B");
            Console.WriteLine("faster,\" a profiler finds the actual bottleneck for you, hand-profiling only");
            Console.WriteLine("confirms or refutes a bottleneck you already suspected.");
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
