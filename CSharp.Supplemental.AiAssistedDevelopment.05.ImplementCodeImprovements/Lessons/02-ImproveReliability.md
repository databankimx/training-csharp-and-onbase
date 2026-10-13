---
title: "Improve Reliability"
chapter: 0
index: 2
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        var rows = new[] { "1,Avery", "bad", "2,Blair", "3," };

        foreach (var result in ParseRows(rows))
            Console.WriteLine(result);
    }

    private static IEnumerable<string> ParseRows(IEnumerable<string> rows)
    {
        foreach (string row in rows)
        {
            string[] parts = row.Split(',');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int id) || string.IsNullOrWhiteSpace(parts[1]))
            {
                yield return $"Rejected: {row}";
                continue;
            }

            yield return $"Accepted: {id} -> {parts[1].Trim()}";
        }
    }
}
```
