---
title: "ConcurrentStack, ConcurrentBag, and BlockingCollection"
chapter: 7
index: 2
dependencies: []
---

```csharp
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

internal static class Program
{
    private static void Main()
    {
        // --- ConcurrentStack (LIFO) ---
        Console.WriteLine("=== ConcurrentStack ===");
        var stack = new ConcurrentStack<int>();
        Parallel.For(0, 100, stack.Push); // method group: Push matches Action<int>
        int popped = 0;
        Parallel.For(0, 100, _ => { if (stack.TryPop(out _)) Interlocked.Increment(ref popped); });
        Console.WriteLine($"Pushed 100, popped {popped} (expected 100), remaining {stack.Count} (expected 0)");

        // --- ConcurrentBag (unordered) ---
        Console.WriteLine("\n=== ConcurrentBag ===");
        // Per-thread local storage -- fastest when the SAME threads both add and take.
        // For strict producer/consumer (different threads add vs. take), use ConcurrentQueue instead.
        var bag = new ConcurrentBag<int>();
        Parallel.For(0, 10, i =>
        {
            for (int j = 0; j < 10; j++) bag.Add(i * 10 + j);
        });
        Console.WriteLine($"Bag contains {bag.Count} items (expected 100)");

        // --- BlockingCollection (producer/consumer with genuine blocking) ---
        Console.WriteLine("\n=== BlockingCollection ===");
        // Wraps ConcurrentQueue by default. Add blocking: consumer genuinely waits, not polls.
        // IDisposable because it holds wait handles internally -- must be disposed.
        using var collection = new BlockingCollection<int>();

        var producer = Task.Run(() =>
        {
            for (int i = 1; i <= 5; i++)
            {
                Console.WriteLine($"  Producing {i}...");
                collection.Add(i);
                Thread.Sleep(400);
            }
            // CompleteAdding() is not optional -- without it GetConsumingEnumerable() blocks
            // forever waiting for one more item that will never arrive. Same obligation as
            // countdown.Signal() or Monitor.Exit(): skip it and other threads hang silently.
            // In real code it belongs in a finally block.
            collection.CompleteAdding();
        });

        var consumer = Task.Run(() =>
        {
            foreach (int item in collection.GetConsumingEnumerable())
                Console.WriteLine($"  Consumed {item}...");
        });

        Task.WaitAll(producer, consumer);
        Console.WriteLine("Producer and consumer both finished.");
        Console.WriteLine();
        Console.WriteLine("Bounded capacity (new BlockingCollection<int>(10)) applies backpressure:");
        Console.WriteLine("Add() blocks when full, forcing a fast producer to match consumer pace.");
        Console.WriteLine("Unbounded queues between mismatched producers/consumers cause OOM at 3am.");
    }
}
```
