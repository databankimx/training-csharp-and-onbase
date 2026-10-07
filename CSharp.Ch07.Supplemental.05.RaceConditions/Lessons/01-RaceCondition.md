---
title: "Race Condition - Silent Data Corruption"
chapter: 7
index: 1
dependencies: []
---

```csharp
using System;
using System.Threading;

internal static class Program
{
    private static int sharedRegister;

    // The 100ms sleep is the most important line. In a real increment, the race window is
    // nanoseconds -- possible to hit but hard to demonstrate. Widening it to 100ms makes
    // the collision fire every single time, making the bug reliable and observable.
    private static void UpdateSharedResource(int num)
    {
        Console.WriteLine($"Start thread {num}...");
        int s = sharedRegister;   // step 1: read
        Thread.Sleep(100);        // race window, held deliberately open
        s++;                      // step 2: add
        sharedRegister = s;       // step 3: write
        Console.WriteLine($"Thread {num} incremented shared register...");
        Console.WriteLine($"End thread {num}...");
    }

    private static void Main()
    {
        sharedRegister = 0;
        Console.WriteLine($"Start value: {sharedRegister}\n");

        var t1 = new Thread(() => UpdateSharedResource(1));
        var t2 = new Thread(() => UpdateSharedResource(2));
        t1.Start();
        t2.Start();
        t1.Join();
        t2.Join();

        Console.WriteLine($"\nExpected: 2");
        Console.WriteLine($"Actual:   {sharedRegister}");
        Console.WriteLine();
        Console.WriteLine("Both threads read 0, both computed 1, both wrote 1.");
        Console.WriteLine("Thread 1's increment was silently discarded. No crash. No exception.");
        Console.WriteLine("That is the defining characteristic of a race condition.");
    }
}
```
