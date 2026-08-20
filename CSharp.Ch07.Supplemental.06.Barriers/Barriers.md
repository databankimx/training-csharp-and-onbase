# Barriers

## Introduction

A `Barrier` coordinates a group of threads that need to periodically meet up at the same point before any of them are allowed to continue, a "rendezvous point." Unlike `CountdownEvent` (which fires once, when a count reaches zero), a `Barrier` can be reused across multiple phases, the group signals, waits for everyone else, then moves on together, repeatedly.

---

## Setting Up a Barrier

```csharp
var barrier = new Barrier(Participants + 1,
    b => Console.WriteLine($"{b.ParticipantCount - 1} participants are at rendezvous point {b.CurrentPhaseNumber + 1}"));
```

The first argument is the number of participants the barrier expects. The second (optional) argument is a callback that runs once every participant has signaled for the current phase, right before the next phase begins.

Each participant calls `SignalAndWait()` to say "I've arrived, wait for everyone else":

```csharp
barrier.SignalAndWait();
```

This blocks until every other participant has also called `SignalAndWait()` for that same phase, then everyone is released together.

---

## Leaving the Barrier Permanently

Sometimes a participant is done for good, not just done with this one phase. That's what `RemoveParticipant()` is for:

```csharp
barrier.RemoveParticipant();
```

This shrinks the barrier's total participant count going forward. It's important to understand this is different from `SignalAndWait()`, a participant does *one or the other* at a given rendezvous point, never both, and once removed, that participant must not call `SignalAndWait()` again unless it first calls `AddParticipant()` to rejoin.

---

## The Bug: Calling Both

The original version of this project had five tasks, three that stayed for two full phases, and two that were supposed to drop out after the first phase:

```csharp
if (localCopy % 2 == 0)
{
    barrier.SignalAndWait();       // stays for another phase
}
else
{
    barrier.RemoveParticipant();   // leaves for good
}

// This ran regardless of which branch above executed:
barrier.SignalAndWait();
```

The problem: the second `SignalAndWait()` call ran no matter which branch had executed. The two tasks that called `RemoveParticipant()` would then immediately call `SignalAndWait()` again, even though they'd just told the barrier they were leaving permanently. This throws `InvalidOperationException`, "too many operations for the number of registered participants." Because these tasks run inside fire-and-forget background work with nothing watching for exceptions, this failure was silent, the program looked like it finished successfully while quietly losing part of what those two tasks were supposed to do.

**Fixed** by keeping each branch's continuation entirely inside that branch:

```csharp
if (localCopy % 2 == 0)
{
    barrier.SignalAndWait();

    // Only participants that are staying reach this second signal.
    barrier.SignalAndWait();
}
else
{
    barrier.RemoveParticipant();
    // Nothing more to do, this task is done.
}
```

---

## Try It Yourself

Run the project and watch the participant count printed by the barrier's callback drop between the two phases, `4` (the three tasks that stayed, plus the main thread) instead of `5`, since two tasks permanently left after phase one. Then try changing which tasks remove themselves (for example, based on `localCopy % 3 == 0` instead of `% 2`) and predict what the new participant counts should be before running it again.
