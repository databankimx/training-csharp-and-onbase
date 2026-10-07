---
title: "Semaphore - N Concurrent Holders"
chapter: 7
index: 3
dependencies: []
---

```csharp
using System;
using System.Threading;

internal static class Program
{
    // Semaphore(0, 3): starts with 0 available slots, maximum 3.
    // Acts as a starting gate -- the main thread decides when the race begins.
    // Semaphore(3, 3) would let the first three threads through immediately on arrival.
    private static readonly Semaphore Pool = new Semaphore(0, 3);

    private static void ResourceWork(object num)
    {
        Console.WriteLine($"Thread {num} requesting access...");
        Pool.WaitOne();

        Console.WriteLine($"Thread {num} enters the semaphore...");
        Thread.Sleep(1000);

        // Release() returns the count BEFORE this release -- how many slots were free
        // just before this thread gave its back.
        Console.WriteLine($"Thread {num} releases (prev count: {Pool.Release()})...");

        // No thread affinity: any thread may call Release() regardless of which called WaitOne().
        // Calling Release() without a matching WaitOne() silently inflates capacity.
        // The only guard is your own discipline.
    }

    private static void Main()
    {
        Console.WriteLine("Spawning 5 threads -- semaphore allows 3 simultaneous...");

        for (int i = 0; i < 5; i++)
        {
            var t = new Thread(ResourceWork);
            t.Start(i + 1);
        }

        Thread.Sleep(1000); // let all 5 threads start and block on WaitOne()

        Console.WriteLine("\nMain thread releases 3 slots...");
        Pool.Release(3); // open the gate for 3 threads at once

        Thread.Sleep(5000); // let all threads complete
        Console.WriteLine("\nDone.");
        Console.WriteLine();
        Console.WriteLine("Use Semaphore to rate-limit access to a finite pool:");
        Console.WriteLine("  database connections, licence slots, outbound API calls.");
        Console.WriteLine("Use Monitor/lock for everything else.");
    }
}
```
