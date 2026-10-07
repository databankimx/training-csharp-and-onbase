---
title: "Sequential - The Baseline"
chapter: 7
index: 1
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
        result += SimulateReadDataFromIo();
        result += DoIntensiveCalculations();

        Console.WriteLine($"Result:  {result:F4}");
        Console.WriteLine($"Elapsed: {sw.Elapsed}");
        Console.WriteLine();
        Console.WriteLine("This is the baseline. Total time = I/O time + calculation time.");
        Console.WriteLine("Note the elapsed time -- everything else in this chapter will beat it.");
    }
}
```
