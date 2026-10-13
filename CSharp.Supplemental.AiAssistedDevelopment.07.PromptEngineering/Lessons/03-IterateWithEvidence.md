---
title: "Iterate with Evidence"
chapter: 0
index: 3
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Verify("First draft", CountCsvFieldsFirstDraft);
        Console.WriteLine();
        Verify("Revised draft", CountNonEmptyCsvFieldsRevised);
    }

    private static int CountCsvFieldsFirstDraft(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
            return 0;

        return row.Split(',').Length;
    }

    private static int CountNonEmptyCsvFieldsRevised(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
            return 0;

        int count = 0;
        foreach (string field in row.Split(','))
            if (!string.IsNullOrWhiteSpace(field))
                count++;

        return count;
    }

    private static void Verify(string label, Func<string, int> counter)
    {
        Console.WriteLine(label);
        Check(counter, "alpha,beta,gamma", 3);
        Check(counter, "alpha,,gamma", 2);
        Check(counter, "", 0);
        Check(counter, null, 0);
    }

    private static void Check(Func<string, int> counter, string row, int expected)
    {
        int actual = counter(row);
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
