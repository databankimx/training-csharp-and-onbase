---
title: "Convert.ChangeType"
chapter: 4
index: 11
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        double income = 10.50;
        int rounded = (int)Convert.ChangeType(income, typeof(int));
        Console.WriteLine(rounded);
    }
}
```
