---
title: "Thread Pool - Fast But Wrong"
chapter: 7
index: 3
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Threading;

internal static class Program
{
    private static double SimulateReadDataFromIo()
    {
        Thread.Sleep(2000);
        return 10d;
    }

    private static double DoIntensiveCalculations()
    {
        double result = 0;
        for (int i = 0; i < 10_000_000; i++)
            result += Math.Sqrt(i) * Math.Sin(i);
        return result;
    }

    private static void Main()
    {
        var sw = Stopwatch.StartNew();

        double result = 0d;

        // QueueUserWorkItem has no Join() equivalent -- you hand off the work and lose direct control.
        // The pool thread is a background thread; it may not have even started by the time we read result.
        ThreadPool.QueueUserWorkItem(x => result += SimulateReadDataFromIo());

        double result2 = DoIntensiveCalculations();

        // result is combined here before the pooled work item has necessarily finished.
        // Run this several times -- the I/O contribution will often be missing entirely.
        result += result2;

        Console.WriteLine($"Result:  {result:F4}   <-- this is often wrong");
        Console.WriteLine($"Elapsed: {sw.Elapsed}  <-- the elapsed time is correct even when the result isn't");
        Console.WriteLine();
        Console.WriteLine("Run this at least five times and count the wrong results.");
        Console.WriteLine("A bug that fails intermittently is far more dangerous than one that fails every time.");
    }
}
```
