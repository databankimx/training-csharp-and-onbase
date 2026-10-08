---
title: "Bitwise AND"
chapter: 0
index: 2
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
        Console.WriteLine($"  156 & 52 = {a & b}");
        Console.WriteLine($"  Binary:  10011100");
        Console.WriteLine($"         & 00110100");
        Console.WriteLine($"         = 00010100  (20)");
        Console.WriteLine();
        Console.WriteLine("AND outputs 1 only where BOTH inputs are 1.");
        Console.WriteLine("Common uses: masking bits, checking flags, testing even/odd.");
    }
}
```
