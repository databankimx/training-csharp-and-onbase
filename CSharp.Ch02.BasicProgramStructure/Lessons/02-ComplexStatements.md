---
title: "Complex Statements"
chapter: 2
index: 2
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int[] numbers = { 5, 24, 36, 19, 45, 60, 78 };
        int evenNums = 0;

        foreach (int num in numbers)
        {
            Console.WriteLine($"num = {num}");
            if (num % 2 == 0)
            {
                evenNums++;
            }
        }

        Console.WriteLine($"Found {evenNums} even number{(evenNums == 1 ? "" : "s")}");
    }
}
```
