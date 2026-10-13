---
title: "Ghost Suggestion: Simple Helper"
chapter: 0
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine(AddInt(2, 3));
        Console.WriteLine(AddInt(-4, 10));
    }

    private static int AddInt(int a, int b)
    {
        return a + b;
    }
}
```
