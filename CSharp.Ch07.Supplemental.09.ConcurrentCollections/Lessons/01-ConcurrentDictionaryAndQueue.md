---
title: "ConcurrentDictionary and ConcurrentQueue"
chapter: 7
index: 1
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
        // --- ConcurrentDictionary ---
        Console.WriteLine("=== ConcurrentDictionary ===");
        Console.WriteLine("Ten threads each incrementing a shared counter for five keys...");

        var wordCounts = new ConcurrentDictionary<string, int>();
        string[] words = ["apple", "banana", "cherry", "date", "elderberry"];

        Parallel.For(0, 10, _ =>
        {
            foreach (string word in words)
            {
                // AddOrUpdate is the atomic replacement for the unsafe check-then-add-or-update pattern.
                // The update delegate may run MORE THAN ONCE under contention (optimistic retry).
                // It must be side-effect free -- no logging, no I/O, no counters inside it.
                wordCounts.AddOrUpdate(word, 1, (key, existing) => existing + 1);
            }
        });

        foreach (var pair in wordCounts)
            Console.WriteLine($"  {pair.Key}: {pair.Value} (expected 10)");

        // --- ConcurrentQueue ---
        Console.WriteLine("\n=== ConcurrentQueue ===");
        Console.WriteLine("Five producers enqueue, five consumers dequeue...");

        var queue = new ConcurrentQueue<int>();
        int totalDequeued = 0;

        Parallel.Invoke(
            () => Parallel.For(0, 5, producer =>
            {
                for (int i = 0; i < 20; i++) queue.Enqueue(producer * 100 + i);
            }),
            () => Parallel.For(0, 5, _ =>
            {
                for (int i = 0; i < 20; i++)
                {
                    // TryDequeue returns false when empty rather than throwing.
                    // The API prevents the check-then-dequeue pattern that would be a race anyway.
                    while (!queue.TryDequeue(out _)) Thread.Sleep(1);
                    Interlocked.Increment(ref totalDequeued);
                }
            })
        );

        Console.WriteLine($"  Total dequeued: {totalDequeued} (expected 100)");
        Console.WriteLine();
        Console.WriteLine("Thread-safe operations do not compose into thread-safe transactions.");
        Console.WriteLine("Checking Count and then adding is still a race, even on ConcurrentDictionary.");
    }
}
```
