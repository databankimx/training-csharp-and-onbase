---
title: "Exchange and CompareExchange"
chapter: 7
index: 2
dependencies: []
---

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

internal static class Program
{
    private static void Main()
    {
        // --- Add and Decrement ---
        int total = 0;
        Parallel.Invoke(
            () => Interlocked.Add(ref total, 10),
            () => Interlocked.Add(ref total, 20),
            () => Interlocked.Add(ref total, 30));
        Console.WriteLine($"Add: total = {total} (expected 60)");
        // Note: Increment/Decrement/Add all return the NEW value.

        // --- Exchange: atomically set a value and get the old one back ---
        // Useful for swapping whole objects (config reload, cache replacement)
        // so readers see either the complete old object or the complete new one.
        string leader = "Nobody";
        string prev = Interlocked.Exchange(ref leader, "Alice");
        Console.WriteLine($"\nExchange: was '{prev}', now '{leader}'");
        prev = Interlocked.Exchange(ref leader, "Bob");
        Console.WriteLine($"Exchange: was '{prev}', now '{leader}'");
        // Exchange returns the OLD value. That's the opposite of Increment/Add.

        // --- CompareExchange: "set this, but only if it still equals what I expect" ---
        // Argument order: (ref location, newValue, comparand)
        // new value comes BEFORE the value you're comparing against -- reads backwards.
        int flag = 0;
        int original = Interlocked.CompareExchange(ref flag, 1, 0); // if flag==0, set to 1
        Console.WriteLine($"\nCompareExchange 1st: was {original}, is {flag}, we set it: {original == 0}");

        original = Interlocked.CompareExchange(ref flag, 1, 0); // flag is now 1, won't swap
        Console.WriteLine($"CompareExchange 2nd: was {original}, is {flag}, we set it: {original == 0}");

        Console.WriteLine();
        Console.WriteLine("Return values to memorise:");
        Console.WriteLine("  Increment / Decrement / Add   -> return the NEW value");
        Console.WriteLine("  Exchange / CompareExchange     -> return the OLD value");
        Console.WriteLine("Getting this backwards compiles silently and produces wrong results.");
    }
}
```
