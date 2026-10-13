---
title: "Turn Vague Requests into Specifications"
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
        string vagueRequest = "Make the import faster and safer.";

        var specification = new ImplementationPrompt(
            goal: "Improve the document import pipeline.",
            clarifyingQuestions: new[]
            {
                "Which import format is being processed?",
                "What measurement shows the current import is slow?",
                "What safety risk should be addressed first?",
                "Which behavior must remain unchanged?"
            },
            constraints: new[]
            {
                "Do not change accepted row format.",
                "Do not add third-party packages.",
                "Preserve existing logging behavior.",
                "Measure before and after performance."
            },
            verificationSteps: new[]
            {
                "Run existing import tests.",
                "Add malformed-row tests.",
                "Compare before and after elapsed time for 10,000 rows."
            });

        Console.WriteLine($"Vague request: {vagueRequest}");
        Console.WriteLine();
        Console.WriteLine(RenderPrompt(specification));
    }

    private static string RenderPrompt(ImplementationPrompt prompt)
    {
        var lines = new List<string>
        {
            "Concrete implementation prompt:",
            $"Goal: {prompt.Goal}",
            "",
            "Clarifying questions:"
        };

        lines.AddRange(prompt.ClarifyingQuestions.Select(question => "- " + question));
        lines.Add("");
        lines.Add("Constraints:");
        lines.AddRange(prompt.Constraints.Select(constraint => "- " + constraint));
        lines.Add("");
        lines.Add("Verification:");
        lines.AddRange(prompt.VerificationSteps.Select(step => "- " + step));

        return string.Join(Environment.NewLine, lines);
    }

    private sealed class ImplementationPrompt
    {
        public ImplementationPrompt(
            string goal,
            IReadOnlyList<string> clarifyingQuestions,
            IReadOnlyList<string> constraints,
            IReadOnlyList<string> verificationSteps)
        {
            Goal = goal;
            ClarifyingQuestions = clarifyingQuestions;
            Constraints = constraints;
            VerificationSteps = verificationSteps;
        }

        public string Goal { get; }
        public IReadOnlyList<string> ClarifyingQuestions { get; }
        public IReadOnlyList<string> Constraints { get; }
        public IReadOnlyList<string> VerificationSteps { get; }
    }
}
```
