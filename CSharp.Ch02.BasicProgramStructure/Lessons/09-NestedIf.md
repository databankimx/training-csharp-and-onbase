---
title: "Nested If Statements"
chapter: 2
index: 9
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int first = 2;
        int second = 0;

        if (first == 2)
        {
            Console.WriteLine("The if statement evaluated to true");
        }
        Console.WriteLine("This line outputs regardless of the if condition");

        if (first == 2 && second == 0)
        {
            Console.WriteLine("The if statement evaluated to true");
        }
        Console.WriteLine("This line outputs regardless of the if condition");

        if (first == 2)
        {
            if (second == 0)
            {
                Console.WriteLine("Both outer and inner conditions are true.");
            }
            Console.WriteLine("Outer condition is true, inner may be true.");
        }
        Console.WriteLine("This line outputs regardless of the if condition");
    }
}
```
