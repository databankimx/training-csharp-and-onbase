# Chapter 7 Supplemental 06: Barriers

## What This Is

A `Barrier` coordinates a group of threads (or tasks) that need to periodically rejoin at the same point before any of them continue — a **rendezvous point**.

Five tasks plus the main thread (6 total participants) work through two phases, with two of the five deliberately dropping out after the first phase via `RemoveParticipant()`. A second variant, `UseBarrierWithCancel()`, adds cancellation support; it's commented out in `Main()` by design because it requires interactive input ("press Enter to cancel"), left as an optional manual exploration rather than run automatically.

**One real bug found and fixed** — see below. Also corrected `internal class Program` to `internal static class Program`, and brought `Main()`'s exception handling in line with the house convention.

---

## How a Barrier Differs From Everything Before It

The Chapter Notes carry the reference forward from `Supplemental.05`:

```
- Barrier
  Provides a means of grouping threads to rejoin at specified conditions
  METHODS
  - AddParticipant()       Adds a process to the barrier
  - AddParticipants()      Adds multiple processes to the barrier
  - RemoveParticipant()    Removes a process from the barrier
  - RemoveParticipants()   Removes multiple processes from the barrier
  - SignalAndWait()        Indicates that a process has reached the barrier and will await the others
  PROPERTIES
  - CurrentPhaseNumber     Identifies the barrier's current phase
  - ParticipantCount       Number of processes participating in the barrier
  - ParticipantsRemaining  Number of participating processes that have not yet reached the barrier
```

Every synchronization tool so far has been **one-shot**. `Thread.Join()`, `EventWaitHandle.WaitOne()`, `CountdownEvent.Wait()`, `Task.WaitAll()` — each answers "is this work finished?" once, and then you're done with it.

A `Barrier` is **repeating**. It answers "has everyone reached this point *this time*?" — and then resets itself and asks again for the next phase. That's what `CurrentPhaseNumber` is tracking.

