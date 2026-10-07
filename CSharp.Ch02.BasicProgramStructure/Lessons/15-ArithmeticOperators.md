---
title: "Arithmetic Operators"
chapter: 2
index: 15
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int a = 4;
        int b = 2;
        Console.WriteLine($"a = {a} and b = {b}");

        int c = +1;
        Console.WriteLine($"c = {c}");
        int d = -1;
        Console.WriteLine($"d = {d}");

        c = a + b;
        Console.WriteLine($"{a} + {b} = {c}");
        c = a - b;
        Console.WriteLine($"{a} - {b} = {c}");
        c = a * b;
        Console.WriteLine($"{a} * {b} = {c}");
        c = a / b;
        Console.WriteLine($"{a} / {b} = {c}");

        c = 5;
        Console.Write($"{c} += 5 yields ");
        c += 5;
        Console.WriteLine(c);
        Console.Write($"{c} -= 5 yields ");
        c -= 5;
        Console.WriteLine(c);
        Console.Write($"{c} *= 2 yields ");
        c *= 2;
        Console.WriteLine(c);
        Console.Write($"{c} /= 2 yields ");
        c /= 2;
        Console.WriteLine(c);

        d = c % b;
        Console.WriteLine($"{c} % {b} = {d}");
    }
}
```
