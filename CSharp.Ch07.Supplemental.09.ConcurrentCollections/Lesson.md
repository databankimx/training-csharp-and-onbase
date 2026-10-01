# Chapter 7 Supplemental 09: Concurrent Collections

## What This Is

The standard collections in `System.Collections.Generic` -- `List<T>`, `Dictionary<TKey,TValue>`, `Queue<T>`, `Stack<T>` -- were built to be fast, not thread-safe. Writing to one from multiple threads simultaneously can corrupt its internal state. `Dictionary<TKey,TValue>` is particularly spectacular about this: a concurrent write during a resize can produce a corrupted hash bucket chain, and the symptom is an infinite loop inside `FindEntry` with the process pinned at 100% CPU and no exception ever thrown. No crash, no error -- just a hung application and a very confused on-call engineer.

`System.Collections.Concurrent` provides drop-in replacements that handle internal synchronization for you. This project covers all five.

---

## How to Write This Program

### Mini-Program 1: ConcurrentDictionary

Clear `Main()` and write:

```csharp
Console.WriteLine("Ten threads each incrementing a shared counter for five keys...");

var wordCounts = new ConcurrentDictionary<string, int>();
string[] words = ["apple", "banana", "cherry", "date", "elderberry"];

Parallel.For(0, 10, i =>
{
    foreach (string word in words)
    {
        // AddOrUpdate: atomically add the key with the given value if it doesn't exist,
        // or update it using the given function if it does. No separate "check then add or update" needed.
        wordCounts.AddOrUpdate(word, 1, (key, existingValue) => existingValue + 1);
    }
});

foreach (var pair in wordCounts)
    Console.WriteLine($"{pair.Key}: {pair.Value} (expected 10)");

GenericFunctions.Pause();
```

Run it. Every key shows 10, every time, with no lock anywhere in sight.

`AddOrUpdate` exists specifically to replace the classic check-then-act pattern that looks safe and isn't:

```csharp
// NOT thread-safe, even on a ConcurrentDictionary:
if (dict.ContainsKey(word))
    dict[word] = dict[word] + 1;
else
    dict[word] = 1;
```

Two threads can both find the key absent and both add 1, or both read the same existing value and both write the same increment. `AddOrUpdate` collapses the whole sequence into one atomic operation.

One catch nobody mentions prominently: the update delegate **may be invoked more than once** for a single logical update. Internally `AddOrUpdate` uses optimistic concurrency (read, compute, `CompareExchange`, retry if someone else got there first). For a pure function like `existingValue + 1`, the discarded retry invocations have no effect. Put a log write, a counter, or any I/O inside the delegate and you get duplicate side effects under contention, intermittently, only under load, impossible to reproduce in development.

### Mini-Program 2: ConcurrentQueue

Clear `Main()` and write:

```csharp
Console.WriteLine("Five producer threads enqueue, five consumer threads dequeue...");

var queue = new ConcurrentQueue<int>();
int totalDequeued = 0;

Parallel.Invoke(
    () => Parallel.For(0, 5, producer =>
    {
        for (int i = 0; i < 20; i++) queue.Enqueue(producer * 100 + i);
    }),
    () => Parallel.For(0, 5, consumer =>
    {
        for (int i = 0; i < 20; i++)
        {
            while (!queue.TryDequeue(out _)) Thread.Sleep(1);
            Interlocked.Increment(ref totalDequeued);
        }
    })
);

Console.WriteLine($"Total items dequeued: {totalDequeued} (expected 100)");
GenericFunctions.Pause();
```

Run it. 100 items dequeued, every time.

`TryDequeue` returns `false` when the queue is momentarily empty rather than throwing. This is the correct shape for concurrent consumers -- checking `Count > 0` then dequeuing is a race no matter how the collection is implemented, so the concurrent types don't offer a throwing variant at all. The API steers you away from the wrong pattern.

Note the `while (!queue.TryDequeue(...)) Thread.Sleep(1)` spin-wait. Each consumer blocks until it gets its item, and both producer and consumer groups draw from the same thread pool. If the pool were to schedule all consumers before any producers, consumers would spin indefinitely while producers wait for a free thread that never becomes free. That's thread pool starvation deadlock, and it's a real hazard when blocking on work that itself needs a pool thread to complete. It doesn't happen here because `Thread.Sleep` yields the thread and the pool injects more threads when it detects starvation, but it's the pattern `BlockingCollection` in Mini-Program 5 exists to eliminate properly.

### Mini-Program 3: ConcurrentStack

Clear `Main()` and write:

```csharp
Console.WriteLine("Pushing and popping from multiple threads at once...");

var stack = new ConcurrentStack<int>();

Parallel.For(0, 100, stack.Push);  // method group conversion

int poppedCount = 0;
Parallel.For(0, 100, _ =>
{
    if (stack.TryPop(out _)) Interlocked.Increment(ref poppedCount);
});

Console.WriteLine($"Items popped: {poppedCount} (expected 100), remaining: {stack.Count} (expected 0)");
GenericFunctions.Pause();
```

Run it. 100 pushed, 100 popped, 0 remaining.

`Parallel.For(0, 100, stack.Push)` is a method group conversion -- `stack.Push` matches the signature `Action<int>` that `Parallel.For`'s delegate expects. All 100 items are pushed before any popping begins, so `TryPop` never has to wait. That's why this uses `if` rather than `while`.

