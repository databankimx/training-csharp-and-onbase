---
title: "Expression and Statement Lambdas"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // Zero parameters -- empty () is required (unlike anonymous methods, which can omit the list)
        Action note = () => Console.WriteLine("Parameterless lambda called.");
        note();

        // One parameter -- parentheses optional, type inferred from Action<string>
        Action<string> greet = message => Console.WriteLine($"Hello, {message}!");
        greet("world");

        // Multiple parameters -- parentheses required
        Action<string, int> label = (text, n) => Console.WriteLine($"{n}: {text}");
        label("first",  1);
        label("second", 2);

        // Expression lambda returning a value -- the expression IS the return value, no return keyword
        Func<float, float> square = x => x * x;
        Console.WriteLine($"2 squared is {square(2)}");
        Console.WriteLine($"5 squared is {square(5)}");

        // Statement lambda -- braces and explicit return required when body has multiple statements
        Func<float, int, float> power = (x, y) =>
        {
            float z = x;
            for (int i = 1; i < y; i++) z *= x;
            return z;
        };
        Console.WriteLine($"2 to the 3 = {power(2, 3)}");
        Console.WriteLine($"3 to the 4 = {power(3, 4)}");
    }
}
```
