---
title: "Build a Complete Prompt"
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
        var weak = new PromptSpec(
            goal: "Write a method that parses CSV rows.",
            context: "",
            inputs: "",
            outputs: "",
            constraints: Array.Empty<string>(),
            examples: Array.Empty<string>(),
            verification: "");

        var complete = new PromptSpec(
            goal: "Write a C# method named ParseInvoiceRows.",
            context: "C# targeting net10.0.",
            inputs: "IReadOnlyList<string> rows in the format InvoiceId,CustomerName,Amount.",
            outputs: "IReadOnlyList<InvoiceRow>.",
            constraints: new[]
            {
                "Throw ArgumentNullException when rows is null.",
                "Skip blank rows.",
                "Skip rows that do not have exactly three fields.",
                "Parse Amount with CultureInfo.InvariantCulture.",
                "Do not add third-party packages."
            },
            examples: new[]
            {
                "\"1001,Avery,42.50\" becomes InvoiceRow(1001, \"Avery\", 42.50m).",
                "\"bad row\" is skipped."
            },
            verification: "Include a Main method with valid and invalid sample rows.");

        PrintScore("Weak prompt", weak);
        Console.WriteLine();
        PrintScore("Complete prompt", complete);
    }

    private static void PrintScore(string label, PromptSpec prompt)
    {
        var checks = new Dictionary<string, bool>
        {
            ["Goal"] = HasText(prompt.Goal),
            ["Context"] = HasText(prompt.Context),
            ["Inputs"] = HasText(prompt.Inputs),
            ["Outputs"] = HasText(prompt.Outputs),
            ["Constraints"] = prompt.Constraints.Any(),
            ["Examples"] = prompt.Examples.Any(),
            ["Verification"] = HasText(prompt.Verification)
        };

        Console.WriteLine(label);
        foreach (var check in checks)
            Console.WriteLine($"{check.Key}: {(check.Value ? "present" : "missing")}");

        Console.WriteLine($"Score: {checks.Count(check => check.Value)}/{checks.Count}");
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private sealed class PromptSpec
    {
        public PromptSpec(
            string goal,
            string context,
            string inputs,
            string outputs,
            IReadOnlyList<string> constraints,
            IReadOnlyList<string> examples,
            string verification)
        {
            Goal = goal;
            Context = context;
            Inputs = inputs;
            Outputs = outputs;
            Constraints = constraints;
            Examples = examples;
            Verification = verification;
        }

        public string Goal { get; }
        public string Context { get; }
        public string Inputs { get; }
        public string Outputs { get; }
        public IReadOnlyList<string> Constraints { get; }
        public IReadOnlyList<string> Examples { get; }
        public string Verification { get; }
    }
}
```
