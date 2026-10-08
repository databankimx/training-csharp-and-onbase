#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

namespace CSharp.Supplemental.AiAssistedDevelopment;

#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

// This program is a companion to the AI Assisted Development supplemental chapter.
// It is not an AI integration - it is a set of concrete C# examples that illustrate
// concepts from the lesson steps: prompt construction, dependency vetting, license
// checking, and the human review obligations that apply to AI-generated code.

internal static class Program
{
    #region Main
    private static void Main()
    {
        DemonstratePromptAnatomy();
        Pause();

        DemonstrateDependencyVetting();
        Pause();

        DemonstrateCodeReviewChecklist();
    }
    #endregion

    #region Prompt Anatomy
    // A good prompt gives the agent the same context a competent colleague would need:
    // language, framework version, constraints, and what "done" looks like.
    // This method prints a before/after comparison so the difference is concrete.
    private static void DemonstratePromptAnatomy()
    {
        Console.WriteLine("=== Prompt Anatomy ===");
        Console.WriteLine();

        var weak = new Prompt(
            "Weak prompt",
            "Write a method that reads a CSV file.");

        var strong = new Prompt(
            "Strong prompt",
            """
                  Write a C# 12 static method called ParseCsvRecords that:
                  - Accepts a string filePath and a CancellationToken.
                  - Returns IAsyncEnumerable<string[]> where each element is one row split by comma.
                  - Skips the header row.
                  - Uses StreamReader with await foreach - no third-party CSV libraries.
                  - Throws ArgumentException for a null or empty filePath.
                  - Does not swallow cancellation.
                  Target: net10.0. No external NuGet packages.
                  """);

        foreach (var prompt in new[] { weak, strong })
        {
            Console.WriteLine($"--- {prompt.Description} ---");
            Console.WriteLine(prompt.Text);
            Console.WriteLine();
        }

        Console.WriteLine("The strong prompt eliminates entire categories of wrong output:");
        Console.WriteLine("  - The agent cannot pick a CSV library you haven't vetted.");
        Console.WriteLine("  - The async contract is explicit, so blocking I/O is off the table.");
        Console.WriteLine("  - Cancellation and argument validation are required, not optional.");
    }
    #endregion

    #region Dependency Vetting
    // AI agents will suggest whatever package solves the problem, with no awareness of
    // license terms, organizational policy, or project reputation.
    // This method walks through the evaluation criteria DataBank uses before accepting
    // a new NuGet dependency into a closed-source project.
    private static void DemonstrateDependencyVetting()
    {
        Console.WriteLine("=== Dependency Vetting ===");
        Console.WriteLine();

        var candidates = new List<PackageCandidate>
        {
            new("CsvHelper",        "MS-PL / Apache 2.0", 280_000_000, true,
                "Widely used, permissive dual license. Acceptable."),
            new("Sylvan.Data.Csv",  "MIT",                 12_000_000, true,
                "MIT, strong reputation in the .NET community. Acceptable."),
            new("LumenworksCsv",    "MIT",                    900_000, true,
                "Low download count - evaluate actively maintained alternatives first."),
            new("SomeCsvLib.Gpl",   "GPL-3.0",              2_000_000, false,
                "GPL forces derivative works to be open-source. NOT acceptable for closed-source."),
            new("UnknownCsvPkg",    "Unknown",                  5_000, false,
                "Unknown license and negligible downloads. Reject."),
        };

        Console.WriteLine($"{"Package",-22} {"License",-20} {"Downloads",12}  {"OK?",-5}  Note");
        Console.WriteLine(new string('-', 90));

        foreach (var pkg in candidates)
        {
            var ok = pkg.ClosedSourceOk ? "YES" : "NO";
            Console.WriteLine($"{pkg.Name,-22} {pkg.License,-20} {pkg.Downloads,12:N0}  {ok,-5}  {pkg.Note}");
        }

        Console.WriteLine();
        Console.WriteLine("DataBank rules for new NuGet dependencies:");
        Console.WriteLine("  1. License must be permissive (MIT, Apache 2.0, MS-PL, BSD, etc.)");
        Console.WriteLine("     or a paid license DataBank already owns.");
        Console.WriteLine("  2. Copyleft licenses (GPL, LGPL, AGPL) are NOT acceptable for");
        Console.WriteLine("     closed-source commercial work.");
        Console.WriteLine("  3. Unknown or absent license: reject until clarified.");
        Console.WriteLine("  4. Download count is a proxy for community health.");
        Console.WriteLine("     Low counts warrant extra scrutiny - prefer established packages.");
        Console.WriteLine("  5. Packages suggested by an AI agent get the same review as any other.");
        Console.WriteLine("     The agent has no knowledge of your organization's policy.");
    }
    #endregion

    #region Code Review Checklist
    // You own every line you deliver, regardless of who or what wrote it.
    // This checklist applies to AI-generated code specifically - the items below
    // are the failure modes that appear most often in agent output.
    private static void DemonstrateCodeReviewChecklist()
    {
        Console.WriteLine("=== AI-Generated Code Review Checklist ===");
        Console.WriteLine();

        var items = new List<ChecklistItem>
        {
            new("Understanding",
                "Can you explain what every line does without looking it up?",
                "If not, do not ship it. Understand it first, or rewrite it yourself."),

            new("Hallucinated APIs",
                "Do all referenced types, methods, and packages actually exist?",
                "Agents invent plausible-sounding APIs. Verify against official docs or IntelliSense."),

            new("Error handling",
                "Are all failure paths handled? Are exceptions specific?",
                "Agents frequently use bare 'catch (Exception)' or swallow errors silently."),

            new("Async correctness",
                "No .Result, .Wait(), or .GetAwaiter().GetResult() in async code?",
                "Blocking on async work is a common agent mistake that causes deadlocks."),

            new("Cancellation",
                "Is CancellationToken threaded through all I/O and long-running calls?",
                "Agents often omit cancellation support entirely."),

            new("Security",
                "No hardcoded secrets, SQL concatenation, or path traversal risks?",
                "Agents reproduce patterns from training data, including insecure ones."),

            new("Dependencies",
                "Have all suggested NuGet packages been vetted for license and reputation?",
                "Agents have no awareness of your organization's dependency policy."),

            new("Standards",
                "Does the code meet DataBank coding standards (copyright header, exceptions, etc.)?",
                "Agent output will not include your organization's conventions by default."),

            new("Tests",
                "Are there NUnit tests covering normal, boundary, and error cases?",
                "Agent-generated tests are often incomplete. Treat them as a starting point."),
        };

        foreach (var item in items)
        {
            Console.WriteLine($"  [{item.Category}]");
            Console.WriteLine($"    {item.Item}");
            Console.WriteLine($"    -> {item.Detail}");
            Console.WriteLine();
        }

        Console.WriteLine("The checklist is a floor, not a ceiling.");
        Console.WriteLine("Code review of AI-generated output requires the same rigor as any other PR.");
    }
    #endregion

    #region Helper Types
    private sealed record Prompt(string Description, string Text);

    private sealed record PackageCandidate(
        string Name,
        string License,
        long Downloads,
        bool ClosedSourceOk,
        string Note);

    private sealed record ChecklistItem(string Category, string Item, string Detail);
    #endregion

    #region Helper Functions
    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.WriteLine();
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
