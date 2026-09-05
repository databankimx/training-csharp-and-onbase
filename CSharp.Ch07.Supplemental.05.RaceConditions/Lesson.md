# Chapter 7 Supplemental 05: Race Conditions

## What This Is

Three ways to run the same increment operation twice: unsynchronized (deliberately racy), synchronized with an `EventWaitHandle`, and synchronized with a `CountdownEvent`.

This is the project the last several lessons have been building toward. `Supplemental.03` and `Supplemental.04` both contained races noted in passing; this one is devoted to the failure mode itself.

**One real bug found and fixed** — see below. Also corrected `internal class Program` to `internal static class Program`, and brought `Main()`'s exception handling in line with the house convention (`DatabankException`/`GenericFunctions.Pause()` instead of a manual `while (ex != null)` loop and `Console.ReadLine()`).

---

## What a Race Condition Actually Is

The Chapter Notes walk through this carefully, and it's worth reproducing:

```
A race condition occurs when two threads try to update the same data.

Assume that you have one variable called sharedData and two threads, and both of them want to run
  the following instruction:
	sharedData++ (which is executed by the CPU in the following way:)
  - Read sharedData in a register.
  - Add 1 to the value in the register.
  - Write the new value from the register back into sharedData variable.
```

If it were one instruction, no error could be introduced — that's an **atomic** operation. But the scheduler can interrupt a thread between any two steps:

```
- sharedData has an initial value of 0.
- The first thread runs the first instruction, reading the value 0.
- The second thread runs the first instruction, reading the value 0.
  - On a single-core machine, this can happen when the scheduler interrupts the first thread...
  - In a multi-core machine this is a common situation because the threads can be scheduled on different cores.
- The first thread increments the value to 1.
- The first thread writes back the value 1 into sharedData.
- The second thread increments the value to 1. Now the value should have been 2, but the value that
  the second thread has is the "old" value of 0.
- The second thread writes back the value 1 into sharedData.
* Result: The change implemented by the first thread is lost
```

Note the phrasing of the outcome. Nothing crashes. No exception is thrown. The program reports success with a wrong number. **This is the defining characteristic of a race condition and the reason it's so dangerous: the failure mode is silent data corruption, not a crash.**

### Strategies, In Order of Preference

```
- Don't share the resource at all
- Make the data read-only
- Isolate the data in smaller modules
- Use synchronization mechanisms
```

That ordering is doing real work and is easy to skim past. **Synchronization is the last resort, not the first tool.** Locks are hard to get right, cost performance, and introduce deadlock risk. The earlier options eliminate the problem instead of managing it.

This is exactly what `Supplemental.03`'s `RunTasksCorrected()` did — each task returned its own value instead of sharing an accumulator. Strategy one, applied. The rest of this project covers strategy four, because sometimes sharing genuinely is unavoidable.

### The Two Mechanism Families

The notes catalog what's available:

**Synchronization Events** — each in one of two states, *signaled* (flag raised) or *non-signaled* (flag lowered).

- **`EventWaitHandle`** — `Set()`, `Reset()`, `WaitOne()`, plus `EventResetMode`. Its two subclasses, `AutoResetEvent` and `ManualResetEvent`, just preset that mode for you.
- **`CountdownEvent`** — `AddCount()`, `Signal()`, `Wait()`, `Reset()`, plus `InitialCount`, `CurrentCount`, `IsSet`. Tracks a *group* and blocks until all of them signal.

**Barriers** — `Barrier`, for grouping threads to rejoin at specified conditions. Covered in `Supplemental.06`.

---

## Approach 1: `RaceCondition()` — No Synchronization

```csharp
sharedRegister = 0;

foreach (int n in new[] {1, 2})
{
	var thread = new Thread(() => UpdateSharedResource(n));
	thread.Start();
}

// This is a cheat way of resynchronizing after the threads.
// Don't do this in production code
Thread.Sleep(200);

Console.WriteLine($"Expected Value: 2\nActual Value:  {sharedRegister}");
```

```csharp
private static void UpdateSharedResource(int num)
{
	Console.WriteLine($"Start thread {num}...");
	int s = sharedRegister;      // read
	Thread.Sleep(100);           // <- the race window, held open deliberately
	s++;                         // add
	sharedRegister = s;          // write
	Console.WriteLine($"Thread {num} incremented shared register...");
	Console.WriteLine($"End thread {num}...");
}
```

