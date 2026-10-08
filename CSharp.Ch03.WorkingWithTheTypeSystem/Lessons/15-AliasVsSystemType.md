---
title: "Alias vs System Type"
chapter: 3
index: 15
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        System.Int32 mySystemInt = new();
        Console.WriteLine($"My System int is [{mySystemInt}]"); // 0
    }
}
```
