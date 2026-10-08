---
title: "Conditional Operators - Bitwise"
chapter: 2
index: 5
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte expr1 = 15; // Binary 00001111
        byte expr2 = 10; // Binary 00001010

        Console.WriteLine($"expr1 = {Convert.ToString(expr1, 2).PadLeft(8, '0')} = {expr1}");
        Console.WriteLine($"expr2 = {Convert.ToString(expr2, 2).PadLeft(8, '0')} = {expr2}");
        Console.WriteLine($"expr1 & expr2 = {Convert.ToString(expr1 & expr2, 2).PadLeft(8, '0')} = {expr1 & expr2}");
        Console.WriteLine($"expr1 | expr2 = {Convert.ToString(expr1 | expr2, 2).PadLeft(8, '0')} = {expr1 | expr2}");
        Console.WriteLine($"expr1 ^ expr2 = {Convert.ToString(expr1 ^ expr2, 2).PadLeft(8, '0')} = {expr1 ^ expr2}");
        Console.WriteLine($"~expr1 = {Convert.ToString((byte)~expr1, 2).PadLeft(8, '0')} = {(byte)~expr1}");
        Console.WriteLine($"~expr2 = {Convert.ToString((byte)~expr2, 2).PadLeft(8, '0')} = {(byte)~expr2}");
    }
}
```