Note that `UpdateSharedResource` **spells out** the three steps the Chapter Notes describe. It doesn't write `sharedRegister++`; it manually does read, add, write with a 100ms sleep wedged into the middle.

That sleep is the most important line in the project. A real `sharedRegister++` race has a window measured in nanoseconds and might take millions of iterations to reproduce. Holding the window open for 100ms makes it fire **every single time**. The demo is reliable precisely because the bug has been made artificially easy to hit.

Both threads read 0, both compute 1, both write 1. Expected 2, actual 1, every run.

The `Thread.Sleep(200)` resynchronization is honestly labeled as a cheat. It's the fifth distinct concurrency antipattern in this chapter presented as a *deliberate* teaching device rather than an oversight — sleeping a guessed duration instead of waiting on a real signal works until the machine is slower or busier than you assumed.

---

## Approach 2: `UsingEventWaitHandle()` — Correct, But Not Concurrent

```csharp
foreach (int n in new[] { 1, 2 })
{
	var done = new EventWaitHandle(false, EventResetMode.AutoReset);
	// Equivalent to:
	//   var done = new AutoResetEvent(false);

	ThreadPool.QueueUserWorkItem(x =>
	{
		UpdateSharedResourceWithEvent(n, done);
	});

	// This defeats the point of being multi-threaded, but by forcing the threads to be synchronous
	//   (sequential), we avoid race conditions on the shared resource
	done.WaitOne();
}
```

```csharp
private static void UpdateSharedResourceWithEvent(int num, EventWaitHandle done)
{
	UpdateSharedResource(num);
	done.Set();
}
```

The result is correct — 2, every time — but read where `done.WaitOne()` sits: **inside** the loop, before the next item is even queued. Thread 2 doesn't start until thread 1 has entirely finished.

The comment says as much outright. This "defeats the point of being multi-threaded." It's correct because it isn't concurrent, and it takes just as long as running the two calls sequentially would have.

That's a genuinely valuable thing to see. It's a real pattern people ship: synchronization applied so broadly that all parallelism is serialized away, leaving the overhead of threading with none of the benefit. The correctness is real. The performance is worse than not threading at all.

Note also the same `UpdateSharedResource` with its racy read/sleep/write is reused unchanged. **The dangerous code wasn't fixed — it was merely prevented from running concurrently.** If someone later moved that `WaitOne()` outside the loop as an "optimization," the race would come straight back.

---

## The Bug That Was Here

The original `UsingCountdownEvent()` never actually spawned any threads. It called `UpdateSharedResource(n)` **directly on the calling thread**, inside the loop. Nothing ran concurrently, so it printed the correct result every time — but not because `CountdownEvent` was doing anything. There was never a race present to prevent.

The `if (countdown.CurrentCount > 1) countdown.Wait();` guard was dead code, never true anywhere in that execution path.

This is a particularly instructive bug for a teaching project, because **it passed**. Green output, expected value, no exception. A demo that appears to prove "CountdownEvent prevents race conditions" while actually proving "single-threaded code doesn't race" teaches a false lesson very convincingly.

Fixed to genuinely spawn two background work items:

```csharp
// Track two pending worker threads, one signal per thread when it finishes.
var countdown = new CountdownEvent(2);

foreach (int n in new[] { 1, 2 })
{
	ThreadPool.QueueUserWorkItem(x => UpdateSharedResourceWithCountdown(n, countdown));
}

// Block the calling thread until BOTH workers have signaled completion.
countdown.Wait();
```

Note `Wait()` is now **outside** the loop — the structural difference from the `EventWaitHandle` version, and the reason this one keeps its concurrency.

---

## Why `CountdownEvent` Alone Still Wouldn't Be Enough

Queuing two real threads and waiting on the `CountdownEvent` isn't sufficient by itself. `CountdownEvent` tracks **when** work finishes. It does nothing to stop two threads from touching `sharedRegister` at the **same time** while both are still running.

That's a different problem — mutual exclusion, not completion tracking. Using `UpdateSharedResource()`'s read/sleep/write pattern here would still race, just with `CountdownEvent` correctly reporting "both are done" over a wrong final value.

```csharp
private static void UpdateSharedResourceWithCountdown(int num, CountdownEvent countdown)
{
	Console.WriteLine($"Start thread {num}...");
	Thread.Sleep(100);
	Interlocked.Increment(ref sharedRegister);
	Console.WriteLine($"Thread {num} incremented shared register...");
	Console.WriteLine($"End thread {num}...");
	countdown.Signal();
}
```

