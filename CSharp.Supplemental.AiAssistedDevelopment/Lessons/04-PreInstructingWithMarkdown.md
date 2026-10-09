---
title: "Pre-Instructing the Agent with Markdown"
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
        Console.WriteLine("=== Pre-Instructing the Agent with Markdown ===");
        Console.WriteLine();

        // An AI agent knows a lot about the .NET BCL and common patterns,
        // but it knows nothing about your organization's APIs, conventions,
        // or domain rules unless you tell it.
        //
        // The most effective way to front-load that knowledge is a markdown
        // context document - a file you write once that describes the domain,
        // the APIs, and the constraints. You feed it to the agent at the start
        // of every session, before any code requests.

        Console.WriteLine("Why agents need context documents:");
        Console.WriteLine("  - The agent has no knowledge of internal APIs.");
        Console.WriteLine("  - It cannot see your project's coding conventions unless you show it.");
        Console.WriteLine("  - It will invent plausible-sounding method names if it doesn't know the real ones.");
        Console.WriteLine("  - It will repeat the same mistakes across sessions without a standing brief.");
        Console.WriteLine();

        Console.WriteLine("What a good context document covers:");
        Console.WriteLine();

        var sections = new List<(string Section, string Contents)>
        {
            ("Purpose",           "What the API or domain is for, in one paragraph."),
            ("Key types",         "The main classes, interfaces, and their relationships."),
            ("Entry points",      "How to obtain the root object (factory, DI, static method)."),
            ("Common patterns",   "How authentication, error handling, and disposal work in this API."),
            ("Conventions",       "Naming rules, exception types, logging approach, cancellation policy."),
            ("What to avoid",     "Deprecated methods, known pitfalls, anti-patterns seen in practice."),
            ("Code examples",     "One or two representative working snippets - not exhaustive."),
        };

        foreach (var (section, contents) in sections)
            Console.WriteLine($"  {section,-20} {contents}");

        Console.WriteLine();
        Console.WriteLine("How to use it:");
        Console.WriteLine("  1. Paste or attach the markdown file at the start of the agent session.");
        Console.WriteLine("  2. Open with: \"The following document describes the API you will be working with.");
        Console.WriteLine("     Treat it as authoritative. Do not invent method names or types.");
        Console.WriteLine("     Ask me if something is unclear.\"");
        Console.WriteLine("  3. Then give your code request.");
        Console.WriteLine();

        Console.WriteLine("DataBank example: the Resources folder.");
        Console.WriteLine("  Two ready-to-use context documents are in the Resources/ folder:");
        Console.WriteLine("    - AgentContext-DataBankStandards.md");
        Console.WriteLine("      Covers exception types, NUnit requirement, async rules,");
        Console.WriteLine("      secret handling, SonarQube gate, and the full merge process.");
        Console.WriteLine("    - AgentContext-OnBaseUnityAPI.md");
        Console.WriteLine("      Covers the correct entry point, all three auth modes,");
        Console.WriteLine("      session lifecycle, key types, common patterns, and pitfalls.");
        Console.WriteLine("  See lesson step 5 for a concrete before/after example");
        Console.WriteLine("  showing what these documents change in agent output.");
        Console.WriteLine();

        Console.WriteLine("Keeping context documents current:");
        Console.WriteLine("  Treat them as living documentation. When you find a new pitfall or");
        Console.WriteLine("  the API changes, update the file. An outdated context document is");
        Console.WriteLine("  worse than none - it actively misleads the agent.");
    }
}
```
