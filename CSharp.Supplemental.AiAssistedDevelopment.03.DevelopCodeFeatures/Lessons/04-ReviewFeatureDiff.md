---
title: "Review the Feature Output"
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
        Verify("Avery", 125m, "Avery owes $125.00");
        Verify("Blair", 0m, "Blair owes $0.00");
    }

    private static string FormatBalanceMessage(string name, decimal balance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return $"{name.Trim()} owes {balance:C}";
    }

    private static void Verify(string name, decimal balance, string expected)
    {
        string actual = FormatBalanceMessage(name, balance);
        Console.WriteLine(actual == expected
            ? $"PASS: {actual}"
            : $"FAIL: expected '{expected}', got '{actual}'");
    }
}
```
