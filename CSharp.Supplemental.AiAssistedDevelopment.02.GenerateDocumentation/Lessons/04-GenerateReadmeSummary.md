---
title: "Generate a README Summary"
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
        var project = new ProjectSummary(
            "Document Import Validator",
            "Validates imported document rows before they enter workflow.",
            new[] { "CSV row validation", "amount parsing", "duplicate document detection" });

        Console.WriteLine(RenderReadmeSummary(project));
    }

    private static string RenderReadmeSummary(ProjectSummary project)
    {
        var lines = new List<string>
        {
            "# " + project.Name,
            "",
            project.Purpose,
            "",
            "## Capabilities"
        };

        foreach (string capability in project.Capabilities)
            lines.Add("- " + capability);

        return string.Join(Environment.NewLine, lines);
    }

    private sealed class ProjectSummary
    {
        public ProjectSummary(string name, string purpose, IReadOnlyList<string> capabilities)
        {
            Name = name;
            Purpose = purpose;
            Capabilities = capabilities;
        }

        public string Name { get; }
        public string Purpose { get; }
        public IReadOnlyList<string> Capabilities { get; }
    }
}
```
