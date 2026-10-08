---
title: "System.Convert and Banker's Rounding"
chapter: 4
index: 9
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        double income = 9.50;
        Console.WriteLine(Convert.ToInt32(income)); // 10

        income = 10.50;
        Console.WriteLine(Convert.ToInt32(income)); // 10, not 11 -- banker's rounding

        // If you need "always round .5 up":
        Console.WriteLine(Math.Round(income, MidpointRounding.AwayFromZero));
    }
}
```
