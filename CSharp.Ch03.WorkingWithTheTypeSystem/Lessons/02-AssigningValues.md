---
title: "Assigning Values"
chapter: 3
index: 2
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int myInt;
        int secondInt;

        myInt = 2;
        secondInt = myInt;

        Console.WriteLine($"myInt = {myInt}");
        Console.WriteLine($"secondInt = {secondInt}");
    }
}
```
