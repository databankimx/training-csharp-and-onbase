---
title: "decimal.Parse and NumberStyles"
chapter: 4
index: 8
dependencies: []
---

```csharp
using System;
using System.Globalization;

internal static class Program
{
    private static void Main()
    {
        string money = "1,000.00";
        Console.WriteLine(decimal.Parse(money));

        money = "$1,000.00";
        try
        {
            Console.WriteLine(decimal.Parse(money));
        }
        catch (FormatException ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine(decimal.Parse(money, NumberStyles.Currency));
    }
}
```
