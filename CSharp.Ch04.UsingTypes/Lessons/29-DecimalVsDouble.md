---
title: "decimal vs. double - Why Money Needs decimal"
chapter: 4
index: 29
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // double cannot represent 0.1 exactly in binary
        double d = 0.1 + 0.2;
        Console.WriteLine(d); // 0.30000000000000004

        // Accumulated error over repeated additions
        double total = 0;
        for (int i = 0; i < 10; i++) total += 0.1;
        Console.WriteLine(total);
        Console.WriteLine(total == 1.0 ? "Equal" : "Not Equal");

        // decimal is base-10 -- 0.1m is exact
        decimal totalM = 0;
        for (int i = 0; i < 10; i++) totalM += 0.1m;
        Console.WriteLine(totalM == 1.0m ? "Equal" : "Not Equal");

        // Predictable rounding with explicit strategy
        decimal roundPrice = 19.995m;
        decimal rounded = Math.Round(roundPrice, 2, MidpointRounding.ToEven);
        Console.WriteLine(rounded);
    }
}
```
