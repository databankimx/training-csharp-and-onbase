---
title: "Using Enums"
chapter: 3
index: 7
dependencies: ["06-Enums.md"]
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
        string name = Enum.GetName(typeof(Months), 8);
        Console.WriteLine("The 8th month in the enum is " + name);

        foreach (byte value in Enum.GetValues(typeof(Months)))
        {
            Console.WriteLine(value);
        }
    }
}
```
