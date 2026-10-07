---
title: "Fork/Join With a Manual Thread"
chapter: 7
index: 2
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

        // 1. Create the thread -- the lambda is a closure that writes to result
        var thread = new Thread(() => result = SimulateReadDataFromIo());

        // 2. FORK -- I/O work starts on the new thread
        thread.Start();

        // 3. Do calculations on the main thread while the forked thread sleeps
        double result2 = DoIntensiveCalculations();

        // 4. JOIN -- block until the forked thread finishes before reading result
        thread.Join();

        // 5. Combine -- both writes are done, safe to add
        result += result2;

        Console.WriteLine($"Result:  {result:F4}");
        Console.WriteLine($"Elapsed: {sw.Elapsed}");
        Console.WriteLine();
        Console.WriteLine("Elapsed is now ~max(I/O, calculation), not their sum.");
        Console.WriteLine("Remove the Join() and the result will often be wrong.");
        Console.WriteLine("The two threads wrote to DIFFERENT variables -- that's what makes it safe.");
    }
}
```
