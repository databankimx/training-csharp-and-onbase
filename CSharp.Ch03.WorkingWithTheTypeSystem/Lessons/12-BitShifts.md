---
title: "Bit Shifts"
chapter: 3
index: 12
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int ig = 1;
        Console.WriteLine("0x{0:x}", ig << 1); // 2 -- int shifted left by 1

        long lg = 1;
        Console.WriteLine("0x{0:x}", lg << 33); // 0x200000000 -- long shifted left by 33
        // Note: shifting an int left by 33 would reduce to shift-by-1 (33 mod 32)
        // because int is only 32 bits wide. long is 64 bits, so 33 is a valid shift.
    }
}
```
