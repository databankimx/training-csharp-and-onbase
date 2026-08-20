# Chapter 7 Supplemental 07: Locking

## What This Is

The most comprehensive locking lesson in this chapter, three distinct synchronization primitives back to back: `Monitor` (two tasks racing for a lock on the same object), `Mutex` (three threads sharing exclusive access, with a timeout), and `Semaphore` (five threads sharing a pool of three slots). Corrected `internal class Program` to `internal static class Program`, and brought `Main()`'s exception handling in line with this solution's house convention. No functional bugs found otherwise, refreshingly clean content.

---

## `Monitor`: The Simplest Case

```csharp
Monitor.Enter(syncObject);
try
{
    syncObject.Id = iCopy + 1;
    Nap(2);
}
finally
{
    Monitor.Exit(syncObject);
}
```

Two tasks both try to lock the same `syncObject`. Whichever wins goes first, holds the lock for 2 seconds while it works, then releases it, letting the second task in. The `try`/`finally` here isn't optional decoration, it's the whole safety guarantee: if anything threw between `Enter()` and `Exit()`, skipping the `finally` would leave the object locked forever, a permanent deadlock for any other code waiting on it. The chapter notes call this out directly as the one thing to never skip.

---

## `Mutex`: Exclusive Access, With a Timeout

```csharp
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

Three threads, one `Mutex`, each holding it for 2 seconds. `WaitOne(5000)` is worth noticing specifically: unlike a bare `WaitOne()` (which blocks forever), this gives up after 5 seconds and returns `false` instead of hanging indefinitely, letting the `else` branch handle a timeout gracefully rather than deadlocking the whole thread. With three threads each holding for 2 seconds, the worst case (the third thread) waits up to roughly 4 seconds for the first two to finish, comfortably inside the 5-second timeout.

---

## `Semaphore`: A Pool, Not a Single Slot

```csharp
SemaphorePool = new Semaphore(0, 3)
```

Worth reading closely: the semaphore starts with an initial count of **0**, not 3, even though the maximum is 3. That means nothing can enter at all until something explicitly releases slots:

```csharp
for (int i = 0; i < 5; i++)
{
    var thread = new Thread(ResourceWorkWithSemaphore);
    thread.Start(i + 1);
}

Nap(1);

Console.WriteLine("Main thread releases 3 semaphore positions...");
excludedObject.SemaphorePool.Release(3);
```

Five threads spawn and immediately block on `WaitOne()`, since the semaphore has zero available slots. After a 1-second pause (giving all five threads time to actually start and reach that blocking call), the main thread releases exactly 3 slots at once, letting 3 of the 5 threads through immediately. As each of those 3 finishes and calls its own `Release()`, a slot frees up for one of the remaining 2 waiting threads, until eventually all 5 have run.

```csharp
Console.WriteLine($"Thread {num} previous semaphore count {excludedObject.SemaphorePool.Release()}...");
```

`Semaphore.Release()` returns the count *before* this release, worth noticing when reading the output, that number tells you how many slots were free the instant before this thread gave its own back, a live view into how contended the semaphore was at that exact moment.

---

## Compare All Three

`Monitor` and `Mutex` both express the same idea, exactly one holder at a time, just with different scopes (`Monitor` is in-process only; `Mutex` can coordinate across processes, and supports a timeout). `Semaphore` generalizes the same idea to *N* simultaneous holders instead of just one, worth thinking of as a `Mutex` with a capacity dial turned up past 1.
