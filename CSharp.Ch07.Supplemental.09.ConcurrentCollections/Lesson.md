# Chapter 7 Supplemental 09: Concurrent Collections

## What This Is

New content — not ported from an existing download. It was added to fill a gap identified by comparing this chapter against the full textbook topic outline: "Working with Concurrent Collections" is its own dedicated textbook subsection, but nothing in the existing chapter content touched `System.Collections.Concurrent` at all.

This project covers all five main types: `ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentStack`, `ConcurrentBag`, and `BlockingCollection`.

---

## The Problem These Solve

```
The collections in System.Collections and System.Collections.Generic (List<T>, Dictionary<TKey,TValue>,
  Queue<T>, Stack<T>, etc.) were built to be fast, not thread-safe. Reading and writing one of them
  from multiple threads at the same time, without your own locking, can corrupt the collection's
  internal state or throw ("Collection was modified" is a common symptom).
```

Worth understanding *why* rather than taking it on faith, because it's the same read-modify-write problem from `Supplemental.05` at a larger scale.

Adding to a `List<T>` isn't one operation — it may check capacity, allocate a bigger array, copy every element across, write the new item, and increment `_size`. Two threads doing that simultaneously can both see the old array, both copy, and one write vanishes. Worse, `_size` can end up incremented twice for one stored item, leaving a `List<T>` that reports a count larger than what it holds and throws `IndexOutOfRangeException` on a perfectly ordinary read.

`Dictionary<TKey, TValue>` is more fragile still. A concurrent write during a resize can produce a **corrupted hash bucket chain** — and the classic symptom is an infinite loop inside `Dictionary.FindEntry`, a process pinned at 100% CPU with no exception ever thrown. That failure has taken down real production systems, and it's notoriously hard to diagnose because nothing crashes.

`System.Collections.Concurrent` provides drop-in replacements that handle this internally, with no locking of your own:

```
- ConcurrentDictionary<TKey, TValue>  Thread-safe version of Dictionary<TKey, TValue>
- ConcurrentQueue<T>                  Thread-safe version of Queue<T> (first-in, first-out)
- ConcurrentStack<T>                  Thread-safe version of Stack<T> (last-in, first-out)
- ConcurrentBag<T>                    Thread-safe, unordered collection (fastest when order
										genuinely doesn't matter)
- BlockingCollection<T>               Wraps one of the above (ConcurrentQueue<T> by default) and
										adds blocking/bounding
```

Note these mostly don't achieve safety by locking internally. `ConcurrentQueue` and `ConcurrentStack` are largely lock-free, built on the `CompareExchange` retry pattern from `Supplemental.08`. `ConcurrentDictionary` uses fine-grained locking — many independent locks across buckets — so threads touching different keys rarely contend. That's why they outperform a plain collection wrapped in a single `lock`.

---

## `ConcurrentDictionary`: `AddOrUpdate` in One Atomic Step

```csharp
var wordCounts = new ConcurrentDictionary<string, int>();
string[] words = ["apple", "banana", "cherry", "date", "elderberry"];

Parallel.For(0, 10, i =>
{
	foreach (string word in words)
	{
		wordCounts.AddOrUpdate(word, 1, (key, existingValue) => existingValue + 1);
	}
});

foreach (var pair in wordCounts)
{
	Console.WriteLine($"{pair.Key}: {pair.Value} (expected 10)");
}
```

Ten threads, each incrementing a shared count for the same five keys, no lock anywhere, and every key ends up with exactly 10.

`AddOrUpdate` is worth reading closely. It's specifically designed to replace the classic — and, on a plain `Dictionary`, thread-unsafe — check-then-act pattern:

```csharp
// NOT thread-safe, even on a ConcurrentDictionary
if (dict.ContainsKey(word))
	dict[word] = dict[word] + 1;
else
	dict[word] = 1;
```

Two threads can both find the key absent and both add 1, or both read the same existing value and both write the same increment. `AddOrUpdate` collapses the whole sequence into one atomic operation.

### The Catch Nobody Mentions

The update delegate **may be invoked more than once** for a single logical update.

Internally `AddOrUpdate` uses the optimistic retry loop from `Supplemental.08`: read the current value, run your delegate, `CompareExchange` the result in, and if another thread got there first, **run your delegate again** with the new value. Only the winning invocation's result is stored.

For `existingValue => existingValue + 1` that's harmless — the discarded computations have no effect. But it means the update delegate **must be pure**. Putting a log write, a counter increment, or an I/O call inside it produces duplicated side effects under contention, intermittently and only under load.

Note this is the same trade-off `Supplemental.08` named: lock-free means retrying rather than blocking, and retrying means your code may run more than once.

