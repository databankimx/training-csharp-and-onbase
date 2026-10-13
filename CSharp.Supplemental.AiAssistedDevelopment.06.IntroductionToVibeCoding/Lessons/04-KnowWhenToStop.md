---
title: "Know When to Stop"
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
        var safePrototype = new TaskRisk(false, false, true, 1);
        var riskySecurityTask = new TaskRisk(true, false, true, 1);
        var flailingTask = new TaskRisk(false, false, true, 4);
        var unknownDomain = new TaskRisk(false, false, false, 1);

        PrintDecision("Safe prototype", safePrototype);
        PrintDecision("Security task", riskySecurityTask);
        PrintDecision("Flailing task", flailingTask);
        PrintDecision("Unknown domain", unknownDomain);
    }

    private static bool ShouldStop(TaskRisk risk)
    {
        return risk.SecuritySensitive
            || risk.ComplianceSensitive
            || !risk.CanValidateOutput
            || risk.IterationCount > 3;
    }

    private static void PrintDecision(string name, TaskRisk risk)
    {
        Console.WriteLine($"{name}: {(ShouldStop(risk) ? "stop and take over" : "continue with review")}");
    }

    private sealed class TaskRisk
    {
        public TaskRisk(bool securitySensitive, bool complianceSensitive, bool canValidateOutput, int iterationCount)
        {
            SecuritySensitive = securitySensitive;
            ComplianceSensitive = complianceSensitive;
            CanValidateOutput = canValidateOutput;
            IterationCount = iterationCount;
        }

        public bool SecuritySensitive { get; }
        public bool ComplianceSensitive { get; }
        public bool CanValidateOutput { get; }
        public int IterationCount { get; }
    }
}
```
