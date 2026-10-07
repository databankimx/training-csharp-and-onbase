---
title: "Bitwise NOT"
chapter: 0
index: 6
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte b = 0b10011100;  // 156
        byte n = (byte)~b;    //  99
        Console.WriteLine($"~156 = {n}  (every bit flipped)");
        Console.WriteLine($"  Binary: 10011100 -> 01100011");
        Console.WriteLine();

        // Practical use: SetBit - uses NOT to clear a specific bit position
        Console.WriteLine("Setting individual bits in a byte:");
        byte word = 0;
        for (byte pos = 0; pos < 8; pos++)
            Console.WriteLine($"  SetBit(0, {pos}, 1) = {SetBit(word, pos, 1)}");
    }

    // Sets or clears the bit at the given position.
    // Uses NOT (~) to flip the mask for clearing: AND with ~mask zeroes the target bit.
    private static byte SetBit(byte word, byte pos, byte value)
    {
        int mask = 1 << pos;
        if (value == 0) return (byte)(word & ~mask); // clear
        if (value == 1) return (byte)(word | mask);  // set
        return word;
    }
}
```
