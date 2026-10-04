---
title: "Enums"
chapter: 3
index: 6
dependencies: []
---

```csharp
using System;

public enum Months : byte
{
    Jan = 1,
    Feb,
    Mar,
    Apr,
    May,
    Jun,
    Jul,
    Aug,
    Sep,
    Oct,
    Nov,
    Dec
}

internal static class Program
{
    private static void Main()
    {
        if (Enum.TryParse("Jul", out Months selected))
            Console.WriteLine($"Jul is month {(byte)selected}");

        Console.WriteLine($"The 8th month is {Enum.GetName(typeof(Months), 8)}");
    }
}
```
