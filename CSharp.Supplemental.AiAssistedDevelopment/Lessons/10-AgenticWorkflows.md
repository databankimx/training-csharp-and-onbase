---
title: "Agentic Coding Workflows"
chapter: 0
index: 10
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    // An agentic workflow is one where the AI takes a series of steps autonomously -
    // reading files, writing code, running tests, iterating - rather than just
    // answering a single prompt. Tools like Claude Code operate in this mode.
    //
    // This step covers how to structure a task for an agentic tool, and how to keep
    // the agent on track through a multi-step implementation.

    private static void Main()
    {
        Console.WriteLine("=== Agentic Coding Workflows ===");
        Console.WriteLine();

        Console.WriteLine("What makes a task 'agentic':");
        Console.WriteLine("  - The agent reads existing files to understand context.");
        Console.WriteLine("  - It writes or modifies multiple files.");
        Console.WriteLine("  - It may run the build or tests and iterate on failures.");
        Console.WriteLine("  - It works toward a goal across several steps without a prompt per step.");
        Console.WriteLine();

        Console.WriteLine("How to set up an agentic task for success:");
        Console.WriteLine();

        var steps = new List<(string Step, string Guidance)>
        {
            (
                "1. Write a clear brief",
                "Describe the goal, the constraints, and what 'done' looks like. " +
                "Include the target framework, namespace conventions, and any related files the agent should read first. " +
                "The brief is the spec. Vague briefs produce vague code."
            ),
            (
                "2. Attach context documents",
                "Paste or attach your markdown context files (API reference, coding conventions, " +
                "DataBank standards) before giving the task. The agent has no memory of previous sessions."
            ),
            (
                "3. Break large tasks into checkpoints",
                "For a feature that touches many files, ask for the design first. " +
                "Review it before the agent writes any code. Catching a design problem is cheaper than " +
                "catching it in 300 lines of generated implementation."
            ),
            (
                "4. Review at each checkpoint",
                "Do not let the agent run for ten steps unattended and review at the end. " +
                "Check the output at each meaningful milestone. Course-correct early."
            ),
            (
                "5. Run the standards checker and SonarQube",
                "After any code-generating session, run the standards policy checker and " +
                "review the SonarQube analysis before opening a PR. The agent does not do this for you."
            ),
            (
                "6. Write or review the tests yourself",
                "Agent-generated tests are often incomplete - they test the happy path " +
                "and skip boundary and error cases. Treat them as a starting point, not a finished suite."
            ),
        };

        foreach (var (step, guidance) in steps)
        {
            Console.WriteLine($"  {step}");
            Console.WriteLine($"    {guidance}");
            Console.WriteLine();
        }

        Console.WriteLine("Exercise: rapid prototyping a data processor");
        Console.WriteLine();
        Console.WriteLine("  Task: use an AI agent of your choice to build a small C# class that:");
        Console.WriteLine("    - Accepts a List<string> of raw CSV rows (no header).");
        Console.WriteLine("    - Each row has the format: Id,Name,Amount (Amount is decimal).");
        Console.WriteLine("    - Returns a summary: total row count, sum of Amount, and the Name");
        Console.WriteLine("      of the record with the highest Amount.");
        Console.WriteLine("    - Throws ArgumentException if the list is null or empty.");
        Console.WriteLine("    - Skips and logs (via a provided ILogger) any row that cannot be parsed.");
        Console.WriteLine("    - Target: net10.0, no third-party packages, NUnit tests included.");
        Console.WriteLine();
        Console.WriteLine("  Before prompting:");
        Console.WriteLine("    1. Write out the full prompt using the anatomy from step 3.");
        Console.WriteLine("    2. Include the DataBank standards constraints explicitly.");
        Console.WriteLine("    3. Run the output through the checklist from step 7.");
        Console.WriteLine();
        Console.WriteLine("  After you have output from the agent, ask yourself:");
        Console.WriteLine("    - Can you explain every line?");
        Console.WriteLine("    - Are the NUnit tests testing meaningful behavior?");
        Console.WriteLine("    - Does the code pass the standards checker?");
        Console.WriteLine("    - Would this pass a PR review from a colleague?");
    }
}
```
