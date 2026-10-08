---
title: "Code Review and the Understanding Requirement"
chapter: 0
index: 7
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    // "The AI wrote it" is not a defense in a production incident.
    // This step covers what code review of AI-generated output looks like in practice,
    // and what DataBank's standards require regardless of how the code was produced.

    private static void Main()
    {
        Console.WriteLine("=== Code Review and the Understanding Requirement ===");
        Console.WriteLine();

        Console.WriteLine("The rule:");
        Console.WriteLine("  You own every line you deliver. If your name is on the PR,");
        Console.WriteLine("  you are responsible for understanding it, for its correctness,");
        Console.WriteLine("  and for its compliance with DataBank standards.");
        Console.WriteLine();
        Console.WriteLine("  This applies identically whether you wrote the code, a colleague");
        Console.WriteLine("  wrote it, or an AI agent wrote it.");
        Console.WriteLine();

        Console.WriteLine("What the review must cover:");
        Console.WriteLine();

        var checklist = new List<(string Area, string Check, string DataBankRule)>
        {
            (
                "Understanding",
                "Can you explain every line without looking it up?",
                "If not, do not ship it. Understand it first, or rewrite the part you cannot explain."
            ),
            (
                "Hallucinated APIs",
                "Do all types, methods, and packages actually exist in the version you are targeting?",
                "Verify against IntelliSense, official Hyland docs, or nuget.org. Do not trust the agent."
            ),
            (
                "Exception handling",
                "Are exceptions specific? Is the original exception preserved as innerException?",
                "CS-3: throw new Exception(...) and throw new ApplicationException(...) fail the standards checker. " +
                "Use approved DataBank exception types (Databank.Exceptions or Databank.NetCore.Exceptions)."
            ),
            (
                "Async correctness",
                "No .Result, .Wait(), or .GetAwaiter().GetResult() in async paths?",
                "CS-9: these patterns are flagged by the standards checker. Use await throughout."
            ),
            (
                "Cancellation",
                "Is CancellationToken threaded through all I/O calls and honoured?",
                "Agents frequently accept the token parameter and never pass it anywhere."
            ),
            (
                "Security",
                "No hardcoded connection strings, credentials, or secrets?",
                "CS-4: hardcoded connection strings fail the standards checker."
            ),
            (
                "Logging",
                "No Console.WriteLine in service or library code?",
                "CS-8: Console.WriteLine is flagged. Use the approved DataBank logging package."
            ),
            (
                "Copyright header",
                "Does the file start with the DataBank copyright region?",
                "CS-12: the standards checker requires the copyright region in the first 30 lines."
            ),
            (
                "Tests",
                "Are there NUnit tests covering normal, boundary, and error cases?",
                "CS-1/CS-2: NUnit is required. xUnit and MSTest are forbidden."
            ),
            (
                "SonarQube",
                "Does the code introduce any new Sonar issues?",
                "The Databank Way quality gate requires zero new issues of any severity."
            ),
        };

        foreach (var (area, check, rule) in checklist)
        {
            Console.WriteLine($"  [{area}]");
            Console.WriteLine($"    Check:  {check}");
            Console.WriteLine($"    Rule:   {rule}");
            Console.WriteLine();
        }

        Console.WriteLine("On the process:");
        Console.WriteLine("  AI-generated code goes through the same PR process as any other code.");
        Console.WriteLine("  There is no 'AI exception' to the review process.");
        Console.WriteLine();
        Console.WriteLine("  Before a PR can be merged:");
        Console.WriteLine("    1. The code must be committed to GitHub Enterprise source control.");
        Console.WriteLine("    2. A pull request must be opened against the target branch.");
        Console.WriteLine("    3. The PR must be reviewed and approved by a team lead or senior developer.");
        Console.WriteLine("    4. All CI scans must pass:");
        Console.WriteLine("         - Standards policy checker (DataBank best practices)");
        Console.WriteLine("         - SonarQube (zero new issues, quality gate: Databank Way)");
        Console.WriteLine("         - Snyk (no new vulnerabilities introduced)");
        Console.WriteLine();
        Console.WriteLine("  Reviewers are not obligated to understand code the author cannot explain.");
        Console.WriteLine("  If a reviewer finds a hallucinated API or a missing cancellation token,");
        Console.WriteLine("  that is a review finding - not a quirk to wave through.");
        Console.WriteLine();
        Console.WriteLine("  The CI scans are a floor, not a substitute for human review.");
        Console.WriteLine("  Code that passes all scans can still be wrong, insecure, or unreadable.");
        Console.WriteLine("  The team lead or senior reviewer is the final gate.");
    }
}
```
