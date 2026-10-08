---
title: "Barrier - A Repeating Multi-Phase Rendezvous"
chapter: 7
index: 1
dependencies: []
---

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

internal static class Program
{
    private const int Participants = 5;

    private static void Main()
    {
        // +1 because the main thread is also a participant and calls SignalAndWait().
        // Too few: main thread's signal exceeds the count -> InvalidOperationException.
        // Too many: phase never completes because a ghost participant never signals -> deadlock.
        // The count must exactly match the number of things that call SignalAndWait().
        var barrier = new Barrier(Participants + 1,
            b =>
            {
                // postPhaseAction fires once per completed phase with everyone stopped.
                // It is the one safe place to touch shared state without synchronization.
                Console.WriteLine($"{b.ParticipantCount - 1} participants reached rendezvous {b.CurrentPhaseNumber + 1}");
            });

        for (int i = 0; i < Participants; i++)
        {
            int localCopy = i; // per-iteration capture (Ch06 closure lesson)

            Task.Run(() =>
            {
                Console.WriteLine($"Task {localCopy} left point A...");
                Thread.Sleep((localCopy + 1) * 1000); // stagger arrivals

                if (localCopy % 2 == 0)
                {
                    // Even tasks go the full distance (phase 1 AND phase 2)
                    Console.WriteLine($"Task {localCopy} arrived at point B...");
                    barrier.SignalAndWait(); // phase 1

                    Thread.Sleep((Participants - localCopy) * 1000);
                    Console.WriteLine($"Task {localCopy} arrived at point C...");
                    barrier.SignalAndWait(); // phase 2
                }
                else
                {
                    // Odd tasks drop out permanently -- RemoveParticipant() and SignalAndWait()
                    // are mutually exclusive at a given point. A removed task must NOT call
                    // SignalAndWait() again or it throws InvalidOperationException.
                    Console.WriteLine($"Task {localCopy} signaled but returned to point A...");
                    barrier.RemoveParticipant();
                }
            });
        }

        Console.WriteLine($"Main thread waiting for {barrier.ParticipantsRemaining - 1} participants...\n");
        barrier.SignalAndWait(); // main thread, phase 1
        Console.WriteLine("\nMain thread signaled phase B...\n");
        barrier.SignalAndWait(); // main thread, phase 2
        Console.WriteLine("\nMain thread signaled phase C...\n");

        Thread.Sleep(Participants * 1000); // let fire-and-forget tasks finish printing
        Console.WriteLine("\nMain thread complete.\n");
    }
}
```
