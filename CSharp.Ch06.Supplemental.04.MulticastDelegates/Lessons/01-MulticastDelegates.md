---
title: "Multicast Delegates - Combine, Invoke, Subtract"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;

internal delegate void CustomDel(string s);

internal static class Program
{
    private static void Hello(string s)   => Console.WriteLine($"Hello, {s}!");
    private static void Goodbye(string s) => Console.WriteLine($"Goodbye, {s}!");

    private static void Main()
    {
        CustomDel hiDel  = Hello;
        CustomDel byeDel = Goodbye;

        // + produces a new delegate; hiDel and byeDel are unchanged
        CustomDel multiDel        = hiDel + byeDel;
        // - also produces a new delegate
        CustomDel multiMinusHiDel = multiDel - hiDel;

        Console.WriteLine("Invoking hiDel:");
        hiDel("A");                     // one line

        Console.WriteLine("\nInvoking byeDel:");
        byeDel("B");                    // one line

        Console.WriteLine("\nInvoking multiDel:");
        multiDel("C");                  // TWO lines -- Hello then Goodbye, in order

        Console.WriteLine("\nInvoking multiMinusHiDel:");
        multiMinusHiDel("D");           // one line -- only Goodbye

        // Sharp edges (demonstrated via comments to avoid crashing the step):
        // 1. Return values: only the LAST result survives. Earlier ones are silently discarded.
        // 2. Exceptions: if any handler throws, the remaining handlers never run.
        // 3. -= can't remove a lambda you didn't store a reference to.
        Console.WriteLine("\nEvery delegate is a multicast delegate -- the invocation list");
        Console.WriteLine("is always there, even when it holds only one entry.");
    }
}
```
