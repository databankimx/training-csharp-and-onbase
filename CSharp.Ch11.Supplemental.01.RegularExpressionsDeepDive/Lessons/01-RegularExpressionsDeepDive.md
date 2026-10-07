---
title: "Regular Expressions - Groups, Matches, Replace, Options, Greedy vs Lazy, Performance"
chapter: 11
index: 1
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

internal static class Program
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private static void Main()
    {
        // --- Pattern breakdown ---
        // ^([A-Z][a-z]*[-' ]?)+$
        //   ^           anchor: start of string
        //   (...)+      group repeats one or more times
        //   [A-Z]       exactly one uppercase letter
        //   [a-z]*      zero or more lowercase letters
        //   [-' ]?      optional hyphen, apostrophe, or space
        //   $           anchor: end of string
        // Without ^ and $, "mary123" would match because "mary" appears inside it.
        const string namePattern = @"^([A-Z][a-z]*[-' ]?)+$";
        string[] names = ["Mary", "Mary-Jane", "O'Brien", "Van Der Berg", "mary", "Mary123", ""];
        Console.WriteLine($"Name pattern: {namePattern}");
        foreach (string name in names)
            Console.WriteLine($"  \"{name}\" -> {Regex.IsMatch(name, namePattern, RegexOptions.Compiled, Timeout)}");

        // --- Named capture groups: extracting pieces of a match ---
        // (?<n>...) defines a named group. match.Groups["n"].Value retrieves it.
        // Far more maintainable than numbered groups ($1, $2) in complex patterns.
        const string emailPattern = @"^(?<user>[^@\s]+)@(?<domain>[^@\s]+\.[^@\s]+)$";
        string[] emails = ["jane.doe@example.com", "not-an-email"];
        Console.WriteLine("\nNamed groups:");
        foreach (string email in emails)
        {
            var m = Regex.Match(email, emailPattern, RegexOptions.Compiled, Timeout);
            if (m.Success)
                Console.WriteLine($"  \"{email}\" -> user: \"{m.Groups["user"].Value}\", domain: \"{m.Groups["domain"].Value}\"");
            else
                Console.WriteLine($"  \"{email}\" -> no match");
        }

        // --- Regex.Matches(): find ALL occurrences ---
        // Regex.Match() (singular) finds the first. Regex.IsMatch() just answers yes/no.
        const string phonePattern = @"\d{3}-\d{3}-\d{4}";
        const string text = "Office: 555-123-4567. Jane direct: 555-987-6543.";
        var matches = Regex.Matches(text, phonePattern, RegexOptions.Compiled, Timeout);
        Console.WriteLine($"\nMatches() found {matches.Count} phone number(s):");
        foreach (Match m in matches)
            Console.WriteLine($"  \"{m.Value}\" at position {m.Index}");

        // --- Regex.Replace(): transform matched text using captured groups ---
        // $1/$2/$3 refer to captured groups in order. ${name} for named groups.
        // MM/DD/YYYY -> YYYY-MM-DD (ISO 8601) in one call.
        const string datePattern = @"(\d{2})/(\d{2})/(\d{4})";
        string original = "Invoice dated 08/25/2026.";
        string replaced = Regex.Replace(original, datePattern, "$3-$1-$2", RegexOptions.Compiled, Timeout);
        Console.WriteLine($"\nReplace (reorder date groups): \"{original}\" -> \"{replaced}\"");

        // --- RegexOptions ---
        Console.WriteLine("\nRegexOptions.IgnoreCase:");
        Console.WriteLine($"  Default:    {Regex.IsMatch("Hello", "hello", RegexOptions.Compiled, Timeout)}");
        Console.WriteLine($"  IgnoreCase: {Regex.IsMatch("Hello", "hello", RegexOptions.Compiled | RegexOptions.IgnoreCase, Timeout)}");

        // --- Greedy vs lazy quantifiers ---
        // ".*" grabs AS MUCH as possible while still letting the overall pattern succeed.
        // ".*?" grabs AS LITTLE as possible.
        // Same input, almost the same pattern, wildly different results.
        const string html = "<b>bold</b> and <i>italic</i>";
        var greedy = Regex.Match(html, "<.*>",  RegexOptions.Compiled, Timeout);
        var lazy   = Regex.Match(html, "<.*?>", RegexOptions.Compiled, Timeout);
        Console.WriteLine($"\nGreedy \"<.*>\":  \"{greedy.Value}\"");
        Console.WriteLine($"Lazy   \"<.*?>\": \"{lazy.Value}\"");
        Console.WriteLine("Add ? after any quantifier (*?, +?, ??) to make it lazy.");

        // --- Compiled instance vs repeated static calls ---
        // Static calls cache a limited number of patterns but do more work per call.
        // A reused compiled instance pays the pattern-parse cost once, up front.
        // RegexOptions.Compiled uses Reflection.Emit to JIT the pattern to native IL --
        // worth it for a high-frequency pattern, overkill for one-shot use.
        const string perfPattern = @"^\d{3}-\d{3}-\d{4}$";
        const string perfInput   = "555-123-4567";
        const int    iterations  = 200_000;

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            Regex.IsMatch(perfInput, perfPattern, RegexOptions.Compiled, Timeout);
        sw.Stop();
        Console.WriteLine($"\nStatic Regex.IsMatch(), {iterations:N0} calls:        {sw.ElapsedMilliseconds} ms");

        var compiled = new Regex(perfPattern, RegexOptions.Compiled, Timeout);
        sw.Restart();
        for (int i = 0; i < iterations; i++)
            compiled.IsMatch(perfInput);
        sw.Stop();
        Console.WriteLine($"Reused compiled instance, {iterations:N0} calls: {sw.ElapsedMilliseconds} ms");
    }
}
```
