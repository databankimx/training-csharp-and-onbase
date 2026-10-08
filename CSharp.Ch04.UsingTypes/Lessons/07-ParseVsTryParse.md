---
title: "Parse vs. TryParse"
chapter: 4
index: 7
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        string numString = "10";
        int number = int.Parse(numString);
        Console.WriteLine($"Parsed: {number}");

        if (int.TryParse("ten", out number))
            Console.WriteLine($"parses to int [{number}]...");
        else
            Console.WriteLine("cannot be parsed to int...");
    }
}
```
