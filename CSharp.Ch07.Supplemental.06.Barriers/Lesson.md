# Chapter 7 Supplemental 06: Barriers

## What This Is

Every synchronization primitive so far - `Join`, `EventWaitHandle`, `CountdownEvent`, `Task.WaitAll` - is one-shot. You wait once, everyone crosses, that's the end of it. A `Barrier` answers a different question: "has everyone reached *this point* this time?" Then it resets itself and asks the same question again for the next phase.

What's being abstracted is the multi-phase coordination pattern. Without `Barrier`, implementing "all workers must finish step N before any of them can start step N+1" requires you to chain multiple `CountdownEvent` instances together, reset them between phases, and manage the participant count manually if some workers drop out mid-run. `Barrier` encapsulates all of that into one object that handles reset, phase tracking, and participant management automatically.

The improvement is maintainability and correctness. A `Barrier` makes the rendezvous structure explicit and hard to break accidentally - any thread that calls `SignalAndWait()` at the wrong time gets an immediate `InvalidOperationException` rather than a silent hang.

---

## How to Write This Program

Add a helper and a constant to `Program.cs`:

```csharp
private const int Participants = 5;

private static void Nap(int seconds) => Thread.Sleep(seconds * 1000);
```

### Mini-Program 1: UseBarrier()

Clear `Main()` and write:

```csharp
// +1 because the main thread is also a participant and will call SignalAndWait()
var barrier = new Barrier(Participants + 1,
    b =>
    {
        // ParticipantCount - 1 to exclude the main thread from the reported count
        // CurrentPhaseNumber + 1 to count phases from 1 instead of 0
        Console.WriteLine($"{b.ParticipantCount - 1} participants are at rendezvous point {b.CurrentPhaseNumber + 1}");
    });

for (int i = 0; i < Participants; i++)
{
    int localCopy = i;  // per-iteration capture - see Supplemental.01

    Task.Run(() =>
    {
        Console.WriteLine($"Task {localCopy} left point A...");
        Nap(localCopy + 1);  // stagger arrivals: task 0 in 1s, task 4 in 5s

        if (localCopy % 2 == 0)
        {
            // Even tasks go the full distance
            Console.WriteLine($"Task {localCopy} arrived at point B...");
            barrier.SignalAndWait();

            Nap(Participants - localCopy);
            Console.WriteLine($"Task {localCopy} arrived at point C...");
            barrier.SignalAndWait();
        }
        else
        {
            // Odd tasks drop out permanently after phase 1
            Console.WriteLine($"Task {localCopy} signaled but returned to point A...");
            barrier.RemoveParticipant();
        }
    });
}

Console.WriteLine($"Main thread is waiting for {barrier.ParticipantsRemaining - 1} participants...\n");

barrier.SignalAndWait();  // main thread signals phase 1
Console.WriteLine("\nMain thread signaled phase B...\n");
barrier.SignalAndWait();  // main thread signals phase 2
Console.WriteLine("\nMain thread signaled phase C...\n");

// Allow fire-and-forget tasks to finish printing
Nap(Participants);
Console.WriteLine("\nMain thread complete.\n");
GenericFunctions.Pause();
```

Run it. Watch tasks arrive at point B in staggered order - task 0 after 1 second, task 4 after 5 seconds - while the barrier holds the early arrivers until the slowest one shows up. Once all six signal (five tasks + main thread), the `postPhaseAction` fires, everyone is released, and odd tasks quietly leave.

The `+1` in `new Barrier(Participants + 1, ...)` is mandatory and non-negotiable. The main thread calls `SignalAndWait()` too - it's a participant, not a spectator. Construct the barrier with 5 and the main thread's signal is the sixth call in a five-participant phase, immediately throwing `InvalidOperationException`. Construct it with 7 and every phase hangs forever waiting for a participant that doesn't exist. **The count must exactly match the number of things that call `SignalAndWait()`.**

The second constructor argument is a `postPhaseAction` - a callback that fires once per completed phase, after every participant has signaled but before any of them are released. It runs on exactly one thread with everyone stopped, which makes it the one safe place to touch shared state without synchronization.

`SignalAndWait()` and `RemoveParticipant()` are mutually exclusive at a given rendezvous point. `SignalAndWait()` means "I've arrived, and I'll be back for the next phase." `RemoveParticipant()` means "I'm done permanently - stop counting me." A task that called `RemoveParticipant()` cannot call `SignalAndWait()` again without first rejoining via `AddParticipant()`. The continuation code for odd-numbered tasks belongs entirely inside the `else` branch - once a task removes itself, it has no further participation, and any subsequent `SignalAndWait()` call throws `InvalidOperationException`.