For the specific case of a counter, `Interlocked` on a plain field is cheaper still — but `ConcurrentDictionary` wins the moment you need *many* independently-keyed counters.

---

## `ConcurrentQueue` and `ConcurrentStack`: `Try` Instead of Throwing

```csharp
while (!queue.TryDequeue(out _))
{
	Thread.Sleep(1);
}
Interlocked.Increment(ref totalDequeued);
```

```csharp
if (stack.TryPop(out _)) Interlocked.Increment(ref poppedCount);
```

Both follow the `IProducerConsumerCollection<T>` pattern:

```
- TryAdd / TryTake     "try" versions that return false instead of throwing if the operation can't
						 complete right now (an empty collection has nothing to TryTake, for example)
```

This matters specifically in concurrent code. A consumer can easily race ahead of a producer and find the collection momentarily empty even though more items are still coming. `TryDequeue`/`TryPop` make that a normal, handled case rather than an exception to catch.

Note there's a deeper reason the `Try` form is the *only* safe form here. On a plain `Queue<T>` you'd write:

```csharp
if (queue.Count > 0) item = queue.Dequeue();   // still broken
```

Another thread can dequeue the last item between the check and the call, so `Dequeue()` throws anyway. **Any check-then-act on a shared collection is racy no matter how the collection is implemented** — which is why the concurrent types don't expose the throwing variants at all. The API shape is deliberately steering you away from a pattern that cannot be made safe.

The same reasoning explains why `Count` is nearly useless on these types. It's accurate for the instant it was computed and potentially stale by the time you read it. Use `IsEmpty` where you can — it's cheaper — and never branch on `Count` expecting the value to hold.

### Worth Noticing: The Spin-Wait Is a Hazard

```csharp
Parallel.Invoke(
	() => Parallel.For(0, 5, producer => { for (int i = 0; i < 20; i++) queue.Enqueue(producer * 100 + i); }),
	() => Parallel.For(0, 5, consumer =>
	{
		for (int i = 0; i < 20; i++)
		{
			while (!queue.TryDequeue(out _)) { Thread.Sleep(1); }
			Interlocked.Increment(ref totalDequeued);
		}
	})
);
```

The arithmetic works out — 5 producers × 20 items = 100 enqueued, 5 consumers × 20 = 100 dequeued — so this completes correctly.

But note the structural risk: each consumer **blocks indefinitely** until it gets its item, and both `Parallel.For` calls draw from the same thread pool. If the pool were to schedule all consumers before any producer, every consumer would sit in its `Thread.Sleep(1)` loop while the producers wait for a thread that never frees up. That's **thread pool starvation**, and it deadlocks.

In practice it doesn't happen here: `Thread.Sleep` releases the thread, the pool injects additional threads when it detects starvation, and the work is small. But it's a genuine antipattern worth recognizing — *blocking on work that itself needs a pool thread to complete* is one of the most common causes of hung .NET services.

The correct answer is the last section of this project. `BlockingCollection` exists precisely so consumers don't have to spin.

The stack demo avoids the issue entirely by pushing all 100 items before popping any, so `TryPop` never needs to wait — which is also why it can use `if` rather than `while`.

Note `Parallel.For(0, 100, stack.Push)` uses a method group conversion, the same shorthand seen in `Supplemental.03`.

---

## `ConcurrentBag`: When Order Genuinely Doesn't Matter

```csharp
var bag = new ConcurrentBag<int>();

Parallel.For(0, 10, i =>
{
	for (int j = 0; j < 10; j++) bag.Add(i * 10 + j);
});

Console.WriteLine($"Bag contains {bag.Count} items (expected 100)");
```

`ConcurrentBag<T>` is unordered — items don't come back out in any particular sequence.

That's a deliberate trade-off. Giving up ordering lets `ConcurrentBag` use **per-thread local storage** internally: each thread gets its own private list, so adding involves no contention at all. A thread taking an item takes from its own list first, and only "steals" from another thread's list when its own is empty.

That design makes it fastest specifically when **the same threads both add and remove**. Note the corollary, which is easy to get wrong: in a pure producer/consumer split — where one set of threads only adds and another only takes — `ConcurrentBag` is a *poor* choice, because the consumers own no local items and every take becomes a steal. `ConcurrentQueue` is better there.

The classic fit is the `localFinally` accumulation pattern from `Supplemental.03`: each parallel worker collecting results into a bag it mostly owns, merged at the end.

---

## `BlockingCollection`: A Real Producer/Consumer Pattern

This is the one genuinely different member of the group. The others are storage; this one adds **coordination**.

```csharp
using var collection = new BlockingCollection<int>();

var producer = Task.Run(() =>
{
	for (int i = 1; i <= 5; i++)
	{
		Console.WriteLine($"Producing item {i}...");
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

Task.WaitAll(producer, consumer);
```

