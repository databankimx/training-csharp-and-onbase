---
title: "Widening and Narrowing"
chapter: 4
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte b = 127;
        int i = (int)b;
        Console.WriteLine($"byte to int: {i}");

        i = 64;
        b = (byte)i;
        Console.WriteLine($"int to byte: {b}");
    }
}
```
