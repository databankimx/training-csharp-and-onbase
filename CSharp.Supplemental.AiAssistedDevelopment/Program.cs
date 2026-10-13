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

        DemonstrateCopilotSurfaceSelection();
        Pause();

        DemonstrateDependencyVetting();
        Pause();

        DemonstrateDocumentationReview();
        Pause();

        DemonstrateGeneratedTestReview();
        Pause();

        DemonstrateRefactoringWorkflow();
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

    #region Copilot Surface Selection
    // Copilot is not a single workflow. The right surface depends on whether you are
    // completing a local thought, asking a question, planning a change, or delegating
    // a scoped implementation task.
    private static void DemonstrateCopilotSurfaceSelection()
    {
        Console.WriteLine("=== Choosing the Right Copilot Surface ===");
        Console.WriteLine();

        var surfaces = new List<CopilotSurface>
        {
            new("Ghost suggestions",
                "Fast inline completions while you are already typing.",
                "Read before accepting; momentum is not the same thing as correctness."),

            new("Inline Chat",
                "Small edits in the current file or selected code.",
                "Keep the request local and specific."),

            new("Ask",
                "Explaining code, comparing approaches, and answering questions.",
                "Attach the file, selection, error, or test failure it needs."),

            new("Plan",
                "Multi-file changes where the approach should be reviewed first.",
                "A plan is a proposal, not a permission slip."),

            new("Agent",
                "Scoped implementation tasks where the assistant can inspect, edit, build, and iterate.",
                "Give it constraints, checkpoints, and a small blast radius."),

            new("Code review / PR summary",
                "A second pass over changes and a readable summary for reviewers.",
                "It helps the review; it does not replace the reviewer."),
        };

        Console.WriteLine($"{"Surface",-24} {"Use when",-62} Caution");
        Console.WriteLine(new string('-', 120));

        foreach (var surface in surfaces)
            Console.WriteLine($"{surface.Name,-24} {surface.UseWhen,-62} {surface.Caution}");

        Console.WriteLine();
        Console.WriteLine("Default rule: use the smallest useful surface.");
        Console.WriteLine("If completion is enough, don't unleash an agent on the whole project.");
    }
    #endregion

    #region Dependency Vetting
    // AI agents will suggest whatever package solves the problem, with no awareness of
    // license terms, organizational policy, or project reputation.
    // This method walks through the evaluation criteria DataBank uses before accepting
    // a new NuGet dependency into a closed-source project.
    //
    // NOTE: The packages below are illustrative examples constructed to demonstrate
    // the evaluation criteria. CsvHelper and Sylvan.Data.Csv are real packages with
    // accurate approximate download counts and licenses as of 2026. LumenworksCsv,
    // SomeCsvLib.Gpl, and UnknownCsvPkg are fictional examples showing what a
    // low-count, copyleft, and unknown-license package evaluation looks like.
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

    #region Documentation Review
    // AI can draft documentation quickly, but the output still has to be checked
    // against the real code and the intended audience.
    private static void DemonstrateDocumentationReview()
    {
        Console.WriteLine("=== Reviewing AI-Generated Documentation ===");
        Console.WriteLine();

        var checks = new List<ChecklistItem>
        {
            new("Audience",
                "Is the documentation written for the right reader?",
                "A maintainer, API consumer, junior developer, and support engineer need different details."),

            new("Behavior",
                "Does it describe what the code actually does?",
                "Generated docs sometimes document what the method name implies rather than what the method implements."),

            new("Failure paths",
                "Are exceptions, null handling, and side effects documented?",
                "The happy path is rarely the part someone looks up at 2am."),

            new("Noise",
                "Did it comment obvious syntax?",
                "Delete decorative comments. They cost attention and buy nothing."),

            new("Uncertainty",
                "Did it invent intent where the code is unclear?",
                "A good assistant should ask questions instead of writing fan fiction about your architecture."),
        };

        foreach (var check in checks)
        {
            Console.WriteLine($"  [{check.Category}]");
            Console.WriteLine($"    {check.Item}");
            Console.WriteLine($"    -> {check.Detail}");
            Console.WriteLine();
        }

        Console.WriteLine("Useful prompt:");
        Console.WriteLine("  Generate XML docs for this method for a DataBank maintainer.");
        Console.WriteLine("  Include parameters, return value, exceptions, and side effects.");
        Console.WriteLine("  Do not describe obvious syntax. List questions if behavior is unclear.");
    }
    #endregion

    #region Generated Test Review
    // Generated tests often look complete because they follow the shape of a unit
    // test. The question is whether they verify the behavior that actually matters.
    private static void DemonstrateGeneratedTestReview()
    {
        Console.WriteLine("=== Reviewing AI-Generated Tests ===");
        Console.WriteLine();

        var checks = new List<string>
        {
            "Do the tests verify requirements, not merely the current implementation?",
            "Are invalid input, empty input, nulls, and boundary values covered?",
            "Are assertions meaningful, or just 'not null' with a tiny hat?",
            "Are async calls awaited without .Result, .Wait(), or swallowed cancellation?",
            "Are the tests independent of execution order and shared mutable state?",
            "Do they follow the existing NUnit style and helper patterns?",
        };

        foreach (var check in checks)
            Console.WriteLine($"  - {check}");

        Console.WriteLine();
        Console.WriteLine("Useful prompt:");
        Console.WriteLine("  Generate NUnit tests for this method.");
        Console.WriteLine("  Cover normal behavior, boundary cases, invalid input, and exceptions.");
        Console.WriteLine("  After writing tests, explain which requirement each test verifies.");
    }
    #endregion

    #region Refactoring Workflow
    // The safest AI-assisted refactor starts with observations and applies one
    // concern at a time. Broad prompts produce broad diffs, and broad diffs hide
    // accidental behavior changes.
    private static void DemonstrateRefactoringWorkflow()
    {
        Console.WriteLine("=== AI-Assisted Refactoring Workflow ===");
        Console.WriteLine();

        var workflow = new List<(string Step, string Guidance)>
        {
            ("1. Ask for observations",
                "List possible improvements by category; do not edit code yet."),
            ("2. Choose the goal",
                "Pick the improvement that matches the current work, not every improvement in sight."),
            ("3. Change one concern",
                "Readability, reliability, performance, or security - one at a time."),
            ("4. Run tests",
                "Refactoring without a safety net is just rearranging furniture in the dark."),
            ("5. Review the diff",
                "Look for behavior changes disguised as cleanup."),
        };

        foreach (var (step, guidance) in workflow)
        {
            Console.WriteLine($"  {step}");
            Console.WriteLine($"    {guidance}");
            Console.WriteLine();
        }

        Console.WriteLine("Useful prompt:");
        Console.WriteLine("  Review this class for refactoring opportunities.");
        Console.WriteLine("  Group findings by readability, reliability, performance, and security.");
        Console.WriteLine("  Do not change behavior or introduce new dependencies.");
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

    private sealed record CopilotSurface(string Name, string UseWhen, string Caution);

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
