---
title: "async/await - Same Result, Different Calling Thread"
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
    private static int counter;

    private static double SimulateWork()
    {
        int instance = ++counter; // minor race on counter -- harmless for this lesson
        Console.WriteLine($"Start {instance}...");
        Thread.Sleep(1000);
        Console.WriteLine($"Stop {instance}...");
        return 1.0d;
    }

    // Returns a Task without using async -- async is for CONSUMING tasks, not producing them.
    private static Task<double> SimulateWorkAsync() => Task.Run(SimulateWork);

    // Uses async/await -- the compiler generates a continuation state machine.
    private static async Task<double> SimulateWorkAwait() => await Task.Run(SimulateWork);

    private static void Main()
    {
        var sw = Stopwatch.StartNew();

        // --- Sequential baseline (~2s) ---
        counter = 0;
        Console.WriteLine("=== Sequential ===");
        sw.Restart();
        Console.WriteLine(SimulateWork());
        Console.WriteLine(SimulateWork());
        Console.WriteLine($"Elapsed: {sw.Elapsed}\n");

        // --- Task + WaitAll (~1s): both tasks queue before either is waited on ---
        counter = 0;
        Console.WriteLine("=== Task + WaitAll ===");
        sw.Restart();
        Task[] tasks = [SimulateWorkAsync(), SimulateWorkAsync()];
        Task.WaitAll(tasks);
        Console.WriteLine($"Elapsed: {sw.Elapsed}\n");

        // --- async/await (~1s): same parallelism, different syntax ---
        // Task.Run wrapper is needed because Main() is not async.
        // Calling .Result directly can deadlock in contexts with a synchronization context.
        counter = 0;
        Console.WriteLine("=== async/await ===");
        sw.Restart();
        bool done = Task.Run(async () =>
        {
            Task<double>[] asyncTasks = [SimulateWorkAwait(), SimulateWorkAwait()];
            foreach (var t in asyncTasks) await t;
            return true;
        }).Result;
        Console.WriteLine($"Elapsed: {sw.Elapsed}\n");

        Console.WriteLine("WaitAll and async/await produce the same elapsed time in a console app.");
        Console.WriteLine("async/await's value is on UI/server threads where blocking has a cost.");
        Console.WriteLine("Task.Run(SimulateWork) moves a blocking Thread.Sleep to a pool thread.");
        Console.WriteLine("It does not make the I/O non-blocking -- it moves the block, not removes it.");
    }
}
```
