# Chapter 7 Supplemental 09: Concurrent Collections

## What This Is

New content (not ported from an existing download), added to fill a gap identified by comparing this chapter against the full textbook topic outline: "Working with Concurrent Collections" is its own dedicated textbook subsection, but nothing in the existing chapter content touched on `System.Collections.Concurrent` at all. This project covers all five main types: `ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentStack`, `ConcurrentBag`, and `BlockingCollection`.

---

## The Problem These Solve

`List<T>`, `Dictionary<TKey, TValue>`, `Queue<T>`, and `Stack<T>` were all built for speed, not thread safety. Reading and writing one from multiple threads at once, without your own locking around it, risks corrupting its internal state or throwing partway through an operation. `System.Collections.Concurrent` provides drop-in, thread-safe replacements that handle that internally, no locking of your own required.

---

## `ConcurrentDictionary`: `AddOrUpdate` in One Atomic Step

```csharp
wordCounts.AddOrUpdate(word, 1, (key, existingValue) => existingValue + 1);
```

Ten threads, each incrementing a shared count for the same five keys, no lock anywhere, and every key ends up with exactly the expected count (10). `AddOrUpdate` is worth reading closely: it's specifically designed to replace the classic (and, on a plain `Dictionary`, thread-unsafe) pattern of "check if the key exists, then either add or update it", collapsing that check-then-act sequence into one atomic operation.

---

## `ConcurrentQueue` and `ConcurrentStack`: `TryDequeue`/`TryPop` Instead of Throwing

```csharp
while (!queue.TryDequeue(out _))
{
    Thread.Sleep(1);
}
```

```csharp
if (stack.TryPop(out _)) Interlocked.Increment(ref poppedCount);
```

Both follow the `IProducerConsumerCollection<T>` pattern: `Try`-prefixed methods that return `false` instead of throwing when the operation can't complete right now (an empty collection has nothing to dequeue or pop). This matters specifically in concurrent code, a consumer thread can easily race ahead of a producer thread and find the collection momentarily empty even though more items are still coming, `TryDequeue`/`TryPop` let that be a normal, handled case (loop and retry, or just skip) rather than an exception to catch.

---

## `ConcurrentBag`: When Order Genuinely Doesn't Matter

```csharp
Parallel.For(0, 10, i =>
{
    for (int j = 0; j < 10; j++) bag.Add(i * 10 + j);
});
```

`ConcurrentBag<T>` is unordered, items don't come back out in any particular sequence. That's a deliberate tradeoff: giving up ordering guarantees lets `ConcurrentBag` use per-thread local storage internally, making it the fastest of these collections specifically when many threads are both adding *and* removing items and the order genuinely doesn't matter.

---

## `BlockingCollection`: A Real Producer/Consumer Pattern

```csharp
var producer = Task.Run(() =>
{
    for (int i = 1; i <= 5; i++)
    {
        collection.Add(i);
        Thread.Sleep(500);
    }
    collection.CompleteAdding();
});

var consumer = Task.Run(() =>
{
    foreach (int item in collection.GetConsumingEnumerable())
    {
        Console.WriteLine($"Consumed item {item}...");
    }
});
```

This is the one genuinely different member of the group. `BlockingCollection<T>` wraps another concurrent collection (a `ConcurrentQueue<T>` by default) and adds actual blocking, `GetConsumingEnumerable()` doesn't poll or return early when nothing's available, it genuinely waits until the producer adds the next item. Run this and watch the "Consuming item N..." messages appear roughly 500ms apart, in step with the producer, not all at once.

`CompleteAdding()` is the other half of the pattern worth understanding: it tells the collection "no more items are coming." Without calling it, `GetConsumingEnumerable()`'s `foreach` loop would never end, it would sit waiting for one more item that will never arrive, even after the producer is done. `CompleteAdding()` lets the consumer's enumeration finish cleanly once the last buffered item has been consumed.

---

## What These Don't Solve

Worth being precise about the actual guarantee: these collections make individual operations (one `Add`, one `TryTake`, one `AddOrUpdate`) safe to call from any thread without extra locking. They do **not** make a *sequence* of operations atomic together. Checking `dictionary.Count` and then deciding whether to add something based on that count is still not safe, even on a `ConcurrentDictionary`, another thread can change the count between your check and your decision. The collections solve "don't corrupt my data structure", not "my multi-step business logic is automatically race-free."
