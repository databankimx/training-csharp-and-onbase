---
title: "Monitor - The lock Keyword Expanded"
chapter: 7
index: 1
dependencies: []
---

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

internal class Thing { public int Id { get; set; } }

internal static class Program
{
    private static void Main()
    {
        var syncObject = new Thing();

        for (int i = 0; i < 2; i++)
        {
            int iCopy = i;
            Task.Run(() =>
            {
                Console.WriteLine($"Start task {iCopy}...");

                // Monitor.Enter + try/finally is exactly what "lock (syncObject) { ... }" compiles to.
                // The try/finally is mandatory: if anything throws between Enter and Exit,
                // skipping Exit leaves the object permanently locked. Other threads block forever
                // with no error pointing at the cause. A leaked lock is worse than a leaked file handle.
                Monitor.Enter(syncObject);
                try
                {
                    Console.WriteLine($"Object locked by task {iCopy}...");
                    syncObject.Id = iCopy + 1;
                    Console.WriteLine($"Object's ID is now {syncObject.Id}...");
                    Thread.Sleep(2000);
                }
                finally
                {
                    Monitor.Exit(syncObject);
                    Console.WriteLine($"Object released by task {iCopy}...");
                }
            });
        }

        Thread.Sleep(5000); // allow both tasks to finish
        Console.WriteLine();
        Console.WriteLine("Use 'lock (obj) { }' in production code -- same expansion, try/finally impossible to forget.");
        Console.WriteLine("Never lock on: this, a public field, a Type, or a string literal.");
        Console.WriteLine("Every thread must lock on the SAME instance. Per-thread lock objects protect nothing.");
    }
}
```
