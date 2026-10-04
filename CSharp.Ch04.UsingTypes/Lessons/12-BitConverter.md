---
title: "BitConverter"
chapter: 4
index: 12
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int packedValue = 42;
        byte[] packedBytes = BitConverter.GetBytes(packedValue);

        Console.WriteLine(string.Join(" ", packedBytes));
        Console.WriteLine(BitConverter.IsLittleEndian);

        int unpacked = BitConverter.ToInt32(packedBytes, 0);
        Console.WriteLine(unpacked);
    }
}
```
