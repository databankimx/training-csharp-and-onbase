# Chapter 11 Supplemental 04: Performance Counters and Profiling

## What This Is

The main lesson's `Stopwatch` pattern answers a narrow question well: "is THIS specific piece of code faster than THAT specific piece of code." This project goes deeper -- JIT warm-up and why it matters, measuring memory alongside time, reading system-wide performance counters, creating your own, and knowing when to stop hand-profiling and use a real profiler tool instead.

---

## How to Write This Program

### Mini-Program 1: Reading Built-In Performance Counters

Clear `Main()` and write:

```csharp
try
{
    using var cpuCounter    = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    using var memoryCounter = new PerformanceCounter("Memory", "Available MBytes");

    // The very first NextValue() on a rate-based counter like "% Processor Time" often
    // returns 0 -- it needs a baseline sample to compare against. Call it once,
    // wait briefly, then call it again for the real reading.
    cpuCounter.NextValue();
    Thread.Sleep(1000);

    float cpuUsage          = cpuCounter.NextValue();
    float availableMemoryMb = memoryCounter.NextValue();

    Console.WriteLine($"System-wide CPU usage: {cpuUsage:F1}%");
    Console.WriteLine($"Available memory:      {availableMemoryMb:N0} MB");
}
catch (Exception ex)
{
    Console.WriteLine($"Could not read performance counters: {ex.Message}");
    Console.WriteLine("(Some environments restrict access to the performance counter subsystem.)");
}

GenericFunctions.Pause();
```

Run it. These are the same numbers Task Manager and Performance Monitor (`perfmon.exe`) show -- `PerformanceCounter` reads from the same system-wide infrastructure.

The "call once, sleep, call again" pattern is required for rate-based counters. A counter like `% Processor Time` measures a rate over a time window, not a point-in-time value. The first call establishes the baseline; the second call, after at least one sample interval, produces a meaningful number. Snapshot counters (like `Available MBytes`) don't need this -- they reflect the current value at the moment of the call.

Reading built-in, already-installed counters requires no special privileges. Creating a new counter category does.

### Mini-Program 2: Creating a Custom Performance Counter

Clear `Main()` and write:

```csharp
const string categoryName = "CSharp.Ch11.Supplemental.04.Demo";
const string counterName  = "Items Processed";

try
{
    if (!PerformanceCounterCategory.Exists(categoryName))
    {
        var counterData = new CounterCreationDataCollection
        {
            new CounterCreationData(counterName, "Number of items processed",
                PerformanceCounterType.NumberOfItems32)
        };

        PerformanceCounterCategory.Create(categoryName, "Demonstration category",
            PerformanceCounterCategoryType.SingleInstance, counterData);
    }

    using var counter = new PerformanceCounter(categoryName, counterName, readOnly: false);
    counter.RawValue = 0;
    counter.Increment();
    counter.Increment();
    counter.Increment();

    Console.WriteLine($"Custom counter value: {counter.RawValue}");
    Console.WriteLine($"Open perfmon.exe, add a counter, look under \"{categoryName}\"");
    Console.WriteLine("to see this alongside every built-in system counter.");
}
catch (Exception ex)
{
    Console.WriteLine("Could not create the custom counter category.");
    Console.WriteLine($"(Creating a new category requires administrator privileges.): {ex.Message}");
}

GenericFunctions.Pause();
```

Run it -- as administrator if possible, otherwise the catch will explain why it failed.

The value proposition: your application's own metrics (orders processed per second, cache hit rate, items in queue) show up in `perfmon.exe` alongside CPU and memory, where ops teams already know how to look, without any custom tooling on their end.

The category must be created before counters within it can be used, and creation is a one-time setup step that needs admin privileges. `readOnly: false` is required to write to the counter; readers elsewhere use `readOnly: true`.

### Mini-Program 3: Hand-Profiling With JIT Warm-Up

Clear `Main()` and write:

```csharp
const int iterations = 50_000;

// The .NET JIT compiles a method to native code the FIRST time it actually runs.
// That first call is almost always slower than every subsequent call -- purely JIT
// overhead, nothing to do with the method's own logic.
var sw = Stopwatch.StartNew();
ComputeSomething(iterations: 1);
sw.Stop();
Console.WriteLine($"First call (includes JIT compilation): {sw.Elapsed.Ticks / 10.0:F1} microseconds");

sw.Restart();
ComputeSomething(iterations: 1);
sw.Stop();
Console.WriteLine($"Second call (already JIT-compiled):    {sw.Elapsed.Ticks / 10.0:F1} microseconds");

Console.WriteLine("\nThe first call is reliably slower. This is why profiling guidelines say:");
Console.WriteLine("run whatever you're timing once (throwaway warm-up), THEN start the stopwatch.");
Console.WriteLine("Skipping warm-up produces misleadingly pessimistic numbers.");

// Warm up, then measure for real.
ComputeSomething(iterations: 1);  // warm-up
sw.Restart();
ComputeSomething(iterations);
sw.Stop();
Console.WriteLine($"\n{iterations:N0} iterations, post-warm-up: {sw.ElapsedMilliseconds} ms total, " +
                  $"{(double)sw.ElapsedTicks / iterations:F2} ticks/iteration");

GenericFunctions.Pause();
```

