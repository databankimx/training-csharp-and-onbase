---
title: "The Ternary Operator"
chapter: 2
index: 6
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte expr1 = 15;
        byte expr2 = 10;
        string result = expr1 > expr2 ? "" : "not ";
        Console.WriteLine($"{expr1} is {result}greater than {expr2}");
    }
}
```
