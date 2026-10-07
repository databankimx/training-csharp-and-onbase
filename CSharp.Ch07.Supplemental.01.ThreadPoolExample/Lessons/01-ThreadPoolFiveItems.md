---
title: "Scaling EventWaitHandle to Five Work Items"
chapter: 7
index: 1
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

// ThreadTracker bundles the three things QueueUserWorkItem can't take separately.
// Its entire reason for existing is that QueueUserWorkItem accepts only one object argument.
internal class ThreadTracker
{
    public int Id { get; set; }
    public EventWaitHandle Handle { get; set; }
    public int SleepTime { get; set; }
}

internal static class Program
{
    private const int NumberOfThreads = 5;
    private const int MaxSleep = 5;
    private static readonly Random Rand = new Random();
    private static readonly List<ThreadTracker> Threads = new List<ThreadTracker>();

    private static void Nap(ThreadTracker tracker)
    {
        Console.WriteLine($"Starting thread {tracker.Id}...");
        Thread.Sleep(tracker.SleepTime * 1000);
        Console.WriteLine($"Thread {tracker.Id} waited {tracker.SleepTime} seconds...");
        tracker.Handle.Set();
    }

    private static void Main()
    {
        // Create five trackers with random sleep times.
        // The SAME times are reused by both run strategies -- same data, different approaches.
        for (int i = 1; i <= NumberOfThreads; i++)
        {
            var tracker = new ThreadTracker
            {
                Id = i,
                Handle = new EventWaitHandle(false, EventResetMode.AutoReset),
                SleepTime = Rand.Next(1, MaxSleep)  // 1..4 inclusive (Next excludes upper bound)
            };
            Console.WriteLine($"Created thread #{tracker.Id}, will run for {tracker.SleepTime}s...");
            Threads.Add(tracker);
        }

        // --- Strategy 1: threaded ---
        Console.WriteLine("\n--- Threaded ---");
        // SetMinThreads defeats the pool's gradual ramp-up so all five start in parallel
        ThreadPool.SetMinThreads(NumberOfThreads, NumberOfThreads);
        var sw = Stopwatch.StartNew();
        try
        {
            foreach (var tracker in Threads)
                ThreadPool.QueueUserWorkItem(_ => Nap(tracker));
        }
        finally
        {
            // Wait in the finally block: even if queuing threw partway through,
            // already-running items keep executing and must be joined.
            foreach (var tracker in Threads)
            {
                tracker.Handle.WaitOne();
                Console.WriteLine($"End thread {tracker.Id}");
            }
            Console.WriteLine($"Threaded total: {(double)sw.ElapsedMilliseconds / 1000:F1}s  (expected ~longest sleep)");
        }

        // --- Strategy 2: sequential ---
        Console.WriteLine("\n--- Sequential ---");
        sw.Restart();
        foreach (var tracker in Threads)
            Nap(tracker);
        Console.WriteLine($"Sequential total: {(double)sw.ElapsedMilliseconds / 1000:F1}s  (expected ~sum of all sleeps)");
    }
}
```
