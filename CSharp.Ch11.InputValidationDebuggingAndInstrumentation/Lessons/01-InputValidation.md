---
title: "Input Validation - TryParse, IsNullOrWhiteSpace, Regex, Sanity Checks"
chapter: 11
index: 1
dependencies: []
---

```csharp
using System;
using System.Globalization;
using System.Text.RegularExpressions;

internal static class Program
{
    private static void Main()
    {
        // --- TryParse: safe parsing without exceptions ---
        // Returns bool; parsed value arrives via out parameter.
        // This is the standard pattern -- never use Parse() on untrusted input.
        string[] candidates = ["42", "not a number", "3.14", ""];

        Console.WriteLine("int.TryParse():");
        foreach (string candidate in candidates)
        {
            bool isValid = int.TryParse(candidate, out int result);
            Console.WriteLine($"  \"{candidate}\" -> valid: {isValid}, value: {(isValid ? result.ToString() : "n/a")}");
        }

        bool priceIsValid = decimal.TryParse("19.99", out decimal price);
        Console.WriteLine($"\ndecimal.TryParse(\"19.99\"): valid: {priceIsValid}, value: {price:C}");

        // CultureInfo.InvariantCulture: date formats vary by culture.
        // Relying on the system's current culture produces surprising failures
        // in international deployments.
        bool dateIsValid = DateTime.TryParse("2026-08-25", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime date);
        Console.WriteLine($"DateTime.TryParse(\"2026-08-25\"): valid: {dateIsValid}, " +
                          $"value: {(dateIsValid ? date.ToShortDateString() : "n/a")}");

        // --- IsNullOrEmpty vs IsNullOrWhiteSpace ---
        // "   " (whitespace only): IsNullOrEmpty says fine -- it's not literally empty.
        // IsNullOrWhiteSpace correctly flags it. Which is right depends on the field.
        Console.WriteLine("\nIsNullOrEmpty() vs IsNullOrWhiteSpace():");
        string[] blanks = ["Hello", "", "   ", null];
        foreach (string s in blanks)
        {
            string display = s == null ? "(null)" : $"\"{s}\"";
            Console.WriteLine($"  {display,-12}  IsNullOrEmpty: {string.IsNullOrEmpty(s),-6}  " +
                              $"IsNullOrWhiteSpace: {string.IsNullOrWhiteSpace(s)}");
        }

        // --- Regex: validates SHAPE, not just parseability ---
        // "Mary123" is a valid string -- it just isn't a valid name.
        // The timeout is not paranoia: certain patterns on certain inputs cause
        // catastrophic backtracking (exponential time). Always pass one.
        const string namePattern = @"^([A-Z][a-z]*[-' ]?)+$";
        string[] names = ["Mary", "Mary-Jane", "O'Brien", "Van Der Berg", "mary", "Mary123", ""];

        Console.WriteLine($"\nRegex name pattern: {namePattern}");
        foreach (string name in names)
        {
            bool isMatch = Regex.IsMatch(name, namePattern, RegexOptions.Compiled, TimeSpan.FromSeconds(10));
            Console.WriteLine($"  \"{name}\" -> {isMatch}");
        }

        // --- Sanity checks: well-formed AND reasonable are two different questions ---
        // 150 parses fine as an int. It's still worth flagging as a suspicious age.
        // Syntax failure -> block outright.
        // Sanity failure -> confirmation prompt, not an outright block.
        int[] ages = [25, -5, 150, 0];
        Console.WriteLine("\nSanity-checking ages:");
        foreach (int age in ages)
        {
            bool ok = age is >= 0 and <= 120;
            Console.WriteLine($"  {age,4}: {(ok ? "reasonable" : "UNUSUAL -- worth confirming")}");
        }
    }
}
```
