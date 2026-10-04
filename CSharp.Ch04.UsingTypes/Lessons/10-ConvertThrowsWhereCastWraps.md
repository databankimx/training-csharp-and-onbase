---
title: "Convert Throws Where a Cast Wraps"
chapter: 4
index: 10
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // Cast silently wraps -- no exception
        int i = 264;
        byte castResult = (byte)i;
        Console.WriteLine($"(byte)264 = {castResult}"); // 8, silently wrong

        // Convert throws instead
        double income = 300.00;
        try
        {
            byte tooSmall = Convert.ToByte(income);
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"Convert.ToByte(300.00): {ex.Message}");
        }
    }
}
```
