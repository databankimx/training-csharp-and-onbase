# Chapter 7 Supplemental 07: Locking

## What This Is

The most comprehensive locking lesson in this chapter — three distinct synchronization primitives back to back:

- **`Monitor`** — two tasks racing for a lock on the same object
- **`Mutex`** — three threads sharing exclusive access, with a timeout
- **`Semaphore`** — five threads sharing a pool of three slots

Corrected `internal class Program` to `internal static class Program`, and brought `Main()`'s exception handling in line with the house convention. No functional bugs found otherwise — refreshingly clean content after the last two projects.

---

## What Locking Is For

```
Locking is implementing a mutual exclusion to ensure only one process at a time can access a resource.
```

This is the direct answer to `Supplemental.05`'s problem. A race condition happens because a read-modify-write sequence can be interrupted partway through. Locking makes that sequence **uninterruptible by other participants** — not by preventing the scheduler from switching threads, but by ensuring any other thread that arrives has to wait its turn.

Note the distinction from everything in `Supplemental.06`. A `Barrier` coordinates *when* threads proceed relative to each other. A lock controls *who* may touch a resource at a given moment. Timing versus exclusivity — the same split flagged in `Supplemental.05` between `CountdownEvent` and `Interlocked`.

The Chapter Notes also name what this project doesn't cover:

```
Other types of locking (not demonstrated in the code examples below) include:
- Interlock
- ReaderWriterLock
- ReaderWriterLockSlim
- and others
```

`Interlocked` appeared in `Supplemental.05` and gets full treatment in `Supplemental.08`. The `ReaderWriterLock` family is worth knowing exists: it allows unlimited concurrent *readers* but exclusive *writers*, which is a large win for data read far more often than it's modified — a configuration cache, a lookup table. A plain `Monitor` serializes readers against each other for no benefit.

---

## `Monitor`: The Simplest Case

```
- Monitor:
  Synchronize object access to reference types only
  METHODS        Note: All methods are static
  - Enter()      Acquires exclusive lock on specified object - enter ready queue and await if already locked
  - Exit()       Release lock on specified object
  - IsEntered()  True if the current thread holds the lock
  - TryEnter()   Attempts to acquire exclusive lock on specified object
	Note: Methods below can only be called when holding the lock
  - Pulse()      Notifies thread in waiting queue that the state has changed
  - PulseAll()   Notifies all threads in waiting queue that the state has changed
  - Wait()       Release lock and enter waiting queue until another thread pulses the monitor

  * Alert!       It's important to always provide an exit in the event of an exception to avoid a deadlock
```

```csharp
Console.WriteLine($"Start task {iCopy}...");
Monitor.Enter(syncObject);
try
{
	Console.WriteLine($"Object locked by task {iCopy}...");
	syncObject.Id = iCopy + 1;
	Console.WriteLine($"Object's ID is now {syncObject.Id}...");
	Nap(2);
}
finally
{
	Monitor.Exit(syncObject);
	Console.WriteLine($"Object released by task {iCopy}...");
}
```

Two tasks both try to lock the same `syncObject`. Whichever wins goes first, holds the lock for 2 seconds while it works, then releases it, letting the second in.

