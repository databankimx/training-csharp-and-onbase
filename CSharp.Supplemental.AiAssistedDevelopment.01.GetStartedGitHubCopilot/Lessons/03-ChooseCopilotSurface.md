---
title: "Choose the Smallest Useful Surface"
chapter: 0
index: 3
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        var scenarios = new[]
        {
            new WorkScenario("Finish a simple guard clause", true, false, false, false),
            new WorkScenario("Explain an unfamiliar method", false, false, true, false),
            new WorkScenario("Add validation across three files", false, true, false, false),
            new WorkScenario("Implement a scoped issue after plan review", false, true, false, true),
        };

        foreach (var scenario in scenarios)
            Console.WriteLine($"{scenario.Name}: {ChooseSurface(scenario)}");
    }

    private static string ChooseSurface(WorkScenario scenario)
    {
        if (scenario.IsLocalAndObvious)
            return "Ghost suggestion or Inline Chat";

        if (scenario.NeedsExplanationOnly)
            return "Ask";

        if (scenario.TouchesMultipleFiles && !scenario.PlanAlreadyReviewed)
            return "Plan";

        if (scenario.TouchesMultipleFiles && scenario.PlanAlreadyReviewed)
            return "Agent";

        return "Quick Chat";
    }

    private sealed class WorkScenario
    {
        public WorkScenario(
            string name,
            bool isLocalAndObvious,
            bool touchesMultipleFiles,
            bool needsExplanationOnly,
            bool planAlreadyReviewed)
        {
            Name = name;
            IsLocalAndObvious = isLocalAndObvious;
            TouchesMultipleFiles = touchesMultipleFiles;
            NeedsExplanationOnly = needsExplanationOnly;
            PlanAlreadyReviewed = planAlreadyReviewed;
        }

        public string Name { get; }
        public bool IsLocalAndObvious { get; }
        public bool TouchesMultipleFiles { get; }
        public bool NeedsExplanationOnly { get; }
        public bool PlanAlreadyReviewed { get; }
    }
}
```
