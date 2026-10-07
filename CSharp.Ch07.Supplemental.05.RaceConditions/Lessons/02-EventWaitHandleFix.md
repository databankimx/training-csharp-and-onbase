---
title: "EventWaitHandle Fix - Correct But Not Concurrent"
chapter: 7
index: 2
dependencies: []
---

```csharp
using System;
using System.Threading;

internal static class Program
{
    private static int sharedRegister;

    private static void UpdateSharedResource(int num)
    {
        Console.WriteLine($"Start thread {num}...");
        int s = sharedRegister;
        Thread.Sleep(100);
        s++;
        sharedRegister = s;
        Console.WriteLine($"Thread {num} incremented shared register...");
        Console.WriteLine($"End thread {num}...");
    }

    private static void Main()
    {
        sharedRegister = 0;
        Console.WriteLine($"Start value: {sharedRegister}\n");

        foreach (int n in new[] { 1, 2 })
        {
            var done = new EventWaitHandle(false, EventResetMode.AutoReset);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                UpdateSharedResource(n);
                done.Set();
            });

            // WaitOne() is INSIDE the loop -- thread 2 is not queued until thread 1 is completely done.
            // The result is always 2, but only because the threads never actually run concurrently.
            done.WaitOne();
        }

        Console.WriteLine($"\nExpected: 2");
        Console.WriteLine($"Actual:   {sharedRegister}");
        Console.WriteLine();
        Console.WriteLine("Correct -- but not concurrent. The racy UpdateSharedResource was never fixed.");
        Console.WriteLine("If someone moved WaitOne() outside the loop as a 'performance improvement',");
        Console.WriteLine("the race would come straight back. The fix is structural, not this waiting pattern.");
    }
}
```
