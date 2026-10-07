---
title: "Bit Shifts"
chapter: 0
index: 9
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte x;

        x = 128;
        Console.WriteLine($"{x} >> 3 = {(byte)(x >> 3)}  (divide by 8)");

        x = 16;
        Console.WriteLine($"{x} << 3 = {(byte)(x << 3)}  (multiply by 8)");

        x = 64;
        Console.WriteLine($"{x} << 3 = {(byte)(x << 3)}  (overflow: ninth bit lost)");

        x = 4;
        Console.WriteLine($"{x} >> 3 = {(byte)(x >> 3)}  (underflow: shifted past ones place)");

        Console.WriteLine();
        Console.WriteLine("Left shift  (<<): equivalent to multiplying by 2^n (until overflow)");
        Console.WriteLine("Right shift (>>): equivalent to dividing by 2^n (until underflow)");
        Console.WriteLine("Shifts are faster than multiplication/division for powers of two.");
    }
}
```
