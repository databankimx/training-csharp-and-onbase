---
title: "Use Examples to Shape Output"
chapter: 0
index: 4
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        var examples = new Dictionary<string, string>
        {
            ["Invoice total is wrong"] = "Billing",
            ["Cannot sign in"] = "Access",
            ["Export crashes on large file"] = "Technical",
            ["What are your support hours?"] = "General",
            ["Password reset link expired"] = "Access"
        };

        foreach (var example in examples)
        {
            string actual = ClassifyTicket(example.Key);
            Console.WriteLine(actual == example.Value
                ? $"PASS: {example.Key} -> {actual}"
                : $"FAIL: {example.Key} expected {example.Value}, got {actual}");
        }
    }

    private static string ClassifyTicket(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "General";

        string normalized = title.ToUpperInvariant();

        if (normalized.Contains("INVOICE") || normalized.Contains("BILLING") || normalized.Contains("PAYMENT"))
            return "Billing";

        if (normalized.Contains("SIGN IN") || normalized.Contains("PASSWORD") || normalized.Contains("ACCESS"))
            return "Access";

        if (normalized.Contains("CRASH") || normalized.Contains("EXPORT") || normalized.Contains("ERROR"))
            return "Technical";

        return "General";
    }
}
```
