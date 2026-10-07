---
title: "Bit Flags"
chapter: 3
index: 13
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte b = 73; // 01001001

        for (int i = 0; i < 8; i++)
        {
            bool isSet = (b & (1 << i)) != 0;
            Console.WriteLine($"Bit {i} is {(isSet ? "" : "not ")}set");
        }
    }
}
```
