---
title: "The checked Block"
chapter: 4
index: 3
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        try
        {
            checked
            {
                int i = 264;
                byte b = (byte)i;
                Console.WriteLine($"int to byte with invalid value ({i}): {b}");
            }
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"OverflowException: {ex.Message}");
        }
    }
}
```
