---
title: "Parallel.For - Race vs. Thread-Local Accumulation"
chapter: 7
index: 2
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
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
        // --- Parallel.For (wrong): Parallel.For DOES wait for all iterations.
        //     This is not a missing-wait bug. It is a true race condition.
        //     Multiple threads do "result +=" simultaneously: read, add, write.
        //     Two threads can both read the same value before either writes back.
        //     Fixing the wait would change nothing -- the shared += is the problem.
        var sw = Stopwatch.StartNew();
        double result = 0d;
        Parallel.For(0, Iterations, i => result += DoWork());
        Console.WriteLine($"Parallel.For (wrong):    {result:F0}  elapsed: {sw.Elapsed}  <-- race on result +=");

        // --- Parallel.For<TLocal> (correct): each participating thread gets its own
        //     private accumulator. The shared result is only touched once per thread,
        //     in the localFinally delegate, serialising the merge step entirely.
        sw.Restart();
        result = 0d;
        Parallel.For(
            0, Iterations,
            () => 0d,                                                    // thread-local init
            (i, state, localResult) => localResult + DoWork(),           // per-iteration body (pure -- no shared writes)
            localResult => result += localResult                          // per-thread merge (runs once per thread)
        );
        Console.WriteLine($"Parallel.For<TLocal>:    {result:F0}  elapsed: {sw.Elapsed}  <-- correct");

        Console.WriteLine();
        Console.WriteLine("Both broken versions look the same on screen but are DIFFERENT bugs:");
        Console.WriteLine("  Parallel.For wrong: a race -- result += is not atomic");
        Console.WriteLine("  Task wrong (step 1): a missing wait -- result is read too early");
        Console.WriteLine("Fixing one bug's symptom does not fix the other bug.");
    }
}
```