`ConcurrentStack<T>` is last-in, first-out. The pop order differs from the push order -- if that matters to your use case, reach for `ConcurrentQueue`.

### Mini-Program 4: ConcurrentBag

Clear `Main()` and write:

```csharp
Console.WriteLine("Ten threads each adding ten items to a shared bag...");

var bag = new ConcurrentBag<int>();

Parallel.For(0, 10, i =>
{
    for (int j = 0; j < 10; j++) bag.Add(i * 10 + j);
});

Console.WriteLine($"Bag contains {bag.Count} items (expected 100)");
GenericFunctions.Pause();
```

Run it. 100 items, every time, in no particular order.

`ConcurrentBag<T>` is unordered, which is the trade-off that lets it use per-thread local storage internally. Each thread adds to its own private list with no contention at all. A thread taking an item takes from its own list first, and only "steals" from another thread's list when its own is empty.

This makes it fastest when **the same threads both add and take** -- think of the `localFinally` accumulation pattern from `Supplemental.03`, where each parallel worker collects results and they're merged at the end. In a strict producer/consumer split where different threads add and take, `ConcurrentBag` is actually a poor choice -- consumers own no local items and every take is a steal. Use `ConcurrentQueue` there.

### Mini-Program 5: BlockingCollection

Clear `Main()` and write:

```csharp
Console.WriteLine("A producer adds items on its own schedule; a consumer waits for each one...");

using var collection = new BlockingCollection<int>();  // wraps ConcurrentQueue by default

var producer = Task.Run(() =>
{
    for (int i = 1; i <= 5; i++)
    {
        Console.WriteLine($"Producing item {i}...");
        collection.Add(i);
        Thread.Sleep(500);
    }

    // Without this, GetConsumingEnumerable() blocks forever waiting for
    // one more item that will never arrive. The consumer never exits.
    collection.CompleteAdding();
});

var consumer = Task.Run(() =>
{
    foreach (int item in collection.GetConsumingEnumerable())
    {
        Console.WriteLine($"Consumed item {item}...");
    }
});

Task.WaitAll(producer, consumer);
Console.WriteLine("Producer and consumer both finished.");
GenericFunctions.Pause();
```

Run it. "Consumed item N..." lines appear roughly 500ms apart, in step with the producer, rather than all at once.

`BlockingCollection<T>` wraps a concurrent collection -- `ConcurrentQueue<T>` by default -- and adds actual blocking. `GetConsumingEnumerable()` doesn't poll or return empty; it genuinely waits until the producer adds the next item. Compare that against Mini-Program 2's `while (!TryDequeue) Thread.Sleep(1)` -- same outcome, but here the consumer thread is truly idle between items rather than waking every millisecond to ask if anything has shown up.

`CompleteAdding()` is not optional. Without it, `GetConsumingEnumerable()`'s `foreach` **never ends** -- it sits waiting for one more item that will never arrive, and `Task.WaitAll` hangs forever. This is the same category of obligation as `countdown.Signal()` in `Supplemental.05` and `Monitor.Exit` in `Supplemental.07`: if something is waiting on your signal, failing to send it produces a silent hang rather than an error. In real code it belongs in a `finally` block for the same reason.

Also note `using var collection` -- `BlockingCollection<T>` is `IDisposable` because it holds wait handles internally. Most concurrent collections aren't; this one is.

The unused constructor overload worth knowing:

```csharp
new BlockingCollection<int>(boundedCapacity: 10)
```

With a capacity, `Add()` blocks when the collection is full, applying **backpressure** -- a fast producer is forced to slow down to the consumer's pace instead of growing the queue without limit until the machine runs out of memory. Unbounded queues between mismatched producers and consumers are a classic source of production memory exhaustion, typically discovered at 3am when the process finally falls over.

---

## What These Don't Solve

Worth being precise about the actual guarantee: these collections make **individual operations** -- one `Add`, one `TryTake`, one `AddOrUpdate` -- safe to call from any thread. They do not make a *sequence* of operations atomic together.

```csharp
// Still broken, even on a ConcurrentDictionary:
if (dictionary.Count < maxItems)
    dictionary.TryAdd(key, value);
```

Another thread can change the count between the check and the add. Thread-safe operations do not compose into thread-safe transactions. When you need several operations to appear as one, you still need a `lock`. The presence of thread-safe building blocks makes it easy to forget this -- and that forgetfulness is exactly how subtle concurrency bugs survive into production.

---

## Takeaways

- Standard collections corrupt under concurrent writes. `Dictionary<TKey,TValue>` can loop forever at 100% CPU with no exception.
- Concurrent collections use lock-free or fine-grained techniques internally -- not one big lock.
- `AddOrUpdate` is the atomic replacement for the unsafe check-then-add-or-update pattern.
- Its update delegate may run multiple times under contention, so it must be side-effect free.
- `TryXxx` methods exist because check-then-act is unfixable on a shared collection.
- `Count` is stale the moment you read it. Never branch on a count and assume it will hold.
- Spin-waiting on work that itself needs a pool thread risks starvation deadlock.
- `ConcurrentBag` is fastest when the same threads both add and take. Use `ConcurrentQueue` for strict producer/consumer splits.
- `BlockingCollection` waits properly instead of polling, and is `IDisposable`.
- `CompleteAdding()` is an obligation. Skip it and consumers wait forever, silently.
- Bounded capacity provides backpressure and prevents unbounded memory growth.
- Thread-safe operations do not compose into thread-safe transactions.
