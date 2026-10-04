---
title: "A Catalog of For Loop Shapes"
chapter: 2
index: 14
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        for (int i = 0; i < 10; i++)  Console.WriteLine($"i = {i}");  // count up by one
        for (int i = 10; i > 0; i--) Console.WriteLine($"i = {i}");  // count down by one
        for (int i = 0; i < 10; i += 2) Console.WriteLine($"i = {i}"); // count up by two
        for (int i = 5; i < 1000; i *= 5) Console.WriteLine($"i = {i}"); // count up by multiples of five
    }
}
```
