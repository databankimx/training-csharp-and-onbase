---
title: "Conditional Operators - The Assignment Gotcha"
chapter: 2
index: 4
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte expr1 = 1;
        byte expr2 = 2;

        Console.WriteLine($"expr1 = {expr1}");
        Console.WriteLine($"expr2 = {expr2}");
        Console.WriteLine($"expr1 < expr2 ? {expr1 < expr2}");
        Console.WriteLine($"expr1 > expr2 ? {expr1 > expr2}");
        Console.WriteLine($"expr1 <= expr2 ? {expr1 <= expr2}");
        Console.WriteLine($"expr1 >= expr2 ? {expr1 >= expr2}");
        Console.WriteLine($"expr1 == expr2 ? {expr1 == expr2}");
        Console.WriteLine($"expr1 != expr2 ? {expr1 != expr2}");

        // One equals sign -- this is assignment, not comparison.
        // expr1 = expr2 is an expression that evaluates to expr2's value,
        // AND silently overwrites expr1 as a side effect.
        Console.WriteLine($"expr1 = expr2 ? {expr1 = expr2}");
        Console.WriteLine($"expr1 is now = {expr1}"); // damage proven
        Console.WriteLine($"expr2 is still = {expr2}");
    }
}
```
