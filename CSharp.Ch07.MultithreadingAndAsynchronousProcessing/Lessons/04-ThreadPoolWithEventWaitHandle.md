---
title: "Thread Pool With EventWaitHandle"
chapter: 7
index: 4
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

        // false  = start unsignaled (true would make WaitOne() return immediately, reintroducing the bug)
        // AutoReset = resets automatically after releasing one waiter (important if called in a loop)
        var calculationDone = new EventWaitHandle(false, EventResetMode.AutoReset);

        ThreadPool.QueueUserWorkItem(x =>
        {
            result += SimulateReadDataFromIo();
            calculationDone.Set();      // signal: I'm done
        });

        double result2 = DoIntensiveCalculations();

        calculationDone.WaitOne();      // block until the pool thread signals

        result += result2;

        Console.WriteLine($"Result:  {result:F4}   <-- always correct");
        Console.WriteLine($"Elapsed: {sw.Elapsed}  <-- same as the manual-thread version");
        Console.WriteLine();
        Console.WriteLine("Same pool as step 3. The EventWaitHandle restores the correctness guarantee");
        Console.WriteLine("that Join() provided, without needing a direct reference to the thread.");
    }
}
```
