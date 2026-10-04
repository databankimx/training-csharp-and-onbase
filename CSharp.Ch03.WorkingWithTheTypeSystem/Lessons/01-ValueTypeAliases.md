---
title: "Value Type Aliases"
chapter: 3
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int myInt = 0;
        int myNewInt = new();

        System.Int32 myInt32 = new();

        Console.WriteLine($"myInt = {myInt}");
        Console.WriteLine($"myNewInt = {myNewInt}");
        Console.WriteLine($"myInt32 = {myInt32}");
    }
}
```
