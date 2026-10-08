---
title: "Performance Counters, JIT Warm-Up, GC Memory, and When to Use a Real Profiler"
chapter: 11
index: 1
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Threading;

internal static class Program
{
    private static void Main()
    {
        // --- Built-in performance counters ---
        // Same numbers Task Manager and perfmon.exe show -- same infrastructure.
        // Rate-based counters (% Processor Time) need a baseline sample:
        // call NextValue() once to establish the baseline, wait, then call again.
        // Snapshot counters (Available MBytes) don't need this.
        Console.WriteLine("Built-in performance counters:");
        try
        {
            using var cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            using var mem = new PerformanceCounter("Memory", "Available MBytes");
            cpu.NextValue();         // baseline -- first reading is always 0 for rate counters
            Thread.Sleep(1000);
            Console.WriteLine($"  CPU usage:        {cpu.NextValue():F1}%");
            Console.WriteLine($"  Available memory: {mem.NextValue():N0} MB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Could not read performance counters: {ex.Message}");
        }

        // --- Custom performance counter ---
        // Creating a new category needs admin. Reading and writing to an existing one doesn't.
        // Value proposition: your app's metrics appear in perfmon.exe alongside CPU/memory,
        // with no custom tooling needed on the ops side.
        const string category = "CSharp.Ch11.Supplemental.04.Demo";
        const string counter  = "Items Processed";
        Console.WriteLine("\nCustom performance counter:");
        try
        {
            if (!PerformanceCounterCategory.Exists(category))
            {
                var data = new CounterCreationDataCollection
                {
                    new CounterCreationData(counter, "Demo counter",
                        PerformanceCounterType.NumberOfItems32)
                };
                PerformanceCounterCategory.Create(category, "Demo",
                    PerformanceCounterCategoryType.SingleInstance, data);
            }
            using var c = new PerformanceCounter(category, counter, readOnly: false);
            c.RawValue = 0;
            c.Increment(); c.Increment(); c.Increment();
            Console.WriteLine($"  Counter value: {c.RawValue}");
            Console.WriteLine($"  Visible in perfmon.exe under \"{category}\".");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Could not create counter category (needs admin): {ex.Message}");
        }

        // --- JIT warm-up ---
        // The JIT compiles a method to native code on its FIRST call.
        // That first call is reliably slower -- pure JIT overhead, not the method's logic.
        // Always call the code once (throwaway warm-up) BEFORE starting the Stopwatch.
        Console.WriteLine("\nJIT warm-up:");
        var sw = Stopwatch.StartNew();
        Compute(1);
        sw.Stop();
        long firstTicks = sw.ElapsedTicks;

        sw.Restart();
        Compute(1);
        sw.Stop();
        long secondTicks = sw.ElapsedTicks;

        Console.WriteLine($"  First call  (includes JIT): {firstTicks,8} ticks");
        Console.WriteLine($"  Second call (already JIT'd): {secondTicks,8} ticks");
        Console.WriteLine("  First call is reliably slower. Warm up before timing.");

        Compute(1); // warm-up
        const int iterations = 50_000;
        sw.Restart();
        Compute(iterations);
        sw.Stop();
        Console.WriteLine($"  {iterations:N0} iterations post-warm-up: {sw.ElapsedMilliseconds} ms " +
                          $"({(double)sw.ElapsedTicks / iterations:F2} ticks/iter)");

        // --- GC memory measurement ---
        // forceFullCollection: true on the BEFORE reading ensures the baseline reflects
        // actual live memory, not memory waiting to be collected.
        // GC.KeepAlive() prevents the optimizer from proving the list is dead code
        // and eliminating the allocation -- which would make it appear to cost nothing.
        Console.WriteLine("\nGC memory measurement (approximate):");
        long before = GC.GetTotalMemory(forceFullCollection: true);
        var list = new System.Collections.Generic.List<string>();
        for (int i = 0; i < 10_000; i++) list.Add($"Item {i}");
        long after = GC.GetTotalMemory(forceFullCollection: true);
        GC.KeepAlive(list);
        Console.WriteLine($"  10,000-item string List<T>: ~{after - before:N0} bytes");
        Console.WriteLine("  Treat as approximate -- background threads shift the number.");

        // --- When to use a real profiler ---
        Console.WriteLine("\nStopwatch answers: \"Is A faster than B?\" (you already know where to look).");
        Console.WriteLine("A real profiler (VS Performance Profiler, dotTrace) answers:");
        Console.WriteLine("  \"Where across my ENTIRE application is time actually going?\"");
        Console.WriteLine("When you don't know why something is slow, a profiler finds the bottleneck.");
        Console.WriteLine("Hand-profiling requires a prior hypothesis; a profiler doesn't.");
    }

    private static long Compute(int iterations)
    {
        long total = 0;
        for (int i = 0; i < iterations; i++) total += i * i;
        return total;
    }
}
```
