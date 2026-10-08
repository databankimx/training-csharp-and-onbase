---
title: "Anonymous Method as Thread Entry Point"
chapter: 6
index: 5
dependencies: []
---

```csharp
using System;
using System.Threading;

internal static class Program
{
    private static void Main()
    {
        // Thread takes a ThreadStart delegate (no params, returns void).
        // The anonymous method here satisfies that -- a second thread starts,
        // sleeps, then prints. Main doesn't wait for it: "Step 2..." appears first.
        var thread = new Thread(delegate ()
        {
            Thread.Sleep(500);
            Console.WriteLine("Step 1...");
        });
        thread.Start();
        Console.WriteLine("Step 2...");

        thread.Join(); // wait so output isn't cut off on exit
    }
}
```