`BlockingCollection<T>` wraps another concurrent collection — a `ConcurrentQueue<T>` by default — and adds actual blocking. `GetConsumingEnumerable()` doesn't poll or return early when nothing's available; it genuinely waits until the producer adds the next item.

Run it and watch the "Consumed item N..." messages appear roughly 500ms apart, in step with the producer, rather than all at once. Compare that against the queue demo's `while (!TryDequeue) Thread.Sleep(1)` — same outcome, but here the consumer thread is genuinely idle rather than waking a thousand times a second to ask again.

### `CompleteAdding()` Is Not Optional

`CompleteAdding()` tells the collection "no more items are coming."

Without it, `GetConsumingEnumerable()`'s `foreach` would **never end**. It would sit waiting for one more item that will never arrive, even after the producer has finished — and `Task.WaitAll` would hang forever. `CompleteAdding()` lets the enumeration finish cleanly once the last buffered item is consumed.

Note this is the same category of obligation as `countdown.Signal()` in `Supplemental.05` and `Monitor.Exit` in `Supplemental.07`: **if something is waiting on your signal, failing to send it produces a silent hang rather than an error.** Here it belongs in a `finally` for exactly that reason — if the producer throws mid-loop, the consumer waits forever.

Note also `using var collection` — `BlockingCollection<T>` is `IDisposable` because it holds wait handles internally. Most concurrent collections aren't; this one is.

### Bounding

The unused constructor overload is worth knowing:

```csharp
new BlockingCollection<int>(boundedCapacity: 10)
```

That makes `Add()` block when the collection is full, applying **backpressure** — a fast producer is forced to slow to the consumer's pace instead of growing the queue without limit until memory runs out. Unbounded queues between mismatched producers and consumers are a classic source of production memory exhaustion.

`GetConsumingEnumerable()` also accepts a `CancellationToken`, giving a clean shutdown path for the cooperative-cancellation pattern from `Supplemental.06`.

---

## What These Don't Solve

Worth being precise about the actual guarantee:

```
These collections don't eliminate the NEED to think about concurrency, they eliminate the need to
  write your OWN locking code around the collection itself.
```

They make **individual operations** — one `Add`, one `TryTake`, one `AddOrUpdate` — safe to call from any thread. They do **not** make a *sequence* of operations atomic together.

```csharp
// Still broken, even on a ConcurrentDictionary
if (dictionary.Count < maxItems)
	dictionary.TryAdd(key, value);
```

Another thread can change the count between the check and the add. The collections solve *"don't corrupt my data structure."* They don't solve *"my multi-step business logic is race-free."*

This is exactly the boundary `Supplemental.08` drew for `Interlocked`, and it's the single most important idea to carry out of both projects: **atomic operations don't compose into atomic transactions.** When you need several operations to appear as one, you still need a `lock` — and it's the presence of thread-safe building blocks that makes it easy to forget.

---

## Try It Yourself

- Replace `ConcurrentDictionary` with a plain `Dictionary` and run it — expect wrong counts, an exception, or a hang.
- Add a `Console.WriteLine` inside the `AddOrUpdate` delegate and count how many times it fires versus the 50 logical updates.
- Comment out `CompleteAdding()` and watch the program hang at `Task.WaitAll`.
- Construct the `BlockingCollection` with `boundedCapacity: 2` and add a `Thread.Sleep` to the consumer — watch the producer slow down to match.
- Change the queue demo's consumers to 6 × 20 items and watch it deadlock, since 120 takes will never be satisfied by 100 items.

That last one is the clearest demonstration of why spin-waiting on a shared pool is fragile.

---

## Takeaways

- Standard collections are built for speed, not thread safety; concurrent writes corrupt internal state.
- A corrupted `Dictionary` can spin forever at 100% CPU with no exception — a hang, not a crash.
- Concurrent collections mostly use lock-free or fine-grained techniques, not one big lock.
- `AddOrUpdate` replaces the unsafe check-then-act pattern with one atomic operation.
- Its update delegate can run multiple times under contention, so it must be side-effect free.
- `Try` methods exist because check-then-act is unfixable on a shared collection.
- `Count` is stale the moment you read it; prefer `IsEmpty` and never branch on a count.
- Blocking on work that needs a thread from the same pool risks starvation deadlock.
- `ConcurrentBag` trades ordering for per-thread storage — great when threads both add and take, poor for a strict producer/consumer split.
- `BlockingCollection` waits properly instead of polling, and is `IDisposable`.
- `CompleteAdding()` is an obligation; skipping it hangs consumers silently.
- Bounded capacity provides backpressure and prevents unbounded memory growth.
- Thread-safe operations do not compose into thread-safe transactions.