Add the compute helper:

```csharp
private static long ComputeSomething(int iterations)
{
    long total = 0;
    for (int i = 0; i < iterations; i++)
        total += i * i;
    return total;
}
```

Run it. The first call is measurably slower. The second call, already JIT-compiled, is dramatically faster.

This is the most common mistake in hand-profiling: measuring the first execution and presenting it as representative. For anything measured only a small number of times -- especially when comparing two implementations -- the JIT warm-up cost can dominate and produce a result that says nothing meaningful about steady-state performance. Call the code once first, discard the result, then start the stopwatch.

### Mini-Program 4: Measuring Memory Allocation

Clear `Main()` and write:

```csharp
// GC.GetTotalMemory(true) forces a full garbage collection first, so the
// "before" reading reflects actual live memory, not just memory waiting to be collected.
long before = GC.GetTotalMemory(forceFullCollection: true);

var list = new List<string>();
for (int i = 0; i < 10_000; i++)
    list.Add($"Item {i}");

long after = GC.GetTotalMemory(forceFullCollection: true);

Console.WriteLine($"Approximate memory for 10,000-item string list: {(after - before):N0} bytes");
Console.WriteLine("\nTreat this as approximate, not exact. GC.GetTotalMemory() reflects the whole");
Console.WriteLine("managed heap -- background threads and runtime bookkeeping can shift the number.");
Console.WriteLine("Good enough to catch a genuinely wasteful allocation pattern; not precise");
Console.WriteLine("enough for exact byte counting.");

GC.KeepAlive(list);  // prevents the optimizer from eliminating the list as dead code

GenericFunctions.Pause();
```

Run it. The before/after difference reflects the approximate cost of building the list.

`forceFullCollection: true` is important for the "before" reading. Without it, the heap may contain memory eligible for collection but not yet swept -- the reading would be artificially high, making the allocation look larger than it is. The `forceFullCollection: false` variant is faster and appropriate for the "after" reading once you've already established a clean baseline.

`GC.KeepAlive(list)` prevents the optimizer from proving the list is dead code and eliminating it, which would make the allocation appear to cost nothing.

### Mini-Program 5: When to Use a Real Profiler

Clear `Main()` and write:

```csharp
Console.WriteLine("Stopwatch-based hand profiling answers a narrow question well:");
Console.WriteLine("  \"Is THIS specific piece of code faster than THAT specific piece?\"");
Console.WriteLine();
Console.WriteLine("It doesn't answer a broader one:");
Console.WriteLine("  \"Where, across my ENTIRE application, is time actually going?\"");
Console.WriteLine();
Console.WriteLine("A real profiler (Visual Studio Performance Profiler, JetBrains dotTrace,");
Console.WriteLine("or similar) instruments or samples an entire running application and produces");
Console.WriteLine("a call-tree breakdown: which methods were called how many times, how much");
Console.WriteLine("cumulative time each one (and everything it called) actually consumed.");
Console.WriteLine();
Console.WriteLine("Reach for a profiler specifically when the question is:");
Console.WriteLine("  \"Why is this slow?\" -- a profiler finds the actual bottleneck.");
Console.WriteLine("Use hand-profiling when the question is:");
Console.WriteLine("  \"Is A or B faster?\" -- you already know where to look.");

GenericFunctions.Pause();
```

Run it. The text is the lesson.

The distinction is worth internalizing. Hand-profiling is fast to set up and answers a targeted question, but it requires you to already suspect where the problem is. A profiler doesn't require a prior hypothesis -- it shows you the entire call tree and lets the data lead you to the bottleneck. When you genuinely don't know why something is slow, a profiler is the right tool; hand-profiling a wrong hypothesis is just fast confirmation of a wrong answer.

---

## Takeaways

- Rate-based performance counters need a baseline sample -- call `NextValue()` once, wait, then call it again.
- Reading built-in counters needs no privileges. Creating a new counter category needs admin.
- Custom performance counters expose your application's metrics to `perfmon.exe` alongside system counters.
- Always warm up the JIT before timing. The first call to any method includes compilation overhead.
- `GC.GetTotalMemory(forceFullCollection: true)` before a block establishes a clean memory baseline.
- `GC.KeepAlive()` prevents the optimizer from eliminating allocations the profiler is supposed to measure.
- Hand-profiling answers "is A faster than B." A real profiler answers "where is my application actually slow."
