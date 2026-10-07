---
title: "Wrap-Around and Overflow"
chapter: 3
index: 16
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // Part 1: short wraps around at its boundary
        short num = 0;
        do
        {
            num++;
            if (num > 32766 || num < 0) Console.WriteLine($"num = {num}");
            if (num < 0) break;
        } while (num <= 32767);

        // Part 2: shifting into the sign bit produces a negative number
        int x = 1;
        for (int i = 1; i < 32; i++)
        {
            x <<= 1;
        }
        Console.WriteLine(x); // negative, because bit 31 is the sign bit

        // Part 3: checked blocks make overflow throw instead of wrap
        try
        {
            checked
            {
                int max = int.MaxValue;
                max += 1; // throws OverflowException
            }
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"Caught: {ex.Message}");
        }
    }
}
```
