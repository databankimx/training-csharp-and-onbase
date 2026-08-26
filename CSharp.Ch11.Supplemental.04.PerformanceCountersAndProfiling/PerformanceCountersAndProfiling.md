# Performance Counters and Profiling

## Introduction

The main lesson showed the basic `Stopwatch` timing pattern. This lesson goes further: a real trap worth knowing about before it fools you, measuring memory alongside time, reading Windows' own performance counters, and when it's time to reach for a real profiling tool instead.

---

## Reading System Performance Counters

```csharp
var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
cpuCounter.NextValue();       // first reading, usually 0, ignore it
Thread.Sleep(1000);
float cpuUsage = cpuCounter.NextValue();   // this one's the real reading
```

This reads the exact same data Task Manager's Performance tab shows. The first reading from a counter like this is basically meaningless, always take a second reading after a short pause.

---

## A Real Trap: Timing Something Before It's "Warmed Up"

```csharp
// This first call includes one-time JIT compilation overhead
ComputeSomething();

// This second call is genuinely faster, purely because of that
ComputeSomething();
```

The first time any method runs, .NET compiles it to native code on the spot, which takes real time. If you time only the first call, you're measuring that one-time compilation cost, not the method's actual speed. Always run whatever you're timing once as a "throwaway" first, then start your stopwatch for real.

---

## Measuring Memory, Not Just Speed

```csharp
long before = GC.GetTotalMemory(forceFullCollection: true);
// ... do something ...
long after = GC.GetTotalMemory(forceFullCollection: true);
Console.WriteLine($"Allocated roughly: {after - before} bytes");
```

Useful for catching code that allocates way more memory than it should, treat the number as approximate, not exact.

---

## When to Use a Real Profiler Instead

Hand-timing with `Stopwatch` is great for comparing two specific approaches to the same problem. It's the wrong tool for a bigger question like "why is my whole app slow?" For that, a real profiler (built into Visual Studio, or a dedicated tool like dotTrace) watches your entire running application and shows you exactly where time is actually going, no guessing required.

---

## Try It Yourself

Run `HandProfilingWithWarmup()` and compare the "first call" and "second call" timings directly, the gap between them is entirely due to JIT compilation, not the code itself running any differently.
