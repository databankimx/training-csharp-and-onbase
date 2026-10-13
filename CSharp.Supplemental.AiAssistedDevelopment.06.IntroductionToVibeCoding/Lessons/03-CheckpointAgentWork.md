---
title: "Checkpoint Agent Work"
chapter: 0
index: 3
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        var checkpoints = new List<Checkpoint>
        {
            new Checkpoint("Plan reviewed", true),
            new Checkpoint("Implementation scoped", true),
            new Checkpoint("Build passed", true),
            new Checkpoint("Tests passed", true),
            new Checkpoint("Human review complete", false),
        };

        foreach (var checkpoint in checkpoints)
            Console.WriteLine($"{checkpoint.Name}: {(checkpoint.Passed ? "pass" : "not yet")}");

        Console.WriteLine(IsReadyToShip(checkpoints) ? "Ready to ship." : "Do not ship yet.");
    }

    private static bool IsReadyToShip(IEnumerable<Checkpoint> checkpoints)
    {
        foreach (var checkpoint in checkpoints)
            if (!checkpoint.Passed)
                return false;

        return true;
    }

    private sealed class Checkpoint
    {
        public Checkpoint(string name, bool passed)
        {
            Name = name;
            Passed = passed;
        }

        public string Name { get; }
        public bool Passed { get; }
    }
}
```
