---
title: "Checking Even/Odd with AND"
chapter: 0
index: 3
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        foreach (int n in new[] { 42, 73 })
        {
            Console.WriteLine($"Using 'n % 2', {n} is {(n % 2 == 0 ? "even" : "odd")}");
            Console.WriteLine($"Using 'n & 1', {n} is {((n & 1) == 0 ? "even" : "odd")}");
            Console.WriteLine();
        }

        Console.WriteLine("The lowest bit of any integer is 1 for odd numbers, 0 for even.");
        Console.WriteLine("AND-ing with 1 isolates that bit - no division required.");
    }
}
```
