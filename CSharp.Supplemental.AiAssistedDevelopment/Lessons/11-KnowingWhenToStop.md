---
title: "Knowing When to Stop Vibing"
chapter: 0
index: 11
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    // AI assistance is a tool. Like any tool, it is well-suited to some jobs
    // and poorly suited to others. This step covers the categories of work where
    // the risk of undetected AI errors is high enough that you should write the
    // code yourself.

    private static void Main()
    {
        Console.WriteLine("=== Knowing When to Stop Vibing ===");
        Console.WriteLine();

        Console.WriteLine("AI-assisted coding is most valuable when:");
        Console.WriteLine("  - You can validate the output quickly.");
        Console.WriteLine("  - The domain is well-represented in the agent's training data.");
        Console.WriteLine("  - Errors are easy to detect and cheap to fix.");
        Console.WriteLine("  - The task is boilerplate, scaffolding, or a well-understood pattern.");
        Console.WriteLine();

        Console.WriteLine("It is least appropriate when the opposite is true:");
        Console.WriteLine();

        var highRiskCategories = new List<(string Category, string Why, string BetterApproach)>
        {
            (
                "Production security code",
                "Authentication, authorization, token validation, and cryptography errors are " +
                "often subtle, compile cleanly, and cause serious security incidents. " +
                "The agent has no understanding of your threat model.",
                "Write it yourself. Have it reviewed by someone with security expertise. " +
                "Do not use AI output as the starting point for auth or crypto logic."
            ),
            (
                "Compliance-sensitive logic",
                "HIPAA, PCI-DSS, SOC 2, and similar requirements have specific rules that " +
                "must be met exactly. An agent will produce code that looks compliant without " +
                "knowing the actual requirement.",
                "Implement against the written requirement. Have the implementation " +
                "reviewed against the requirement explicitly, not just for general correctness."
            ),
            (
                "Unfamiliar domains you cannot validate",
                "If you cannot tell whether the agent's output is correct, you cannot review it. " +
                "The code may look authoritative and be subtly wrong in ways that only surface in " +
                "edge cases.",
                "Learn enough about the domain to validate the output before using AI assistance. " +
                "Or write it yourself as part of the learning process."
            ),
            (
                "Code the agent is visibly struggling with",
                "If three rounds of iteration have not converged on a correct solution, " +
                "the agent may be at the edge of its capability for this problem.",
                "Stop iterating. Write it yourself, or decompose the problem differently " +
                "and try again with a smaller, more specific prompt."
            ),
            (
                "Performance-critical paths",
                "Agents optimize for correctness and clarity, not for performance. " +
                "AI-generated code in a hot path may be functionally correct but allocate " +
                "excessively, enumerate eagerly, or miss obvious caching opportunities.",
                "Profile first. Identify the bottleneck. Write or rewrite the hot path yourself " +
                "with performance as an explicit goal."
            ),
        };

        foreach (var (category, why, betterApproach) in highRiskCategories)
        {
            Console.WriteLine($"  [{category}]");
            Console.WriteLine($"    Why it is high risk:");
            Console.WriteLine($"      {why}");
            Console.WriteLine($"    Better approach:");
            Console.WriteLine($"      {betterApproach}");
            Console.WriteLine();
        }

        Console.WriteLine("Signs you should stop and write it yourself:");
        Console.WriteLine("  - You have iterated more than three times without convergence.");
        Console.WriteLine("  - You are editing the output more than you would edit a blank file.");
        Console.WriteLine("  - You cannot explain what a section does without re-reading it each time.");
        Console.WriteLine("  - The agent is introducing complexity the problem does not require.");
        Console.WriteLine("  - You are accepting output you do not fully understand because the");
        Console.WriteLine("    deadline is close. (This is the most dangerous one.)");
        Console.WriteLine();

        Console.WriteLine("The handoff point:");
        Console.WriteLine("  Use AI assistance to get to a working prototype quickly.");
        Console.WriteLine("  Use your own judgment to decide which parts of that prototype");
        Console.WriteLine("  are good enough to ship and which need to be rewritten properly.");
        Console.WriteLine("  The agent accelerates the path to the prototype.");
        Console.WriteLine("  You are responsible for the distance from prototype to production.");
    }
}
```
