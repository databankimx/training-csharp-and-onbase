---
title: "Four Ways to Write the Same Delegate"
chapter: 6
index: 3
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private delegate void TestDelegate(string s);

    private static void M(string s) => Console.WriteLine(s);

    private static void Main()
    {
        // C# 1.0 -- explicit delegate constructor; only syntax that permits var on left
        var testDelA = new TestDelegate(M);

        // C# 2.0 -- anonymous method; parameter type must be written explicitly
        TestDelegate testDelB = delegate (string s) { Console.WriteLine(s); };

        // C# 3.0 -- lambda; type inferred from TestDelegate on the left
        TestDelegate testDelC = (x) => { Console.WriteLine(x); };

        // Method group conversion -- cleanest when a method already exists
        TestDelegate testDelD = Console.WriteLine;

        testDelA("A - original delegate constructor syntax");
        testDelB("B - anonymous method");
        testDelC("C - lambda expression");
        testDelD("D - method group conversion");

        // For new code: prefer D when a method exists, C without braces when writing inline.
        // A and B are legacy syntax you'll read in older code rather than write yourself.
    }
}
```
