---
title: "Add Guardrails and Negative Constraints"
chapter: 0
index: 5
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
        var weakPrompt = new PromptPolicy(
            "Write a method that resolves an import file path.",
            Array.Empty<string>());

        var guardedPrompt = new PromptPolicy(
            "Write a method that resolves an import file path.",
            new[]
            {
                "Do not accept absolute paths from the caller.",
                "Do not allow .. path traversal.",
                "Do not read files in this method.",
                "Do not add third-party packages.",
                "Do not include secrets in code.",
                "Do not execute generated code at runtime."
            });

        PrintPolicy("Weak prompt", weakPrompt);
        Console.WriteLine();
        PrintPolicy("Guarded prompt", guardedPrompt);
    }

    private static void PrintPolicy(string label, PromptPolicy policy)
    {
        Console.WriteLine(label);
        foreach (string guardrail in RequiredGuardrails())
            Console.WriteLine($"{guardrail}: {(policy.Covers(guardrail) ? "covered" : "missing")}");
    }

    private static IEnumerable<string> RequiredGuardrails()
    {
        yield return "absolute paths";
        yield return "path traversal";
        yield return "read files";
        yield return "third-party packages";
        yield return "secrets";
        yield return "execute generated code";
    }

    private sealed class PromptPolicy
    {
        public PromptPolicy(string goal, IReadOnlyList<string> negativeConstraints)
        {
            Goal = goal;
            NegativeConstraints = negativeConstraints;
        }

        public string Goal { get; }
        public IReadOnlyList<string> NegativeConstraints { get; }

        public bool Covers(string phrase)
        {
            return NegativeConstraints.Any(rule =>
                rule.IndexOf(phrase, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
```
