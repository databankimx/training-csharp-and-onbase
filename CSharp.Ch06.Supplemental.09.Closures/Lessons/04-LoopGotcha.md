---
title: "Access to Modified Closure - The Loop Gotcha"
chapter: 6
index: 4
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        // The modified closure bug in its most famous costume: a for loop.
        // Every lambda closes over the SAME i -- not a copy of what i was at the time.
        // By the time any action runs, the loop is done and i has settled on 3.
        var broken = new List<Action>();
        for (int i = 0; i < 3; i++)
            broken.Add(() => Console.Write(i + " "));

        Console.Write("Broken (for loop, shared i):  ");
        foreach (var a in broken) a();
        Console.WriteLine();  // 3 3 3

        // Fix: declare a fresh local variable on each iteration.
        // Each closure gets its own independent copy.
        var fixed1 = new List<Action>();
        for (int i = 0; i < 3; i++)
        {
            int local = i;
            fixed1.Add(() => Console.Write(local + " "));
        }

        Console.Write("Fixed  (local copy per iter): ");
        foreach (var a in fixed1) a();
        Console.WriteLine();  // 0 1 2

        // foreach has been immune to this since C# 5 -- the loop variable is
        // already scoped per iteration. Only for/while and explicit reuse of a
        // single variable across iterations will catch you.
        int[] values = [0, 1, 2];
        var fixed2 = new List<Action>();
        foreach (int v in values)
            fixed2.Add(() => Console.Write(v + " "));

        Console.Write("foreach (immune since C# 5):  ");
        foreach (var a in fixed2) a();
        Console.WriteLine();  // 0 1 2
    }
}
```
