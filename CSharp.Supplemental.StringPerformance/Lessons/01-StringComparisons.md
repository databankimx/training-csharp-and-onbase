---
title: "String Comparisons - Always Use StringComparison"
chapter: 0
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Uri url       = new("https://learn.microsoft.com/");
        string haystack = "The Quick Brown Fox";
        string fileName = "REPORT.PDF";

        // string.Equals - the recommended way to test equality
        Console.WriteLine("--- string.Equals ---");
        Console.WriteLine($"Without StringComparison: {string.Equals(url.Scheme, "HTTPS")}");
        Console.WriteLine($"With OrdinalIgnoreCase:   {string.Equals(url.Scheme, "HTTPS", StringComparison.OrdinalIgnoreCase)}");

        // string.Compare - for sorting, not equality
        Console.WriteLine();
        Console.WriteLine("--- string.Compare (for sorting, not equality checks) ---");
        Console.WriteLine($"Without: {string.Compare("apple", "Apple")} (non-zero - case matters by default)");
        Console.WriteLine($"With OrdinalIgnoreCase: {string.Compare("apple", "Apple", StringComparison.OrdinalIgnoreCase)} (zero - equal)");

        // IndexOf
        Console.WriteLine();
        Console.WriteLine("--- string.IndexOf ---");
        Console.WriteLine($"Without: {haystack.IndexOf("quick")} (not found)");
        Console.WriteLine($"With OrdinalIgnoreCase: {haystack.IndexOf("quick", StringComparison.OrdinalIgnoreCase)} (found)");

        // StartsWith / EndsWith
        Console.WriteLine();
        Console.WriteLine("--- StartsWith / EndsWith ---");
        Console.WriteLine($"Starts with 'HTTP' (no comparison): {url.Scheme.StartsWith("HTTP")}");
        Console.WriteLine($"Starts with 'HTTP' (OrdinalIgnoreCase): {url.Scheme.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase)}");
        Console.WriteLine($"Ends with '.pdf' (no comparison): {fileName.EndsWith(".pdf")}");
        Console.WriteLine($"Ends with '.pdf' (OrdinalIgnoreCase): {fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)}");

        Console.WriteLine();
        Console.WriteLine("Always pass a StringComparison to avoid culture-sensitive surprises.");
        Console.WriteLine("Use Equals/StartsWith/EndsWith for equality; Compare/CompareTo only for sorting.");
    }
}
```
