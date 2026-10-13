---
title: "Validate Before Accepting"
chapter: 0
index: 4
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Verify("alpha,beta,gamma", 3);
        Verify("alpha,,gamma", 2);
        Verify("", 0);
        Verify(null, 0);
    }

    private static int CountNonEmptyCsvFields(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
            return 0;

        int count = 0;
        foreach (string field in row.Split(','))
            if (!string.IsNullOrWhiteSpace(field))
                count++;

        return count;
    }

    private static void Verify(string row, int expected)
    {
        int actual = CountNonEmptyCsvFields(row);
        Console.WriteLine(actual == expected
            ? $"PASS: {Display(row)} -> {actual}"
            : $"FAIL: {Display(row)} expected {expected}, got {actual}");
    }

    private static string Display(string value)
    {
        return value == null ? "<null>" : $"\"{value}\"";
    }
}
```
