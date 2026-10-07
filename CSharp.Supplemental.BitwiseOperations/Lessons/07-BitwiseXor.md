---
title: "Bitwise XOR"
chapter: 0
index: 7
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int a = 0b10011100; // 156
        int b = 0b00110100; //  52
        Console.WriteLine($"  156 ^ 52 = {a ^ b}");
        Console.WriteLine($"  Binary:  10011100");
        Console.WriteLine($"         ^ 00110100");
        Console.WriteLine($"         = 10101000  (168)");
        Console.WriteLine();
        Console.WriteLine("XOR outputs 1 where inputs DIFFER - 0 where they are the same.");
        Console.WriteLine();

        // Practical use: swap two variables without a temporary
        int x = 5, y = 8;
        Console.WriteLine($"Before swap: x = {x}, y = {y}");
        x ^= y; y ^= x; x ^= y;
        Console.WriteLine($"After swap:  x = {x}, y = {y}");
        Console.WriteLine();
        Console.WriteLine("The XOR swap works because applying XOR twice with the same value restores the original.");
    }
}
```
