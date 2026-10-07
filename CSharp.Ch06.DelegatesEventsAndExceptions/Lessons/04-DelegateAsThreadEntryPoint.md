---
title: "Delegate as Thread Entry Point"
chapter: 6
index: 4
dependencies: []
---

```csharp
using System;
using System.Threading;

internal static class Program
{
    private static void Main()
    {
        // Thread's constructor takes a ThreadStart delegate (no params, returns void).
        // The anonymous method satisfies that and becomes the thread's entry point.
        // Compare to Supplemental 06, which uses ParameterizedThreadStart to pass
        // an argument into the thread entry point instead.
        var t1 = new Thread(delegate ()
        {
            Thread.Sleep(300);
            Console.WriteLine("Thread: Hello World (from a background thread)");
        });
        t1.Start();

        // Start() returns immediately -- the thread is scheduled but not waited for.
        // This line runs on the main thread while t1 sleeps.
        Console.WriteLine("Main:   Running concurrently...");

        t1.Join(); // wait so the output isn't swallowed on program exit
        Console.WriteLine("Main:   Thread finished.");
    }
}
```
