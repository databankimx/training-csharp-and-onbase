---
title: "Increment and Decrement"
chapter: 2
index: 17
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int a = 0;
        Console.WriteLine($"a = {a}");

        a = a + 1;
        Console.WriteLine($"a = {a}");
        a += 1;
        Console.WriteLine($"a = {a}");
        a++;
        Console.WriteLine($"a = {a}");
        ++a;
        Console.WriteLine($"a = {a}");

        a = a - 1;
        Console.WriteLine($"a = {a}");
        a -= 1;
        Console.WriteLine($"a = {a}");
        a--;
        Console.WriteLine($"a = {a}");
        --a;
        Console.WriteLine($"a = {a}");

        Console.WriteLine("Prefix");
        Console.WriteLine($"a = {++a}");
        Console.WriteLine($"a = {a}");

        Console.WriteLine("Postfix");
        Console.WriteLine($"a = {a++}");
        Console.WriteLine($"a = {a}");

        Console.WriteLine($"{Environment.NewLine}Using postfix in a for loop iterator...");
        for (int i = 0; i < 5; i++)
        {
            Console.Write($"{(i > 0 ? ", " : "")}{i}");
        }

        Console.WriteLine($"{Environment.NewLine}Using prefix in a for loop iterator...");
        for (int i = 0; i < 5; ++i)
        {
            Console.Write($"{(i > 0 ? ", " : "")}{i}");
        }
        Console.WriteLine();
    }
}
```
