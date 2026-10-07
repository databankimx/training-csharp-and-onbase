---
title: "Narrowing That Doesn't Fit"
chapter: 4
index: 2
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int i = 264;
        byte b = (byte)i;
        Console.WriteLine($"int to byte with invalid value ({i}): {b}");
    }
}
```
