# Ch07 Textbook Code: Barrier With Cancellation Sample

## What This Is

This is the textbook's canonical Code Lab for "Working with Cancellations", `Barrier.SignalAndWait(CancellationToken)`, letting a caller cancel a rendezvous that's still in progress. The embedded "Instructor Note" is explicit about scope: syntax errors (missing characters) were fixed, but logic errors were deliberately left untouched.

**No changes made here.** Preserved exactly as downloaded, including a real structural bug, on the grounds that this was an explicit editorial choice already made, not an oversight for me to silently correct.

---

## The Preserved Bug: Task Spawning Serialized Behind `Console.ReadLine()`

```csharp
for (int i = 0; i < participants; i++)
{
    var localCopy = i;
    Task.Run(() => { ... });

    Console.WriteLine("Main thread is waiting for {0} tasks!", barrier.ParticipantsRemaining - 1);
    Console.WriteLine("Press enter to cancel!");
    Console.ReadLine();

    if (barrier.CurrentPhaseNumber < 2)
    {
        tokenSource.Cancel();
        Console.WriteLine("We canceled the operation!");
    }
    else
    {
        Console.WriteLine("Too late to cancel!");
        Console.WriteLine("Main thread is done!");
        Console.ReadLine();
    }
}
```

The entire "press Enter to cancel" prompt, including the blocking `Console.ReadLine()` call, sits **inside** the loop that spawns the five tasks. That means:

- Task 1 spawns, then the main thread immediately blocks waiting for input, before task 2 is even created.
- The five tasks are supposed to run concurrently and race toward the barrier together, but they can't, each one only gets created after you've pressed Enter for the previous one.
- If you press Enter early (canceling on the first iteration), the loop still runs four more times, checking the same already-canceled `CancellationTokenSource` and printing "We canceled the operation!" again on each remaining iteration (calling `Cancel()` on an already-canceled source is harmless, just redundant).
- None of the `barrier.SignalAndWait(tokenSource.Token)` calls inside the tasks are wrapped in a `try`/`catch (OperationCanceledException)`, unlike the equivalent demo in `CSharp.Ch07.Supplemental.06.Barriers`/`Supplemental.07.Locking`. If cancellation actually happens while a task is mid-`SignalAndWait`, that exception is unobserved (fire-and-forget `Task.Run()`), silently swallowed rather than causing a visible crash.

This whole block was almost certainly meant to run *once*, after all five tasks have been spawned, not once per task. Moving it outside the `for` loop would fix the structural problem, that's a real, substantive rewrite of the control flow, not a small correction, which is exactly the kind of "logic error" the Instructor Note says was deliberately left alone.

---

## Worth Trying, As-Is

Run it and press Enter promptly. You'll see the serialization described above directly: each "Task N left point A!" appears only after you've pressed Enter for the previous one, rather than all five appearing in a burst as genuinely concurrent tasks would.

---

## Compare Against the Corrected Versions

`CSharp.Ch07.Supplemental.06.Barriers`'s `UseBarrierWithCancel()` (commented out by design, but present in the file) and `CSharp.Ch07.Supplemental.07.Locking` both demonstrate the same `CancellationToken`-based pattern with the "wait for cancel input" step properly placed *after* all participants have been spawned, and with a `try`/`catch (OperationCanceledException)` guarding each task. Worth reading both back to back with this one, seeing the broken structure and the corrected structure side by side makes the fix obvious in a way that's harder to see from either version alone.
