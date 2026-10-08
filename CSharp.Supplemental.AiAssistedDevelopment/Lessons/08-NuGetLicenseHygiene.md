---
title: "NuGet License Hygiene"
chapter: 0
index: 8
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    // An AI agent will suggest whatever package solves the problem.
    // It has no awareness of your organization's license policy, preferred packages,
    // or download-count thresholds. This step covers what DataBank requires before
    // accepting a new NuGet dependency.

    private static void Main()
    {
        Console.WriteLine("=== NuGet License Hygiene ===");
        Console.WriteLine();

        Console.WriteLine("DataBank ships closed-source commercial software.");
        Console.WriteLine("The license on every dependency must be compatible with that.");
        Console.WriteLine();

        Console.WriteLine("License categories you will encounter:");
        Console.WriteLine();

        var licenses = new List<(string License, string Verdict, string Reason)>
        {
            ("MIT",               "ACCEPTABLE",     "Permissive. No restrictions on use in closed-source products."),
            ("Apache 2.0",        "ACCEPTABLE",     "Permissive. Requires attribution and license notice in distribution."),
            ("BSD 2/3-Clause",    "ACCEPTABLE",     "Permissive. Similar obligations to Apache 2.0."),
            ("MS-PL",             "ACCEPTABLE",     "Microsoft Public License. Permissive for commercial use."),
            ("GPL 2.0 / 3.0",    "NOT ACCEPTABLE", "Copyleft. Derivative works must be open-source under the same license."),
            ("LGPL",              "CASE BY CASE",   "Lesser GPL. May be acceptable if used as an unmodified library dependency - " +
                                                    "get confirmation before using."),
            ("AGPL",              "NOT ACCEPTABLE", "Affero GPL. Copyleft extends to network use. Incompatible with closed-source."),
            ("Commercial/Paid",   "CASE BY CASE",   "Only acceptable if DataBank already holds a valid license for this product."),
            ("Unknown / None",    "REJECT",         "No license means all rights reserved by default. Do not use."),
        };

        Console.WriteLine($"  {"License",-18} {"Verdict",-16} Reason");
        Console.WriteLine($"  {new string('-', 80)}");
        foreach (var (license, verdict, reason) in licenses)
            Console.WriteLine($"  {license,-18} {verdict,-16} {reason}");

        Console.WriteLine();
        Console.WriteLine("DataBank policy (REPO-4):");
        Console.WriteLine("  Third-party packages MUST allow closed-source commercial use.");
        Console.WriteLine("  Review the license before adding any package.");
        Console.WriteLine("  This rule is not checked automatically - it is a manual obligation.");
        Console.WriteLine();

        Console.WriteLine("Beyond the license - what else to check:");
        Console.WriteLine();

        var checks = new List<(string Check, string Guidance)>
        {
            ("Download count",
             "A proxy for community health and active maintenance. Prefer packages with " +
             "millions of downloads over ones with thousands. Low counts warrant extra scrutiny."),
            ("Last published date",
             "A package that has not been updated in three or more years may be unmaintained. " +
             "Check whether there is an active issue tracker and recent commit history."),
            ("Owner / publisher",
             "Prefer packages published by the library author, a known organization, or the " +
             ".NET Foundation. Anonymous or single-person publishers on a critical dependency are a risk."),
            ("Does DataBank already use it?",
             "If the package is already present in other solutions, the license and reputation " +
             "have been implicitly vetted. Prefer consistency."),
            ("Is it actually needed?",
             "Agents suggest packages for convenience. A CSV parser, a JSON helper, or a retry " +
             "policy may be a few lines of code in your context. Fewer dependencies means fewer " +
             "supply-chain risks and fewer binding redirect headaches."),
        };

        foreach (var (check, guidance) in checks)
        {
            Console.WriteLine($"  [{check}]");
            Console.WriteLine($"    {guidance}");
            Console.WriteLine();
        }

        Console.WriteLine("Where to check the license:");
        Console.WriteLine("  1. nuget.org package page (License section in the right-hand panel).");
        Console.WriteLine("  2. The package's GitHub repository - look for LICENSE or LICENSE.md.");
        Console.WriteLine("  3. SPDX identifier in the .csproj PackageLicense metadata.");
        Console.WriteLine();
        Console.WriteLine("If the license is listed as 'License expression' on nuget.org,");
        Console.WriteLine("click the expression to read the full text before deciding.");
    }
}
```
