---
title: "If, Else, and Else If"
chapter: 2
index: 8
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int x = 1;
        int y = 2;

        if (true) Console.WriteLine("This statement still executes.");

        if (x < y)
        {
            Console.WriteLine($"{x} is less than {y}");
        }

        x = 3;

        if (x > y)
        {
            Console.WriteLine($"{x} is greater than {y}");
        }
        else
        {
            Console.WriteLine($"{x} is not greater than {y}");
        }

        x = 2;

        if (x < y)
        {
            Console.WriteLine($"{x} is less than {y}");
        }
        else if (x > y)
        {
            Console.WriteLine($"{x} is greater than {y}");
        }
        else
        {
            Console.WriteLine($"{x} is equal to {y}");
        }
    }
}
```
