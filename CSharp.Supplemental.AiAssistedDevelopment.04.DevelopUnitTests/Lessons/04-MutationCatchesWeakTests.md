---
title: "Weak Tests Miss Mutations"
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
        AssertEqual("AP-000123", FormatKeyCorrect("ap", 123), "correct implementation");

        string mutated = FormatKeyMutated("ap", 123);
        Console.WriteLine(mutated == "AP-000123"
            ? "Mutation survived. Tests are too weak."
            : $"Mutation caught. Mutated output was '{mutated}'.");
    }

    private static string FormatKeyCorrect(string type, int id)
    {
        return $"{type.Trim().ToUpperInvariant()}-{id:D6}";
    }

    private static string FormatKeyMutated(string type, int id)
    {
        return $"{type.Trim()}-{id}";
    }

    private static void AssertEqual(string expected, string actual, string name)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
            throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
    }
}
```
