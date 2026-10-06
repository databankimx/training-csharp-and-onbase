---
title: "Assigning and Reassigning a Delegate"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private delegate void Printer(string data);

    private static void DoWork(string data) => Console.WriteLine(data);

    private static void Main()
    {
        // Console.WriteLine is a named method -- stored WITHOUT parentheses
        Printer p = Console.WriteLine;
        p("The delegate using an anonymous method was called.");

        // Reassign to an entirely different named method -- same signature, different type
        p = DoWork;
        p("The delegate using a named method was called.");

        // The label in the first string is just a string -- it describes intent, not mechanism.
        // Both assignments here are named methods; the real lesson is that the variable
        // accepted both because they share the same signature: (string) -> void.
    }
}
```
