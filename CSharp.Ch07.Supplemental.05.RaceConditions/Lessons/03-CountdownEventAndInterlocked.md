---
title: "CountdownEvent + Interlocked - Correct and Concurrent"
chapter: 7
index: 3
dependencies: []
---

```csharp
using System;
using System.Threading;

internal static class Program
{
    private static int sharedRegister;

    // UpdateSharedResource (the racy version) is GONE. Replaced with a genuinely atomic operation.
    // CountdownEvent answers "is everyone done?" -- it does not stop two threads touching
    // sharedRegister at the same time. Interlocked.Increment handles that separately.
    // Both tools are needed because they solve two different problems.
    private static void UpdateSharedResourceWithCountdown(int num, CountdownEvent countdown)
    {
        Console.WriteLine($"Start thread {num}...");
        Thread.Sleep(100);
        Interlocked.Increment(ref sharedRegister);  // read-add-write as one atomic CPU instruction
        Console.WriteLine($"Thread {num} incremented shared register...");
        Console.WriteLine($"End thread {num}...");
        countdown.Signal();                          // decrement the countdown
    }

    private static void Main()
    {
        sharedRegister = 0;
        Console.WriteLine($"Start value: {sharedRegister}\n");

        // Start count = 2, one Signal() per thread
        var countdown = new CountdownEvent(2);

        // Queue both work items BEFORE waiting -- this is what preserves the concurrency
        foreach (int n in new[] { 1, 2 })
            ThreadPool.QueueUserWorkItem(_ => UpdateSharedResourceWithCountdown(n, countdown));

        // Wait() is OUTSIDE the loop -- both threads run genuinely in parallel
        countdown.Wait();

        Console.WriteLine($"\nExpected: 2");
        Console.WriteLine($"Actual:   {sharedRegister}");
        Console.WriteLine();
        Console.WriteLine("Both 'Start thread N...' lines appear before either 'End thread N...' line --");
        Console.WriteLine("the threads really are concurrent.");
        Console.WriteLine();
        Console.WriteLine("CountdownEvent = correct timing (know when everyone is done)");
        Console.WriteLine("Interlocked    = correct data (increments can't be lost)");
        Console.WriteLine("One without the other still fails -- they solve different problems.");
    }
}
```
