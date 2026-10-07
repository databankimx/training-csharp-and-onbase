---
title: "ParameterizedThreadStart vs. ThreadStart"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;
using System.Threading;

// Non-static class -- required so both a static and an instance method can coexist.
// A static class cannot be instantiated and cannot contain instance members.
internal class Program
{
    // Static method -- the resulting delegate has a null Target
    private static void DoWork(object data)
    {
        Console.WriteLine($"Static thread:   Data='{data}'");
    }

    // Instance method -- the delegate carries this Program instance as its target
    private void DoMoreWork(object data)
    {
        Console.WriteLine($"Instance thread: Data='{data}'");
    }

    private static void Main()
    {
        // ParameterizedThreadStart: void (object) -- Start() accepts a single argument
        var t1 = new Thread(DoWork);
        t1.Start(42);

        // Instance method variant -- requires an actual instance
        var program = new Program();
        var t2 = new Thread(program.DoMoreWork);
        t2.Start("The answer.");

        t1.Join();
        t2.Join();

        // The modern alternative avoids ParameterizedThreadStart's lack of type safety:
        int answer = 42;
        var t3 = new Thread(() => DoWork(answer)); // ThreadStart, fully typed via closure
        t3.Start();
        t3.Join();

        Console.WriteLine("\nParameterizedThreadStart is essentially a pre-closure workaround.");
        Console.WriteLine("Prefer a closure (or Task.Run) in new code.");
    }
}
```
