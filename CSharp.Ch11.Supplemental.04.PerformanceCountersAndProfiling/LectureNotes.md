# Chapter 11 Supplemental 04: Performance Counters and Profiling

## What This Is

The main lesson's `ProfilingByHand()` showed the basic `Stopwatch` pattern. This Supplemental goes deeper: the JIT warm-up pitfall (a genuine, common source of misleading hand-timing results), measuring memory alongside time, `PerformanceCounter` (reading the same system-wide counters Task Manager and Performance Monitor read from, and creating your own), and when a real profiler tool is worth reaching for instead of any of this.

---

## Reading Built-In Performance Counters

```csharp
using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
cpuCounter.NextValue();      // first call, often returns 0
Thread.Sleep(1000);
float cpuUsage = cpuCounter.NextValue();   // second call, reflects real usage
```

Worth knowing about specifically: a rate-based counter like `% Processor Time` needs a baseline sample to compare against, its *first* `NextValue()` call typically returns `0`, not a real reading. The pattern is always: sample once, wait briefly, sample again, the second sample is the meaningful one. No administrator privileges are needed to *read* built-in, already-installed counters like this, they're the exact same data Task Manager's own Performance tab displays.

---

## Custom Performance Counters: The Same Permission Boundary as `EventLog`

```csharp
if (!PerformanceCounterCategory.Exists(categoryName))
{
    var counterData = new CounterCreationDataCollection
    {
        new CounterCreationData(counterName, "...", PerformanceCounterType.NumberOfItems32)
    };
    PerformanceCounterCategory.Create(categoryName, "...", PerformanceCounterCategoryType.SingleInstance, counterData);
}
```

A real application can expose its *own* metrics ("orders processed per second", "queue depth") through the exact same Performance Monitor infrastructure built-in counters use, genuinely useful for production monitoring. But creating a *new category* the first time requires administrator privileges, the same permission boundary already covered for `EventLog.CreateEventSource()` in the main lesson, writing to an *existing* category doesn't need elevation, only creating a brand-new one does. This method is wrapped in `try`/`catch` for the same reason the main lesson's `LoggingToEventLog()` was, so running without administrator rights degrades gracefully instead of crashing outright.

---

## The JIT Warm-Up Pitfall

```csharp
var stopwatch = Stopwatch.StartNew();
ComputeSomething(iterations: 1);   // includes JIT compilation overhead
stopwatch.Stop();
// ... time it again ...
ComputeSomething(iterations: 1);   // already JIT-compiled, genuinely faster
```

.NET's JIT compiler compiles a method to native code the *first time it actually runs*, not ahead of time. That means a method's very first call is almost always measurably slower than every call after it, purely from compilation overhead, nothing to do with the method's own logic. Timing only the first call produces a misleadingly pessimistic number. The general rule worth internalizing: run whatever you're timing once, throwaway, specifically to "warm up" the JIT, *then* start the stopwatch for the timing that actually matters. This is easy to skip by accident when hand-profiling something measured only once or twice, and a genuine, common source of misleading results.

---

## Measuring Memory, Not Just Time

```csharp
long before = GC.GetTotalMemory(forceFullCollection: true);
// ... do something that allocates ...
long after = GC.GetTotalMemory(forceFullCollection: true);
Console.WriteLine($"Approximate bytes allocated: {after - before:N0}");
```

`forceFullCollection: true` matters here: without it, the "before" and "after" readings could include memory that's already garbage (eligible for collection) but hasn't actually been swept yet, skewing the difference. Worth treating this technique as *approximate*, not exact, `GC.GetTotalMemory()` reflects the whole managed heap, and other concurrent activity (background threads, the runtime's own bookkeeping) can shift the number slightly between the two calls. Good enough to catch a genuinely wasteful allocation pattern (an operation that allocates far more than expected), not precise enough for exact byte-level accounting.

---

## When Hand-Profiling Isn't Enough

`Stopwatch`-based hand profiling (everything above, and the main lesson's `ProfilingByHand()`) answers a narrow question well: "is *this specific* piece of code faster than *that specific* piece of code." It doesn't answer a broader one: "where, across an entire application, is time actually going." A real profiler (Visual Studio's built-in Performance Profiler, JetBrains dotTrace, or similar) instruments or samples a whole running application and produces a call-tree breakdown, which methods were called how many times, how much *cumulative* time each one (and everything it in turn called) actually consumed. Worth reaching for specifically when the question itself is genuinely "why is this slow," not yet "is A or B faster," a profiler finds the actual bottleneck for you; hand-profiling only confirms or refutes a bottleneck you already suspected going in.
