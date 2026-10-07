---
title: "Task vs. Task<T> - Missing Wait vs. Race"
chapter: 7
index: 1
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

internal static class Program
{
    private const int Iterations = 16;

    private static double DoWork()
    {
        double r = 0;
        for (int i = 0; i < 5_000_000; i++) r += Math.Sqrt(i);
        return r;
    }

    private static void Main()
    {
        // --- Baseline: sequential ---
        var sw = Stopwatch.StartNew();
        double result = 0d;
        for (int i = 0; i < Iterations; i++) result += DoWork();
        Console.WriteLine($"Sequential:    {result:F0}  elapsed: {sw.Elapsed}");

        // --- Task (wrong): tasks run, but result is read before they finish ---
        sw.Restart();
        result = 0d;
        var tasksWrong = new Task[Iterations];
        for (int i = 0; i < Iterations; i++)
            tasksWrong[i] = Task.Run(() => result += DoWork()); // race on result +=
        // No wait here -- result is read immediately. Often 0 or partial.
        Console.WriteLine($"Task (wrong):  {result:F0}  elapsed: {sw.Elapsed}  <-- missing wait AND race");

        // --- Task<double> (correct): .Result blocks; accumulation is single-threaded after ---
        sw.Restart();
        result = 0d;
        var tasks = new Task<double>[Iterations];
        for (int i = 0; i < Iterations; i++)
            tasks[i] = Task.Run(DoWork);         // method group -- matches Func<double>
        foreach (var t in tasks) result += t.Result;  // .Result blocks until that task finishes
        Console.WriteLine($"Task<double>:  {result:F0}  elapsed: {sw.Elapsed}  <-- correct and faster");

        Console.WriteLine();
        Console.WriteLine("The broken Task[] version and the correct Task<double>[] version produce");
        Console.WriteLine("identically wrong-looking output. They are two different bugs:");
        Console.WriteLine("  broken Task[]:    missing wait (no WaitAll called)");
        Console.WriteLine("  Parallel.For bug: a true race on result += (see next step)");
    }
}
```
