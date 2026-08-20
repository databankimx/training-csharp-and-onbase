# Ch07 Textbook Code: Barrier With Tasks

## What This Is

The other half of the split, `CSharp.Ch07.TextbookCode.BarrierSample`'s companion project. Same underlying "barrier + cancellation" demo, but built with `Task.Delay(...).ContinueWith(...).ContinueWith(...)` chains instead of a single `Task.Run()` with `Thread.Sleep()` calls, genuinely different technique worth comparing directly.

---

## The Bug That Was Here: `return;` Doesn't Cross a `ContinueWith` Boundary

```csharp
// Original:
Task.Delay(1000 * localCopy + 1, tokenSource.Token)
    .ContinueWith(_ =>
    {
        ...
        if (localCopy % 2 == 0)
        {
            barrier.SignalAndWait(tokenSource.Token);
        }
        else
        {
            barrier.RemoveParticipant();
            return;   // <-- only ends THIS continuation
        }
    }, TaskContinuationOptions.NotOnCanceled)
    .ContinueWith(_ =>
    {
        // This still runs regardless of the branch taken above!
        barrier.SignalAndWait(tokenSource.Token);
    }, TaskContinuationOptions.NotOnCanceled);
```

This looks like the same fix that worked correctly in the sibling `BarrierSample` project, an early `return;` right after `RemoveParticipant()`. But `BarrierSample` has both branches inside **one single lambda**, so `return;` there exits the whole method, both "point B" and "point C" logic together. Here, the logic is split across **two separately chained** `ContinueWith` calls. `return;` inside the first continuation's delegate only completes *that* continuation's own `Task`, normally (as `RanToCompletion`, not `Canceled`), it has no effect on whether the second, already-chained continuation runs. `TaskContinuationOptions.NotOnCanceled` on the second continuation doesn't help either, since the first one never enters the `Canceled` state either way, returning early is a normal, successful completion from the TPL's point of view.

The result: every odd-numbered task would call `RemoveParticipant()`, then still reach the second continuation and call `barrier.SignalAndWait(tokenSource.Token)` again, throwing `InvalidOperationException`, unobserved and silent since nothing awaits these continuation chains.

**Fixed** with a per-iteration flag the first continuation can set, that the second continuation checks before proceeding:

```csharp
var stillParticipating = true;
Task.Delay(1000 * localCopy + 1, tokenSource.Token)
    .ContinueWith(_ =>
    {
        ...
        else
        {
            barrier.RemoveParticipant();
            stillParticipating = false;
        }
    }, TaskContinuationOptions.NotOnCanceled)
    .ContinueWith(_ =>
    {
        if (!stillParticipating) return;
        ...
        barrier.SignalAndWait(tokenSource.Token);
    }, TaskContinuationOptions.NotOnCanceled);
```

`stillParticipating` is declared fresh inside the `for` loop, alongside `localCopy`, so each of the five tasks gets its own independent flag, captured by both continuations' closures.

---

## Worth Internalizing: `return;` Inside a Continuation Only Ends That Continuation

This is a genuinely important, easy-to-miss subtlety about the Task Parallel Library, and this chapter is specifically about it. A method body with an early `return;` skips everything after it, in that same method. A **chain** of `ContinueWith()` calls is not one method, it's several separate `Task` objects, each with its own delegate, linked together. Returning from one doesn't reach into the next, the only things that can influence whether a later continuation runs are that continuation's own `TaskContinuationOptions` filter (`OnlyOnRanToCompletion`, `NotOnCanceled`, `OnlyOnFaulted`, etc.) checked against the *actual completion status* of the task it's chained from, not anything about *which code path* that task's delegate happened to take internally.

---

## Compare Both Barrier Cancellation Projects

`CSharp.Ch07.TextbookCode.BarrierWithCancellationSample` has a real bug too (the cancellation prompt serialized inside the task-spawning loop), but its Instructor Note explicitly says logic errors were left uncorrected, so it was preserved as-is. This project's Instructor Note carries no such caveat, "the book code was badly broken" without the "logic errors not corrected" qualifier, so the continuation-chaining bug found here was treated as a genuine oversight worth fixing, not a preserved teaching artifact.