The classic use case is iterative parallel computation: a simulation where every worker computes its slice of step N, all must finish before any can begin step N+1 (because step N+1 reads neighbors' results), repeat for a thousand steps. A `CountdownEvent` per step would mean allocating a thousand of them. A `Barrier` handles all thousand phases with one object.

Note the mental model shift: `CountdownEvent` counts **down** to zero and is spent. A `Barrier` counts arrivals, releases everyone, resets, and does it again.

---

## The Setup

```csharp
// When declaring a barrier, add one extra participant for the main thread
var barrier = new Barrier(Participants + 1,
	b =>
	{
		// Here, we count one less than the actual count, since we aren't concerned with the main thread
		// I have added one to the phase ID to count from 1 instead of 0
		Console.WriteLine($"{b.ParticipantCount - 1} participants are at rendezvous point {b.CurrentPhaseNumber + 1}");
	});
```

`Participants` is 5, so the barrier is constructed expecting **6**.

That `+ 1` is essential and easy to forget. The main thread also calls `SignalAndWait()` — it's a participant, not an observer. Construct the barrier with 5 and the main thread's signal would be the sixth call in a five-participant phase, throwing `InvalidOperationException`. Construct it with 7 and every phase would hang forever waiting for a participant that doesn't exist.

**A barrier's participant count must exactly match the number of things that will call `SignalAndWait()`.** Off by one in either direction and you get a crash or a deadlock.

### The Post-Phase Action

The second constructor argument is a `postPhaseAction` — a callback that fires **once per completed phase**, after every current participant has signaled but before any of them are released.

This is a genuinely useful hook, and worth understanding beyond the logging use here. It runs on exactly one thread, with every participant known to be stopped, which makes it the one place in a parallel algorithm where you can safely touch shared state without synchronization. In an iterative simulation, this is where you'd aggregate the step's results, check a convergence condition, or swap buffers.

Note `b.CurrentPhaseNumber + 1` — phases are zero-indexed, and the `+ 1` is purely cosmetic so output reads "point 1" and "point 2". `b.ParticipantCount - 1` likewise excludes the main thread from the reported count.

---

## The Worker Logic

```csharp
for (int i = 0; i < Participants; i++)
{
	int localCopy = i;

	Task.Run(() =>
	{
		Console.WriteLine($"Task {localCopy} left point A...");
		Nap(localCopy + 1);

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
	});
}
```

Note `int localCopy = i;` — the `for`-loop capture fix from `Supplemental.01`. Without it, all five tasks would capture the same `i` and most likely see `5`.

Even-numbered tasks (0, 2, 4) go the distance: signal at B, work, signal at C. Odd-numbered tasks (1, 3) leave permanently at B.

`Nap(localCopy + 1)` gives each task a different arrival time — 1 through 5 seconds — so you can watch them straggle in rather than arriving together. That's the point of the demo: the barrier holds the early arrivers until the slowest one shows up.

### The Main Thread's Side

```csharp
Console.WriteLine($"Main thread is waiting for {barrier.ParticipantsRemaining - 1} participants...\n");

barrier.SignalAndWait(); // Main thread waiting at the first phase
Console.WriteLine("\nMain thread signaled phase B...\n");
barrier.SignalAndWait(); // Main thread waiting at the second phase
Console.WriteLine("\nMain thread signaled phase C...\n");

// This pause is to allow the remaining threads that were blocked at B by the main thread to complete the journey
Nap(Participants);
Console.WriteLine("\nMain thread complete.\n");
```

The main thread participates in both phases, then naps to let the fire-and-forget tasks finish printing before the program exits.

That final `Nap` is the same "cheat resynchronization" `Supplemental.05` explicitly warned against — sleeping a guessed duration instead of waiting on a real signal. It's here because `Task.Run` results are never captured, so there's nothing to wait on. Worth noticing that the *reason* the cheat is needed is a separate design shortcut: keeping the task handles and calling `Task.WaitAll` would remove the need for it entirely.

---

## The Bug That Was Here

Both `BarrierProcess()` and `BarrierProcessWithCancel()` had the same structural problem. The trailing "point C" code sat **outside** the `if`/`else`:

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

`RemoveParticipant()` permanently removes the calling participant from the barrier — it's meant for a participant that's genuinely done and won't be back. But because the point-C code was outside the branch, every task, **including the ones that had just removed themselves**, fell through and called `SignalAndWait()` again.

A `Barrier` throws `InvalidOperationException` — *"the number of operations using the barrier exceeded the number of registered participants"* — when more `SignalAndWait()` calls arrive in a phase than the current `ParticipantCount` allows. That's exactly what this triggered for every odd-numbered task.

### Why Nobody Noticed

This is the part worth dwelling on. These run inside **fire-and-forget `Task.Run()` calls with nothing awaiting or observing them.** As established in `Supplemental.03`, an exception inside a task is captured into the task's `Exception` property and surfaces only when you `Wait()`, read `.Result`, or `await`. None of that happens here.

So the exception didn't crash the program or print anything. It silently killed that task's execution partway through.

Meanwhile the main thread's two `SignalAndWait()` calls still completed correctly, because the barrier's post-`RemoveParticipant()` count only ever expected the genuinely-remaining participants. The program **appeared to run to completion successfully** while quietly swallowing an exception on every odd-numbered task, every single run.

Three separate factors had to line up to hide this: fire-and-forget tasks that swallow exceptions, a barrier count that stayed self-consistent, and console output interleaved enough that two missing "point C" lines didn't look wrong. Any one of them absent and the bug would have been obvious.

**Fixed** by moving the point-C continuation inside the even branch, where it belongs — a task that took the `RemoveParticipant()` path has nothing further to do:

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

The bug is a good excuse to internalize the distinction, because the two calls look superficially similar — both are things a participant does when it reaches the barrier.

| Call | Means |
|---|---|
| `SignalAndWait()` | "I've reached this rendezvous point, and I'll be here again next phase too." |
| `RemoveParticipant()` | "I'm done, permanently. Don't wait for me anymore, ever." |

`SignalAndWait()` is a **per-phase** signal from an ongoing participant. `RemoveParticipant()` **shrinks the barrier's total participant count** for every future phase.

A thread can do one or the other at a given rendezvous point, never both — and definitely cannot call `SignalAndWait()` again after `RemoveParticipant()` without first calling `AddParticipant()` to rejoin.

There's a subtlety in the naming worth flagging: `RemoveParticipant()` does *not* signal the current phase. It reduces the number of signals the phase is waiting for. The net effect on whether the phase completes is similar, but the mechanism is different — and that difference is exactly why calling both is an error rather than merely redundant.

---

## Worth Actually Running

Watch the post-phase callback print twice, once per phase.

The lecture notes suggest you'll see the participant count drop between the two prints. **In practice you most likely won't**, and it's worth understanding why rather than assuming the output is wrong.

The barrier starts at 6. Tasks 1 and 3 nap 2 and 4 seconds respectively before removing themselves. Task 4 — the slowest even task — naps 5 seconds before signaling at B. Since phase 1 can't complete until task 4 arrives at the 5-second mark, both removals have already happened by then. `ParticipantCount` is 4 when the phase-1 callback fires, and still 4 for phase 2. Both prints show `3`.

The count *would* visibly drop if an odd task napped longer than every even task. Change `Nap(localCopy + 1)` to `Nap(Participants - localCopy)` and task 1 becomes the straggler — then the phase-1 callback fires before some removals land, and the two prints differ.

That's a better exercise than the original claim, because it makes the real lesson explicit: **`ParticipantCount` is read at the moment the callback fires, and what it reports depends entirely on the relative timing of the participants.** Reasoning about "what the count should be" without accounting for arrival order is how off-by-one barrier bugs get written.

---

## The Cancellation Variant

`UseBarrierWithCancel()` is commented out in `Main()` because it blocks on `Console.ReadLine()`. Worth reading and optionally uncommenting.

```csharp
barrier.SignalAndWait(tokenSource.Token);
```

`SignalAndWait` accepts a `CancellationToken`. If the token is cancelled while a participant is blocked, the wait throws `OperationCanceledException` instead of hanging forever.

```csharp
catch (OperationCanceledException)
{
	// Do nothing
}
```

Each task wraps its work in a `try`/`catch` that swallows the cancellation. Note this is one of the few places where an empty catch is legitimate — cancellation is an expected outcome, not a failure. It's worth contrasting with the bug above: there, an exception was swallowed *accidentally* by the fire-and-forget pattern and hid a real defect. Here it's swallowed *deliberately* and explicitly, with a comment saying so. The difference between those two situations is intent made visible in the code.

```csharp
if (barrier.CurrentPhaseNumber < 1)
{
	tokenSource.Cancel();
	Console.WriteLine("\nOperation canceled...\n");
}
else
{
	Console.WriteLine("Too late to cancel...");
}
```

Cancellation is only attempted if phase 0 hasn't completed yet. This models a real constraint: once a phase has committed, cancelling mid-flight leaves participants in inconsistent states. Deciding *when* cancellation is still safe is part of designing for it — a token alone doesn't make an operation cancellable.

Note that .NET cancellation is always **cooperative**. `tokenSource.Cancel()` doesn't stop anything; it sets a flag that blocked waits and polling code observe. A task ignoring its token runs to completion regardless. Same principle as `BackgroundWorker.CancellationPending` in `Supplemental.02`.

---

## Try It Yourself

- Run `UseBarrier()` and watch tasks arrive at B in staggered order while the barrier holds them.
- Change `new Barrier(Participants + 1, ...)` to `new Barrier(Participants, ...)` and watch it throw when the main thread signals.
- Change it to `Participants + 2` and watch it deadlock instead.
- Swap `Nap(localCopy + 1)` for `Nap(Participants - localCopy)` and see the phase-1 participant count differ from phase 2.
- Restore the bug — move the point-C block outside the `if`/`else` — and confirm the program still *appears* to succeed.
- Then capture the tasks in an array and add `Task.WaitAll(tasks)`, and watch the hidden `InvalidOperationException` finally surface as an `AggregateException`.

That last pair is the most valuable exercise in the project.

---

## Takeaways

- A `Barrier` is a repeating rendezvous; every other primitive so far was one-shot.
- Phases make it the right tool for iterative parallel work where each step depends on the last.
- The participant count must exactly match the number of `SignalAndWait()` callers — too few throws, too many deadlocks.
- The main thread counts as a participant if it signals.
- `postPhaseAction` runs once per phase on one thread with everyone stopped — the safe place to touch shared state.
- `SignalAndWait()` is per-phase; `RemoveParticipant()` is permanent, and they are mutually exclusive at a given point.
- A removed participant that signals again exceeds the remaining count and throws.
- Fire-and-forget tasks swallow exceptions, so a broken task can look like a working one.
- A program that appears to succeed may be failing silently on every run.
- Capturing task handles and calling `WaitAll` surfaces hidden failures and removes the need to sleep-and-hope.
- `ParticipantCount` reflects the instant the callback fires; arrival order determines what you see.
- Cancellation in .NET is cooperative and needs a defined point after which it's refused.
- An empty catch is defensible when the exception is an expected outcome and the code says so.
