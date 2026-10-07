---
title: "Interlocked.Increment - No Lock Required"
chapter: 7
index: 1
dependencies: []
---

```csharp
using System;
using System.Threading;

internal static class Program
{
    private static void RunManyThreads(int threadCount, int incrementsPerThread, Action action)
    {
        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++) action();
            });
            threads[i].Start();
        }
        foreach (var t in threads) t.Join(); // retain handles, Join -- no guessed sleep
    }

    private static void Main()
    {
        const int threadCount = 100;
        const int incrementsPerThread = 1000;
        const int expected = threadCount * incrementsPerThread;

        // Unprotected: lost updates produce a plausible-looking number below 100,000.
        // The wrong result is usually close to correct -- that is why it goes uninvestigated.
        int unprotected = 0;
        RunManyThreads(threadCount, incrementsPerThread, () => unprotected++);
        Console.WriteLine($"Unprotected: expected {expected:N0}, actual {unprotected:N0}");

        // Interlocked.Increment: read-add-write as a single atomic CPU instruction.
        // Not a lock -- no thread is ever blocked, no try/finally required.
        int protected_ = 0;
        RunManyThreads(threadCount, incrementsPerThread, () => Interlocked.Increment(ref protected_));
        Console.WriteLine($"Interlocked: expected {expected:N0}, actual {protected_:N0}");

        Console.WriteLine();
        Console.WriteLine("Interlocked.Increment is not a faster lock -- it is not a lock at all.");
        Console.WriteLine("It protects one variable, one operation. Two calls in sequence are not atomic together.");
    }
}
```
