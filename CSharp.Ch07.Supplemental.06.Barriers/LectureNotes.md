# Chapter 7 Supplemental 06: Barriers

## What This Is

A `Barrier` coordinates a group of threads (or tasks) that need to periodically rejoin at the same point before any of them continue, a "rendezvous point." Five tasks plus the main thread (6 total participants) work through two phases, with two of the five tasks deliberately dropping out after the first phase via `RemoveParticipant()`. A second variant, `UseBarrierWithCancel()`, adds cancellation support, it's commented out in `Main()` by design (it requires interactive input, "press Enter to cancel"), left as an optional manual exploration rather than run automatically.

Corrected `internal class Program` to `internal static class Program`, and brought `Main()`'s exception handling in line with this solution's house convention.

---

## The Bug That Was Here

Both `BarrierProcess()` and `BarrierProcessWithCancel()` had the same structural problem. Each task branches on whether its index is even or odd:

```csharp
if (localCopy % 2 == 0)
{
    Console.WriteLine($"Task {localCopy} arrived at point B...");
    barrier.SignalAndWait();
}
else
{
    Console.WriteLine($"Task {localCopy} signaled but returned to point A...");
    barrier.RemoveParticipant();
}

// This ran for BOTH branches, even and odd alike:
Nap(Participants - localCopy);
Console.WriteLine($"Task {localCopy} arrived at point C...");
barrier.SignalAndWait();
```

`RemoveParticipant()` permanently removes the calling logical "slot" from the barrier, it's meant for a participant that's genuinely done and won't be back. But the trailing `Nap()`/`SignalAndWait()` code sat *outside* the `if`/`else`, so every task, including the ones that had just called `RemoveParticipant()`, fell through to call `SignalAndWait()` again anyway. A `Barrier` throws `InvalidOperationException` ("the number of operations using the barrier exceeded the number of registered participants") if more `SignalAndWait()` calls happen in a phase than the current `ParticipantCount` allows, exactly what this would trigger for every odd-numbered task.

Because these run inside fire-and-forget `Task.Run()` calls with nothing `await`ing or observing them, the exception didn't crash the program or print anything, it just silently killed that task's execution partway through. The main thread's own two `barrier.SignalAndWait()` calls would still complete correctly (since the barrier's post-`RemoveParticipant()` participant count only ever counted the genuinely-remaining participants), so the program *appeared* to run to completion successfully, while quietly swallowing an exception on every odd-numbered task the entire time.

**Fixed** by moving the "point C" continuation entirely inside the even branch, where it belongs, a task that took the `RemoveParticipant()` path has nothing further to do:

```csharp
if (localCopy % 2 == 0)
{
    Console.WriteLine($"Task {localCopy} arrived at point B...");
    barrier.SignalAndWait();

    Nap(Participants - localCopy);
    Console.WriteLine($"Task {localCopy} arrived at point C...");
    barrier.SignalAndWait();
}
else
{
    Console.WriteLine($"Task {localCopy} signaled but returned to point A...");
    barrier.RemoveParticipant();
}
```

Same fix applied to `BarrierProcessWithCancel()`.

---

## Worth Reading Closely: `RemoveParticipant()` vs. `SignalAndWait()`

The bug is a good excuse to internalize the actual distinction between these two: `SignalAndWait()` says "I've reached this rendezvous point, and I'll be here again next phase too", it's a per-phase signal from an ongoing participant. `RemoveParticipant()` says "I'm done, permanently, don't wait for me anymore, ever", it shrinks the barrier's total participant count for every future phase. A thread can do one or the other at a given rendezvous point, but never both, and definitely can't call `SignalAndWait()` again after calling `RemoveParticipant()` without first calling `AddParticipant()` to rejoin.

---

## Worth Actually Running

The `postPhaseAction` callback passed to `new Barrier(...)` fires once per completed phase, after every current participant has signaled:

```csharp
var barrier = new Barrier(Participants + 1,
    b => Console.WriteLine($"{b.ParticipantCount - 1} participants are at rendezvous point {b.CurrentPhaseNumber + 1}"));
```

Run the corrected version and watch this print twice, once per phase, with the participant count visibly dropping between the two prints, `Participants + 1` (6) minus the main thread minus the two that removed themselves, since `RemoveParticipant()` permanently shrinks `ParticipantCount` starting from the very next phase onward.
