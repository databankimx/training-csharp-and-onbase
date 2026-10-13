---
title: "Document a Method"
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
        var rows = new[]
        {
            "100.50",
            "not money",
            "42.25",
            "",
            "17.25"
        };

        Console.WriteLine(SumValidAmounts(rows));
    }

    /// <summary>
    /// Adds all rows that can be parsed as decimal currency amounts.
    /// </summary>
    /// <param name="rows">
    /// Raw amount rows. Null, empty, whitespace, and unparsable rows are ignored.
    /// </param>
    /// <returns>
    /// The sum of all valid decimal rows. Returns zero when no row can be parsed.
    /// </returns>
    private static decimal SumValidAmounts(IEnumerable<string> rows)
    {
        if (rows == null)
            throw new ArgumentNullException(nameof(rows));

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row))
            .Select(row => decimal.TryParse(row, out decimal value) ? value : 0m)
            .Sum();
    }
}
```