### Mini-Program 2: UseBarrierWithCancel() (Optional)

This one blocks on `Console.ReadLine()` waiting for you to trigger cancellation, so it's left out of the automatic run. Uncomment it in `Main()` when you're ready to explore it manually.

```csharp
var tokenSource = new CancellationTokenSource();

var barrier = new Barrier(Participants + 1,
    b => Console.WriteLine($"{b.ParticipantCount - 1} participants are at rendezvous point {b.CurrentPhaseNumber + 1}"));

for (int i = 0; i < Participants; i++)
{
    int localCopy = i;

    Task.Run(() =>
    {
        try
        {
            Console.WriteLine($"Task {localCopy} left point A...");
            Nap(1);

            if (localCopy % 2 == 0)
            {
                Console.WriteLine($"Task {localCopy} arrived at point B...");
                barrier.SignalAndWait(tokenSource.Token);

                Nap(1);
                Console.WriteLine($"Task {localCopy} arrived at point C...");
                barrier.SignalAndWait(tokenSource.Token);
            }
            else
            {
                Console.WriteLine($"Task {localCopy} signaled but returned to point A...");
                barrier.RemoveParticipant();
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation is an expected outcome, not a failure - swallowing it here is intentional
        }
    });
}

Console.WriteLine($"Main thread is waiting for {barrier.ParticipantsRemaining - 1} participants...\n");
Console.WriteLine("Press <ENTER> at any time to cancel...\n");
Console.ReadLine();

if (barrier.CurrentPhaseNumber < 1)
{
    tokenSource.Cancel();
    Console.WriteLine("\nOperation canceled...\n");
}
else
{
    Console.WriteLine("Too late to cancel...");
}

Nap(Participants);
Console.WriteLine("\nMain thread complete\n");
GenericFunctions.Pause();
```

`SignalAndWait(token)` throws `OperationCanceledException` when the token is cancelled, instead of blocking forever. Each task catches it and exits cleanly.

The empty `catch (OperationCanceledException) { }` is one of the rare defensible uses of swallowing an exception. Cancellation is an expected outcome with an explicit comment saying so.

`tokenSource.Cancel()` only fires if `barrier.CurrentPhaseNumber < 1` - if phase 0 hasn't completed yet. Once a phase has committed, refusing to signal leaves other participants waiting at `SignalAndWait` forever. Knowing when cancellation is still safe is part of designing for it; the token alone doesn't make an operation safely cancellable.

.NET cancellation is always cooperative. `Cancel()` sets a flag. Nothing stops forcibly - your code has to check the flag and bail out voluntarily.

---

## Summary: Barrier vs. Prior Primitives

| Primitive | One-shot? | Resets automatically? | Participant drop-out? | Phase callback? |
|---|---|---|---|---|
| `EventWaitHandle` | Yes | `AutoReset` only | No | No |
| `CountdownEvent` | Yes | No (must reset manually) | No | No |
| `Task.WaitAll` | Yes | N/A | No | No |
| `Barrier` | No - reusable across phases | Yes, after every phase | Yes, via `RemoveParticipant` | Yes, `postPhaseAction` |

The elapsed time for a `Barrier`-coordinated group is determined by the slowest participant in each phase - same as `Task.WaitAll`, but repeating across however many phases are needed. `Barrier` doesn't improve speed; it improves the expressibility and safety of multi-phase coordination.

---

## Takeaways

- A `Barrier` is a repeating rendezvous. Every other primitive so far was one-shot.
- The participant count must exactly match the number of `SignalAndWait()` callers. Too few throws; too many deadlocks.
- The main thread is a participant if it signals, and must be counted.
- `postPhaseAction` runs once per phase with everyone stopped - the safe place to touch shared state.
- `SignalAndWait()` is per-phase; `RemoveParticipant()` is permanent. They are mutually exclusive at a given point.
- A removed participant that signals again throws `InvalidOperationException`.
- Fire-and-forget tasks swallow exceptions. A broken task can look exactly like a working one.
- Retaining task handles and calling `WaitAll` surfaces hidden failures and eliminates guessed sleep durations.
- .NET cancellation is cooperative. `Cancel()` sets a flag; your code decides what to do with it.
- An empty `catch` is defensible when the exception is an expected outcome and the code says so.
