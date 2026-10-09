---
title: "Working with Generated Code"
chapter: 0
index: 6
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    // Generated code looks authoritative. It is formatted well, it usually compiles,
    // and it frequently solves the stated problem. None of that means it is correct.
    // This step covers the most common failure modes in AI-generated C# code.

    private static void Main()
    {
        Console.WriteLine("=== Working with Generated Code ===");
        Console.WriteLine();

        Console.WriteLine("The output looks good. Should you ship it?");
        Console.WriteLine("Not yet. Work through each of these before touching the PR.");
        Console.WriteLine();

        var failureModes = new List<(string Category, string WhatHappens, string Example)>
        {
            (
                "Hallucinated APIs",
                "The agent invents plausible method names that do not exist.",
                "app.SearchDocuments(typeId)  // no such method in Hyland.Unity"
            ),
            (
                "Stale API knowledge",
                "The agent uses methods that existed in an older version and are now removed or renamed.",
                "Application.CreateOnBaseApplication(props)  // deprecated; use Application.Connect(authProps)"
            ),
            (
                "Overly broad exception handling",
                "catch (Exception ex) with no context, or exceptions swallowed silently.",
                "catch (Exception) { return null; }  // caller has no idea what went wrong"
            ),
            (
                "Blocking on async",
                ".Result or .Wait() in code that is ostensibly async.",
                "var result = SomeServiceAsync().Result;  // deadlocks on ASP.NET"
            ),
            (
                "Missing cancellation",
                "CancellationToken accepted but never passed to I/O calls.",
                "await File.ReadAllTextAsync(path);  // token parameter ignored"
            ),
            (
                "Hardcoded values",
                "Connection strings, URLs, or credentials embedded in code.",
                "var url = \"http://onbase-prod/AppServer/Service.asmx\";  // standards checker FAIL"
            ),
            (
                "Unnecessary complexity",
                "The agent produces a solution that is more complex than the problem requires.",
                "A 12-line factory with a strategy pattern for a method that needs 3 lines."
            ),
            (
                "Off-brand conventions",
                "Code that compiles but does not follow DataBank standards.",
                "No copyright header, xUnit tests, throw new Exception(...), Console.WriteLine in a service."
            ),
        };

        foreach (var (category, whatHappens, example) in failureModes)
        {
            Console.WriteLine($"  [{category}]");
            Console.WriteLine($"    {whatHappens}");
            Console.WriteLine($"    e.g. {example}");
            Console.WriteLine();
        }

        Console.WriteLine("The practical approach:");
        Console.WriteLine("  1. Read the output top to bottom before running it.");
        Console.WriteLine("  2. Check every type and method name against IntelliSense or official docs.");
        Console.WriteLine("  3. Trace every exception path - what does the caller see when it fails?");
        Console.WriteLine("  4. Search for .Result, .Wait(), GetAwaiter().GetResult().");
        Console.WriteLine("  5. Search for Console.WriteLine, throw new Exception, hardcoded strings.");
        Console.WriteLine("  6. If you find yourself unable to explain a block of code, do not keep it.");
        Console.WriteLine("     Ask the agent to explain it, or rewrite it yourself.");
    }
}
```
