---
title: "Integer Overflow"
chapter: 0
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte b = 255;
        b++;
        Console.WriteLine($"(byte)  255 + 1 = {b}  (wraps to 0 - only 8 bits available)");

        sbyte s = 127;
        s++;
        Console.WriteLine($"(sbyte) 127 + 1 = {s}  (wraps to -128 - the sign bit flips)");

        Console.WriteLine();
        Console.WriteLine("Overflow wraps around silently by default in C#.");
        Console.WriteLine("Use 'checked' to throw an OverflowException instead:");

        try
        {
            checked
            {
                byte c = 255;
                c++;
            }
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"  checked block: {ex.Message}");
        }
    }
}
```
