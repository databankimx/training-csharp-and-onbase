---
title: "Refactor Without Changing Behavior"
chapter: 0
index: 1
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

internal static class Program
{
    private static void Main()
    {
        var values = new[] { 4, -2, 8, 0, 3 };

        int before = SumPositiveNumbersBefore(values);
        int after = SumPositiveNumbersAfter(values);

        Console.WriteLine($"Before: {before}");
        Console.WriteLine($"After:  {after}");
        Console.WriteLine(before == after ? "Behavior preserved." : "Behavior changed.");
    }

    private static int SumPositiveNumbersBefore(IEnumerable<int> values)
    {
        int total = 0;
        foreach (int value in values)
        {
            if (value > 0)
            {
                total = total + value;
            }
        }
        return total;
    }

    private static int SumPositiveNumbersAfter(IEnumerable<int> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        return values.Where(value => value > 0).Sum();
    }
}
```