Watch the output ordering: both "Start task" lines print immediately (they're outside the lock), then one "Object locked" line, a 2-second gap, a "released" line, and only then the other task's "locked" line. The gap is the second task blocked inside `Monitor.Enter`.

### The `try`/`finally` Is the Whole Safety Guarantee

The Chapter Notes flag this with an explicit `* Alert!`, and it earns it. If anything threw between `Enter()` and `Exit()`, skipping the `finally` would leave the object **locked forever** — a permanent deadlock for any other code waiting on it.

Note how this differs from an ordinary resource leak. A leaked file handle is eventually reclaimed when the process exits. A leaked lock **actively blocks other threads** for the remaining life of the process, and those threads produce no error — they just stop. You get a hung application with no exception and no stack trace pointing at the cause.

### The `lock` Keyword

C# has syntax for exactly this pattern:

```csharp
lock (syncObject)
{
	syncObject.Id = iCopy + 1;
	Nap(2);
}
```

That compiles to essentially the `Monitor.Enter`/`try`/`finally`/`Monitor.Exit` above. In production code, `lock` is what you should write — it makes the `finally` impossible to forget.

The explicit form is used here deliberately, so you can see the mechanism the keyword hides. It's also genuinely necessary when you need `TryEnter` with a timeout, which `lock` can't express.

### What to Lock On

`Monitor` works on **reference types only**, as the notes state. Locking a value type would box it, producing a different object each time and therefore no mutual exclusion at all.

Two rules worth carrying beyond this demo:

**Never lock on something publicly reachable.** Not `this`, not a `public` field, not a `Type`, and never a string (interned literals are shared process-wide, so unrelated code can collide with you). The convention is a dedicated `private static readonly object` used for nothing else.

**Every thread must lock on the same instance.** This demo works because `syncObject` is created once outside the loop and captured by both closures. Move `new Thing()` inside the loop and each task locks its own object — the code still compiles, still looks locked, and provides zero protection. That's a particularly nasty bug class, because the safety machinery is all visibly present.

Note also that `Monitor` is **reentrant**: a thread already holding a lock can `Enter` it again without deadlocking, provided it `Exit`s the matching number of times.

### `Wait` and `Pulse`

`Wait()`, `Pulse()`, and `PulseAll()` aren't used in this demo but are the mechanism behind condition-variable patterns — "hold the lock, but release it and sleep until someone tells me the state changed." That's how you'd build a producer/consumer queue by hand. `Supplemental.09.ConcurrentCollections` covers the collections that spare you from doing so.

---

## `Mutex`: Exclusive Access, With a Timeout

```
- Mutex:
  Short for "mutual exclusion"
  Synchronizes access (including inter-process) to a resource, blocking threads until they own the mutex
```

```csharp
Console.WriteLine($"{Thread.CurrentThread.Name} is requesting the mutex...");
if (excludedObject.Mutex.WaitOne(5000))
{
	try
	{
		excludedObject.Name = Thread.CurrentThread.Name;
		Nap(2);
	}
	finally
	{
		excludedObject.Mutex.ReleaseMutex();
	}
}
else
{
	Console.WriteLine($"{Thread.CurrentThread.Name} has failed to acquire control of the mutex...");
}
```

Three threads, one `Mutex`, each holding it for 2 seconds.

Note the mutex is a **member of the protected object** (`Thing.Mutex`), as the source comment points out. That's a good habit: the lock travels with the thing it protects, so there's no way to get a reference to the resource without also having its guard. Contrast with a lock stored somewhere else entirely, which is how "everyone agreed to lock on the same object" quietly stops being true.

Note also `Thread.CurrentThread.Name` — the threads are named at creation (`Name = $"Thread {i + 1}"`), which is why the output is readable. Naming threads costs nothing and is enormously helpful in a debugger, where the alternative is a list of numeric IDs.

### The Timeout Is the Point

`WaitOne(5000)` is the detail worth studying. Unlike a bare `WaitOne()` — which blocks forever — this gives up after 5 seconds and returns `false`, letting the `else` branch handle the timeout gracefully rather than hanging the thread indefinitely.

With three threads each holding for 2 seconds, the worst case (the third thread) waits roughly 4 seconds, comfortably inside the timeout. So in normal operation the `else` never fires.

That's worth sitting with. **The timeout branch is dead code in the happy path, and that's exactly why it matters.** A bounded wait converts a silent hang into an observable, handleable event. The failure it protects against is the one where some other thread has died holding the lock — and without a timeout, your only symptom is a thread that never returns.

`Monitor.TryEnter(obj, 5000)` provides the same capability for monitors, which is one of the cases where the `lock` keyword isn't sufficient.

### The Cross-Process Capability

A `Mutex` can be **named**, at which point it's a kernel object visible to every process on the machine:

```csharp
var mutex = new Mutex(false, "Global\\MyAppSingleInstance");
```

This is the standard technique for "only one copy of this application may run at a time." `Monitor` cannot do this at all — it's purely in-process.

That capability is why a `Mutex` is substantially more expensive than a `Monitor`. Every acquisition is a kernel transition, where an uncontended `Monitor` stays in user mode. **If you don't need cross-process coordination or a timeout, use `Monitor`/`lock`.**

Note the unnamed `new Mutex()` here is in-process only, so this demo pays the cost without using the feature — reasonable for a lesson, wasteful in production.

One sharp edge: a `Mutex` has **thread affinity**. Only the thread that acquired it may release it. `ReleaseMutex()` from another thread throws `ApplicationException`. That rules out acquiring in one place and releasing in a continuation — a real constraint when mixing mutexes with `async`/`await`.

---

## `Semaphore`: A Pool, Not a Single Slot

```
- Semaphore:
  Limits the number of threads that can simultaneously access a resource
  - WaitOne()            Wait for the semaphore to have at least one available slot and take control
  - Release()            Release control of slot and signal available
  - Release(n)           Release control of n slots and signal available
```

```csharp
SemaphorePool = new Semaphore(0, 3)
```

Worth reading closely: the semaphore starts with an initial count of **0**, not 3, even though the maximum is 3. Nothing can enter at all until something explicitly releases slots.

```csharp
for (int i = 0; i < 5; i++)
{
	var thread = new Thread(ResourceWorkWithSemaphore);
	thread.Start(i + 1);
}

Nap(1);

Console.WriteLine("Main thread releases 3 semaphore positions...");
excludedObject.SemaphorePool.Release(3);

Console.WriteLine("Main thread exits...");
```

Five threads spawn and immediately block on `WaitOne()`, since zero slots are available. After a 1-second pause — giving all five time to actually start and reach that blocking call — the main thread releases 3 slots at once, letting 3 of the 5 through immediately. As each finishes and calls its own `Release()`, a slot frees for one of the remaining 2, until all 5 have run.

Note this `new Semaphore(0, 3)` + `Release(3)` construction is a **starting gate**, deliberately different from `new Semaphore(3, 3)` which would let the first three threads through the instant they arrive. Starting at zero lets the main thread decide precisely when the race begins — which is what makes the output legible.

Note also `thread.Start(i + 1)` passing the number through `ParameterizedThreadStart`, hence `ResourceWorkWithSemaphore(object num)` taking an `object`. That's the awkward single-`object` signature from Chapter 6, and the reason `ThreadTracker` in `Supplemental.01` bundled its state into a class.

### `Release()` Returns the Previous Count

```csharp
Console.WriteLine($"Thread {num} previous semaphore count {excludedObject.SemaphorePool.Release()}...");
```

`Semaphore.Release()` returns the count **before** this release. That number tells you how many slots were free the instant before this thread gave its own back — a live view into how contended the semaphore was at that moment.

A `0` means the semaphore was fully saturated and threads were queued. Anything higher means capacity was going unused. In a real system that's the signal for whether your pool size is tuned correctly.

### The Danger: Unbalanced `Release()`

A semaphore does **not** track who holds its slots. Calling `Release()` without a matching `WaitOne()` simply hands out a slot that was never taken, permanently raising available capacity past what your design intended. There's no error — just more concurrent access than you asked for.

The only guard is the maximum: exceeding it throws `SemaphoreFullException`. That's a genuine hazard here in principle, since five threads each call `Release()` while the main thread also released 3. The demo stays balanced only because every worker calls `WaitOne()` before its `Release()`, so the net count never exceeds 3.

This is the practical difference from a `Mutex`: a mutex's thread affinity makes an unbalanced release *impossible*, while a semaphore trusts you completely.

---

## Worth Noticing: The Semaphore Worker Has No `try`/`finally`

```csharp
private static void ResourceWorkWithSemaphore(object num)
{
	Console.WriteLine($"Thread {num} requesting semaphore access...");
	excludedObject.SemaphorePool.WaitOne();

	Console.WriteLine($"Thread {num} enters the semaphore...");
	Nap(1);

	Console.WriteLine($"Thread {num} releases the semaphore...");
	Console.WriteLine($"Thread {num} previous semaphore count {excludedObject.SemaphorePool.Release()}...");
}
```

Both the `Monitor` and `Mutex` demos wrap their protected work in `try`/`finally`, and the Chapter Notes flag it with an `* Alert!`. This one doesn't.

If `Nap(1)` threw, the slot would never be released and the pool would permanently shrink by one — the exact deadlock the notes warn about, just gradual rather than immediate. Lose all three slots to exceptions and the semaphore is dead with no error ever reported.

Nothing here can throw, so the demo is correct as written. It's flagged because **the inconsistency itself is the lesson**: the same code, in the same file, applies a safety pattern twice and omits it once. That's precisely how the pattern erodes in real codebases — nobody removes the `try`/`finally`, someone just writes the next method without it.

The correct form:

```csharp
excludedObject.SemaphorePool.WaitOne();
try
{
	Nap(1);
}
finally
{
	excludedObject.SemaphorePool.Release();
}
```

Left as-is to preserve the original structure, but worth writing out. **Acquire, `try`, work, `finally` release** — no exceptions, regardless of which primitive you're holding.

---

## The `Nap()` Calls in `Main()`

```csharp
UsingMonitor();
Nap(5); // Allow tasks to complete
```

Each lesson method returns immediately after spawning its workers, so `Main()` sleeps to let them finish before the next section starts. The same guessed-duration cheat seen in `Supplemental.05` and `.06` — necessary here only because no task handles or thread references are retained.

The durations are chosen to comfortably exceed the work: 5 seconds for two 2-second monitor tasks, 7 for three 2-second mutex threads, 5 for five 1-second semaphore threads in a 3-slot pool. Note the mutex figure is the tightest — 3 × 2s = 6 seconds of strictly serialized work against a 7-second budget.

---

## Compare All Three

| | Scope | Holders | Timeout | Affinity | Cost |
|---|---|---|---|---|---|
| `Monitor` / `lock` | In-process | 1 | via `TryEnter` | Reentrant, same-thread release | Cheapest |
| `Mutex` | Cross-process when named | 1 | via `WaitOne(ms)` | Must release on acquiring thread | Kernel-level |
| `Semaphore` | Cross-process when named | N | via `WaitOne(ms)` | None — any thread may release | Kernel-level |

`Monitor` and `Mutex` both express the same idea — exactly one holder at a time — with different scopes. `Semaphore` generalizes to *N* simultaneous holders: a `Mutex` with the capacity dial turned past 1.

**Default to `lock`.** Reach for `Mutex` when you need cross-process coordination, and `Semaphore` when you're rate-limiting access to a genuinely finite pool — database connections, licence seats, outbound API calls.

---

## Try It Yourself

- Move `new Thing()` inside the `UsingMonitor` loop and watch both tasks "lock" simultaneously with no protection whatsoever.
- Rewrite the `Monitor` block using the `lock` keyword and confirm identical behavior.
- Remove the `finally` from `UseResourceWithMutex` and throw inside the `try` — then watch the other two threads hit the 5-second timeout and report failure. This is the clearest demonstration of why the timeout exists.
- Change `WaitOne(5000)` to `WaitOne(1000)` and watch the third thread fail to acquire.
- Change `new Semaphore(0, 3)` to `new Semaphore(3, 3)` and note the starting gate disappears.
- Add an extra `Release()` to the semaphore worker and watch `SemaphoreFullException` appear.
- Add the missing `try`/`finally` to `ResourceWorkWithSemaphore`, then throw inside it, and compare against the current behavior.

---

## Takeaways

- Locking provides mutual exclusion — the direct fix for the race conditions in `Supplemental.05`.
- Locks control *who* may access a resource; barriers control *when* threads proceed.
- Always `try`/`finally` around held locks; a leaked lock hangs other threads with no error.
- The `lock` keyword is `Monitor.Enter`/`try`/`finally`/`Exit` and should be your default.
- Lock on a private, dedicated reference type — never `this`, a public field, a `Type`, or a string.
- Every thread must lock the same instance; per-thread lock objects compile fine and protect nothing.
- `Monitor` is in-process, reentrant, and cheap when uncontended.
- `Mutex` can coordinate across processes when named, at kernel-transition cost.
- A `Mutex` has thread affinity — only the acquiring thread may release it.
- A bounded wait turns a silent hang into a handleable event, even if it never fires in testing.
- `Semaphore` allows N concurrent holders and tracks only a count, not ownership.
- Unbalanced `Release()` silently inflates capacity until it throws `SemaphoreFullException`.
- `Release()` returns the prior count — a live measure of contention.
- A safety pattern applied inconsistently within one file is how the pattern eventually disappears.
