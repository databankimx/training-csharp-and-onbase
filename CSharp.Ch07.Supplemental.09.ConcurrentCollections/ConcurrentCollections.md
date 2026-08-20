# Concurrent Collections

## Introduction

The everyday collection types, `List<T>`, `Dictionary<TKey, TValue>`, `Queue<T>`, `Stack<T>`, are fast but not thread-safe. Using one from multiple threads at once, without your own locking, can corrupt it or throw an exception partway through an operation. `System.Collections.Concurrent` provides thread-safe versions that handle that internally.

---

## `ConcurrentDictionary`

```csharp
var wordCounts = new ConcurrentDictionary<string, int>();

wordCounts.AddOrUpdate("apple", 1, (key, existingValue) => existingValue + 1);
```

`AddOrUpdate` handles both cases in one atomic step: if the key doesn't exist yet, it's added with the given starting value; if it does exist, the update function runs to compute the new value. Run ten threads all calling this on the same five keys, and every key ends up with exactly the expected count, no lost updates.

---

## `ConcurrentQueue` and `ConcurrentStack`

```csharp
queue.Enqueue(42);
bool got = queue.TryDequeue(out int value);   // false if the queue happened to be empty

stack.Push(42);
bool popped = stack.TryPop(out int value);    // false if the stack happened to be empty
```

Same familiar first-in-first-out (queue) and last-in-first-out (stack) behavior as their non-concurrent counterparts, but safe to call from any thread. The `Try`-prefixed methods return `false` instead of throwing when there's nothing available, useful since a consumer thread can easily check right when the collection happens to be momentarily empty, even though a producer thread is about to add more.

---

## `ConcurrentBag`

```csharp
bag.Add(42);
bool got = bag.TryTake(out int value);
```

An unordered thread-safe collection, items don't necessarily come out in the order they went in. That's a deliberate tradeoff: giving up ordering makes `ConcurrentBag` the fastest option specifically when many threads are both adding and removing items and you don't care what order they come out in.

---

## `BlockingCollection`: Producer/Consumer

```csharp
using var collection = new BlockingCollection<int>();

// Producer:
collection.Add(1);
collection.Add(2);
collection.CompleteAdding();   // "no more items are coming"

// Consumer:
foreach (int item in collection.GetConsumingEnumerable())
{
    Console.WriteLine(item);
}
```

`BlockingCollection<T>` adds real blocking on top of a collection like `ConcurrentQueue<T>`: `GetConsumingEnumerable()` doesn't just check once and move on, it genuinely waits until an item becomes available. This is the classic producer/consumer pattern, one thread adds items whenever it has them ready, another thread processes each one as it arrives, waiting patiently in between.

`CompleteAdding()` matters here: without it, the consumer's `foreach` would wait forever for one more item after the producer is actually done. Calling it tells the collection "that's everything", letting the consumer's loop end cleanly once the last item has been processed.

---

## What This Doesn't Fix

These collections make *individual* operations safe (one `Add`, one `TryTake`). They don't make a *sequence* of operations safe together. Checking `dictionary.Count` and then deciding what to do based on that number can still go wrong, even with a `ConcurrentDictionary`, another thread can change the count between your check and your next line of code. Thread-safe collections solve "don't corrupt the data structure itself", they don't automatically make your surrounding logic race-free.

---

## Try It Yourself

Run the `BlockingCollection` example and watch the timing: "Producing item N..." and "Consumed item N..." should alternate roughly every 500 milliseconds, proof that the consumer really is waiting for each item rather than just checking once and giving up.
