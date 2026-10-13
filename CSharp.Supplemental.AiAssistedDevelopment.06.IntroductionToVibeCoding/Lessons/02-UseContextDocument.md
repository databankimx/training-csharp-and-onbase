---
title: "Use a Context Document"
chapter: 0
index: 2
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
        var context = new ContextRules(
            new[] { "No new NuGet packages", "Use NUnit", "Thread CancellationToken through async work" });

        var generatedPlan = new[]
        {
            "Create parser with no new dependencies",
            "Add NUnit tests for valid and invalid rows",
            "Accept CancellationToken in async import method"
        };

        foreach (string rule in context.RequiredRules)
            Console.WriteLine(PlanMentionsRule(generatedPlan, rule)
                ? $"Covered: {rule}"
                : $"Missing: {rule}");
    }

    private static bool PlanMentionsRule(IEnumerable<string> plan, string rule)
    {
        string[] words = rule.Split(' ');
        return plan.Any(step => words.Any(word => step.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private sealed class ContextRules
    {
        public ContextRules(IReadOnlyList<string> requiredRules)
        {
            RequiredRules = requiredRules;
        }

        public IReadOnlyList<string> RequiredRules { get; }
    }
}
```
