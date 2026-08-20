# Chapter 7 Supplemental 05: Race Conditions

## What This Is

Three ways to run the same increment operation twice: unsynchronized (deliberately racy), synchronized with an `EventWaitHandle`, and synchronized with a `CountdownEvent`. Corrected `internal class Program` to `internal static class Program`, and brought the exception handling in `Main()` in line with this solution's house convention (`DatabankException`/`GenericFunctions.Pause()` instead of a manual `while (ex != null)` loop and `Console.ReadLine()`).

---

## The Bug That Was Here

The original `UsingCountdownEvent()` never actually spawned any threads, it called `UpdateSharedResource(n)` directly on the calling thread, inside the loop. Nothing ran concurrently, so it printed the "expected" correct result every time, but not because the `CountdownEvent` synchronization was doing anything, there was never a race condition present to prevent in the first place. The `if (countdown.CurrentCount > 1) countdown.Wait();` guard was dead code, never true anywhere in that execution path.

**Fixed** to genuinely spawn two background threads and use `CountdownEvent` for what it's actually for:

```csharp
private static void UsingCountdownEvent()
{
    ...
    var countdown = new CountdownEvent(2);

    foreach (int n in new[] { 1, 2 })
    {
        ThreadPool.QueueUserWorkItem(x => UpdateSharedResourceWithCountdown(n, countdown));
    }

    countdown.Wait();
    ...
}
```

---

## Why `CountdownEvent` Alone Still Wouldn't Have Been Enough

Simply queuing two real threads and waiting on the `CountdownEvent` isn't actually sufficient by itself, `CountdownEvent` tracks *when* work finishes, it does nothing to stop two threads from touching `sharedRegister` at the *same time* while both are still running. That's a different problem (mutual exclusion), and using `UpdateSharedResource()`'s original read/sleep/write pattern here would still race, just with `CountdownEvent` correctly reporting "both are done" over a wrong final value.

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

`Interlocked.Increment` performs the read-add-write sequence the Chapter Notes describe as three separate, interruptible steps as a single atomic CPU operation instead, there's no window left for the race to occur. `CountdownEvent` and `Interlocked.Increment` are solving two genuinely different problems here, and the corrected demo needs both: `CountdownEvent` gets you correct *timing* (you know exactly when it's safe to read the final result), `Interlocked` gets you correct *data* (no increment can be silently lost). Worth noticing this distinction explicitly, since "I used a synchronization primitive" doesn't automatically mean the specific problem you have has been solved, the primitive has to actually match the problem.

`Interlocked` was chosen deliberately over a full `lock` block here, it's the minimal, direct fix for exactly the non-atomic-increment problem the Chapter Notes already walk through, without reaching ahead into `Supplemental.07.Locking`'s actual subject matter.

---

## Compare All Three Approaches

- **`RaceCondition()`**: two real threads, no synchronization at all, deliberately racy (and deliberately widened via `Thread.Sleep(100)` inside the worker so the race reproduces reliably rather than by chance).
- **`UsingEventWaitHandle()`**: two real work items, but forced fully sequential (`done.WaitOne()` blocks before the next item is even queued), correctness bought by giving up concurrency entirely. The comment says as much outright.
- **`UsingCountdownEvent()`** (as fixed): two real work items running genuinely concurrently, correctness achieved without giving up concurrency, by pairing the right tool for "when is everyone done" (`CountdownEvent`) with the right tool for "protect this one shared value" (`Interlocked`).
