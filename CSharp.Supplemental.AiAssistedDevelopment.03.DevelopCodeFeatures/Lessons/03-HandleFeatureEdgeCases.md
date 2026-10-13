---
title: "Handle Feature Edge Cases"
chapter: 0
index: 3
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
        var values = Enumerable.Range(1, 12).ToList();

        PrintPage(values, pageNumber: 1, pageSize: 5);
        PrintPage(values, pageNumber: 3, pageSize: 5);
        PrintPage(values, pageNumber: 9, pageSize: 5);
    }

    private static IReadOnlyList<int> GetPage(IReadOnlyList<int> values, int pageNumber, int pageSize)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        return values.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
    }

    private static void PrintPage(IReadOnlyList<int> values, int pageNumber, int pageSize)
    {
        var page = GetPage(values, pageNumber, pageSize);
        Console.WriteLine($"Page {pageNumber}: {string.Join(", ", page)}");
    }
}
```
