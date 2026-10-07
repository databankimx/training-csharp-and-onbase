---
title: "Task Continuations - Four Dependency Shapes"
chapter: 7
index: 3
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

internal static class Program
{
    private static void Step(int num, int seconds = 2)
    {
        Console.WriteLine($"Step {num} start...");
        Thread.Sleep(seconds * 1000);
        Console.WriteLine($"Step {num} end...");
    }

    private static void Main()
    {
        var sw = Stopwatch.StartNew();

        // Baseline: all sequential (~6s)
        Console.WriteLine("=== Sequential (baseline) ===");
        sw.Restart();
        Step(1); Step(2); Step(3);
        Console.WriteLine($"Elapsed: {sw.Elapsed}\n");

        // Scenario 1: all independent -- Parallel.Invoke runs all three at once (~2s)
        Console.WriteLine("=== All independent ===");
        sw.Restart();
        Parallel.Invoke(() => Step(1), () => Step(2), () => Step(3));
        Console.WriteLine($"Elapsed: {sw.Elapsed}\n");

        // Scenario 2: Step 3 depends on Step 1 only -- Steps 1+2 overlap; 3 starts after 1 (~4s)
        Console.WriteLine("=== Step 3 depends on Step 1 ===");
        sw.Restart();
        Task t1 = Task.Run(() => Step(1));
        Task t2 = Task.Run(() => Step(2));
        Task t3 = t1.ContinueWith(_ => Step(3));  // starts when t1 finishes
        Task.WaitAll(t2, t3);                      // t1 is implicitly covered by t3
        Console.WriteLine($"Elapsed: {sw.Elapsed}\n");

        // Scenario 3: Step 3 depends on BOTH 1 and 2 -- starts after the slower of the two (~4s)
        Console.WriteLine("=== Step 3 depends on Steps 1 AND 2 ===");
        sw.Restart();
        t1 = Task.Run(() => Step(1));
        t2 = Task.Run(() => Step(2));
        t3 = Task.Factory.ContinueWhenAll(new[] { t1, t2 }, _ => Step(3));
        t3.Wait();  // t1 and t2 are implicitly covered
        Console.WriteLine($"Elapsed: {sw.Elapsed}\n");

        // Scenario 4: Step 3 depends on EITHER 1 or 2 -- starts as soon as the first finishes (~4s)
        // The loser task is NOT in t3's dependency chain -- must be waited on separately.
        Console.WriteLine("=== Step 3 depends on Step 1 OR Step 2 ===");
        sw.Restart();
        t1 = Task.Run(() => Step(1));
        t2 = Task.Run(() => Step(2));
        t3 = Task.Factory.ContinueWhenAny(new[] { t1, t2 }, _ => Step(3));
        Task.WaitAll(t1, t2, t3);  // loser must be waited explicitly
        Console.WriteLine($"Elapsed: {sw.Elapsed}");

        Console.WriteLine("\nThe dependency graph sets the minimum runtime.");
        Console.WriteLine("No amount of parallelism beats the longest dependency chain.");
    }
}
```
