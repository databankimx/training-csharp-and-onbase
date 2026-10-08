---
title: "Reconstructing Integers from Bytes"
chapter: 0
index: 11
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte[] bytes =
        {
            0b1101_0010, 0b0000_0010, 0b1001_0110, 0b0100_1001,
            0b0001_0101, 0b1100_1101, 0b0101_1011, 0b0000_0111
        };

        int[] values = ReconstructIntegers(bytes);
        Console.WriteLine("Reconstructed integers from 8 bytes (4 bytes each, little-endian):");
        foreach (int v in values)
            Console.WriteLine($"  {v:#,0}");

        Console.WriteLine();
        Console.WriteLine("Technique: shift each byte left by its position * 8, then OR together.");
        Console.WriteLine("Position 0 = no shift, position 1 = 8 bits, position 2 = 16 bits, etc.");
    }

    private static int[] ReconstructIntegers(byte[] bytes, int size = 4)
    {
        if (bytes.Length % size != 0)
            throw new ArgumentException("Invalid data length.", nameof(bytes));

        int count    = bytes.Length / size;
        int[] result = new int[count];

        for (int n = 0; n < count; n++)
        {
            int value = 0;
            for (int p = 0; p < size; p++)
                value += bytes[n * size + p] << (p * 8);
            result[n] = value;
        }
        return result;
    }
}
```
