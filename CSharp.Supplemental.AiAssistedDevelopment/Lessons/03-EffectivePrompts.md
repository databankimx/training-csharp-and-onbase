---
title: "Writing Effective Prompts"
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
        Console.WriteLine("=== Writing Effective Prompts ===");
        Console.WriteLine();

        // A prompt is a specification. The more precise the specification,
        // the less the agent has to guess - and the less wrong output you
        // have to sift through.

        Console.WriteLine("The anatomy of a strong prompt:");
        Console.WriteLine();

        var components = new List<(string Element, string Example)>
        {
            ("Language and version",   "C# 12, targeting net10.0"),
            ("Method signature",       "public static async Task<IReadOnlyList<T>> ..."),
            ("Input / output contract","Accepts X, returns Y, throws Z for invalid input"),
            ("Constraints",            "No third-party packages. Use StreamReader, not File.ReadAllText."),
            ("Cancellation",           "Accept and honour a CancellationToken throughout."),
            ("Error handling",         "Throw ArgumentException for null path. Wrap IO errors in a named exception."),
            ("Style",                  "Follow the existing file structure. Include XML doc comments."),
        };

        foreach (var (element, example) in components)
            Console.WriteLine($"  {element,-28} -> {example}");

        Console.WriteLine();
        Console.WriteLine("--- Weak prompt ---");
        Console.WriteLine("  \"Write a method that reads a CSV file.\"");
        Console.WriteLine();
        Console.WriteLine("Problems with the weak prompt:");
        Console.WriteLine("  - The agent picks the target framework (may be wrong).");
        Console.WriteLine("  - The agent picks a CSV library (may be GPL or unmaintained).");
        Console.WriteLine("  - The agent decides whether it is sync or async.");
        Console.WriteLine("  - Error handling and cancellation are omitted unless guessed.");
        Console.WriteLine();

        Console.WriteLine("--- Strong prompt ---");
        Console.WriteLine("  \"Write a C# 12 static method ParseCsvRecords that:");
        Console.WriteLine("   - Accepts string filePath and CancellationToken.");
        Console.WriteLine("   - Returns IAsyncEnumerable<string[]>, one element per data row.");
        Console.WriteLine("   - Skips the header row.");
        Console.WriteLine("   - Uses StreamReader and await foreach - no third-party CSV libraries.");
        Console.WriteLine("   - Throws ArgumentException for null or empty filePath.");
        Console.WriteLine("   - Does not swallow cancellation.\"");
        Console.WriteLine();

        Console.WriteLine("Iterating when output misses the mark:");
        Console.WriteLine("  1. Identify the specific gap (wrong return type, missing null check, etc.).");
        Console.WriteLine("  2. Add that gap as an explicit constraint in a follow-up prompt.");
        Console.WriteLine("  3. Do not accept a revision you cannot fully read and explain.");
        Console.WriteLine("  4. If iteration is not converging, write that part yourself.");
    }
}
```
