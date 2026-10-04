---
title: "Conditional Operators - Relational"
chapter: 2
index: 3
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        const bool myConditionResult = false;
        Console.WriteLine($"myConditionResult = {myConditionResult}");

        byte expr1 = 1;
        Console.WriteLine($"expr1 = {expr1}");
        byte expr2 = 2;
        Console.WriteLine($"expr2 = {expr2}");
        Console.WriteLine($"expr1 < expr2 ? {expr1 < expr2}");
        Console.WriteLine($"expr1 > expr2 ? {expr1 > expr2}");
        Console.WriteLine($"expr1 <= expr2 ? {expr1 <= expr2}");
        Console.WriteLine($"expr1 >= expr2 ? {expr1 >= expr2}");
        Console.WriteLine($"expr1 == expr2 ? {expr1 == expr2}");
        Console.WriteLine($"expr1 != expr2 ? {expr1 != expr2}");
    }
}
```