`Interlocked.Increment` performs the read-add-write sequence — which the Chapter Notes describe as three separate, interruptible steps — as a **single atomic CPU operation**. There's no window left for the race to occur.

Note the `Thread.Sleep(100)` is still there, now *before* the increment rather than inside it. The threads genuinely overlap; the sleep no longer straddles the read and the write, so there's nothing to interleave badly.

**`CountdownEvent` and `Interlocked.Increment` solve two genuinely different problems, and the corrected demo needs both:**

| Tool | Provides |
|---|---|
| `CountdownEvent` | correct **timing** — you know when it's safe to read the final result |
| `Interlocked` | correct **data** — no increment can be silently lost |

Worth stating explicitly, because "I used a synchronization primitive" doesn't automatically mean the problem you have has been solved. **The primitive has to match the problem.** Reaching for a lock when you needed a completion signal, or a completion signal when you needed mutual exclusion, produces code that looks defensively written and still corrupts data.

`Interlocked` was chosen deliberately over a full `lock` block — it's the minimal, direct fix for exactly the non-atomic-increment problem the Chapter Notes walk through, without reaching ahead into `Supplemental.07.Locking`'s subject matter. `Supplemental.08.LockFreeAlternatives` covers the `Interlocked` family properly.

### A Note on `Signal()` and `finally`

`countdown.Signal()` is the last line of the method, not in a `finally`. If `UpdateSharedResourceWithCountdown` threw, the signal would never fire and `countdown.Wait()` would block **forever** — a deadlock, with no error message.

Nothing here can throw, so the demo is safe. But in production, a countdown signal belongs in a `finally` for the same reason `Supplemental.01` put its waits there: if you've promised a signal, you must deliver it on every path out.

---

## Compare All Three Approaches

| Method | Threads | Concurrent? | Correct? | Why |
|---|---|---|---|---|
| `RaceCondition()` | 2 real | Yes | **No** | No synchronization at all; race widened by `Thread.Sleep(100)` so it reproduces reliably |
| `UsingEventWaitHandle()` | 2 real | **No** | Yes | Correctness bought by giving up concurrency entirely — `WaitOne()` inside the loop |
| `UsingCountdownEvent()` | 2 real | Yes | Yes | `CountdownEvent` for "when is everyone done" + `Interlocked` for "protect this value" |

Only the third gets both properties at once, and it needed two different tools to do it. That progression — broken, then over-corrected, then properly corrected — is the shape of the lesson.

---

## Try It Yourself

- Run `RaceCondition()` repeatedly and confirm it reports 1 every time.
- Reduce `Thread.Sleep(100)` in `UpdateSharedResource` to `0` and see how often the race still occurs. This shows why real races evade testing.
- Move `done.WaitOne()` outside the loop in `UsingEventWaitHandle()` and watch correctness disappear.
- Replace `Interlocked.Increment(ref sharedRegister)` with `sharedRegister++` in the countdown version and watch it start failing intermittently.
- Change `new CountdownEvent(2)` to `new CountdownEvent(3)` and observe the deadlock — a signal that never arrives.
- Bump both loops to 10 items and compare elapsed times between the `EventWaitHandle` and `CountdownEvent` versions. The gap is the cost of over-synchronizing.

---

## Takeaways

- A race condition corrupts data silently — no crash, no exception, just a wrong answer.
- `++` and `+=` are read-modify-write; the scheduler can interrupt between any two steps.
- Multi-core makes simultaneous reads common rather than merely possible.
- Prefer, in order: don't share, make it read-only, isolate it, and only then synchronize.
- Widening the race window with a sleep is how you make a probabilistic bug demonstrable.
- Sleeping a guessed duration is not synchronization.
- A demo that passes for the wrong reason teaches the wrong lesson convincingly.
- Waiting inside the loop serializes the work and discards the benefit of threading.
- Preventing concurrency is not the same as fixing unsafe code.
- `CountdownEvent` answers "is everyone done"; it does not provide mutual exclusion.
- `Interlocked.Increment` makes read-add-write a single atomic operation.
- Correct timing and correct data are separate problems needing separate tools.
- A promised signal must fire on every path, including exceptions, or waiters deadlock.
