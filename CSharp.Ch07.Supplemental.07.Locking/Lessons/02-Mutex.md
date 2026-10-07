---
title: "Mutex - Cross-Process, Thread Affinity, Timeout"
chapter: 7
index: 2
dependencies: []
---

```csharp
using System;
using System.Threading;

internal static class Program
{
    private static Mutex _mutex = new Mutex();
    private static string _resourceOwner;

    private static void UseResourceWithMutex()
    {
        Console.WriteLine($"{Thread.CurrentThread.Name} requesting the mutex...");

        // WaitOne(ms): give up after 5s instead of blocking forever.
        // Converts a silent hang into a handleable event, even if it never fires in testing.
        if (_mutex.WaitOne(5000))
        {
            try
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} has control...");
                _resourceOwner = Thread.CurrentThread.Name;
                Thread.Sleep(2000);
                Console.WriteLine($"{Thread.CurrentThread.Name} done (owned by: {_resourceOwner})...");
            }
            finally
            {
                // Thread affinity: only the acquiring thread may call ReleaseMutex().
                // Calling from a different thread throws ApplicationException.
                // This rules out acquiring in one place and releasing in a ContinueWith.
                _mutex.ReleaseMutex();
                Console.WriteLine($"{Thread.CurrentThread.Name} released the mutex...");
            }
        }
        else
        {
            Console.WriteLine($"{Thread.CurrentThread.Name} timed out acquiring the mutex...");
        }
    }

    private static void Main()
    {
        // Naming threads costs nothing and is invaluable in a debugger
        for (int i = 0; i < 3; i++)
        {
            var t = new Thread(UseResourceWithMutex) { Name = $"Thread {i + 1}" };
            t.Start();
        }

        Thread.Sleep(7000); // allow all three threads to complete
        Console.WriteLine();
        Console.WriteLine("Mutex vs. Monitor:");
        Console.WriteLine("  Mutex:   can be named (cross-process); thread affinity; kernel-level cost.");
        Console.WriteLine("  Monitor: in-process only; reentrant; cheapest when uncontended.");
        Console.WriteLine("Default to lock/Monitor. Use Mutex only when cross-process is genuinely needed.");
    }
}
```
