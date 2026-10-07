---
title: "ToString With Format and Culture"
chapter: 4
index: 27
dependencies: []
---

```csharp
using System;
using System.Globalization;

internal static class Program
{
    private static void Main()
    {
        double d = 12345.67890;
        Console.WriteLine(d.ToString());
        Console.WriteLine(d.ToString(CultureInfo.InvariantCulture));

        Console.WriteLine(d.ToString("c"));
        Console.WriteLine(d.ToString("c", CultureInfo.CreateSpecificCulture("en-US")));
        Console.WriteLine(d.ToString("c", CultureInfo.CreateSpecificCulture("en-GB")));

        int i = 1234567890;
        Console.WriteLine(i.ToString("0,0"));
        Console.WriteLine(d.ToString("0,0.00"));
    }
}
```
