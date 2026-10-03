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

// Top-level statements, deliberately - matches the style of the original source material this
// was adapted from, and this solution's other console lessons already show the more traditional
// internal static class Program style plenty of times over.

#region Using Directives
using System;
using System.Diagnostics;
using System.Text;
#endregion

#region Main
Uri url = new("https://learn.microsoft.com/");
string haystack = "The Quick Brown Fox";
string fileName = "REPORT.PDF";

DemonstrateEquals(url);
Console.WriteLine();
DemonstrateCompare();
Console.WriteLine();
DemonstrateIndexOf(haystack);
Console.WriteLine();
DemonstrateStartsWith(url);
Console.WriteLine();
DemonstrateEndsWith(fileName);

Console.WriteLine();
Console.WriteLine("Press any key to continue to the StringBuilder demo...");
Console.ReadKey();
Console.WriteLine();

DemonstrateStringBuilder();

// string.Equals - the recommended way to test whether two strings are equal
static void DemonstrateEquals(Uri url)
{
    Console.WriteLine("--- string.Equals ---");
    bool withoutComparison = string.Equals(url.Scheme, "HTTPS");
    bool withComparison = string.Equals(url.Scheme, "HTTPS", StringComparison.OrdinalIgnoreCase);
    Console.WriteLine($"Without StringComparison: {withoutComparison}");
    Console.WriteLine($"With StringComparison.OrdinalIgnoreCase: {withComparison}");
}

// string.Compare - for sorting, not for equality checks (see the note below)
static void DemonstrateCompare()
{
    Console.WriteLine("--- string.Compare ---");
    const string a = "apple";
    const string b = "Apple";
    int withoutComparison = string.Compare(a, b);
    int withComparison = string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    Console.WriteLine($"Without StringComparison: {withoutComparison} (non-zero - case matters by default)");
    Console.WriteLine($"With StringComparison.OrdinalIgnoreCase: {withComparison} (zero - case ignored)");
    Console.WriteLine("Note: Compare/CompareTo are for sorting, not for equality checks - don't test");
    Console.WriteLine("for a zero return value to determine whether two strings are equal, use Equals.");
}

// string.IndexOf
static void DemonstrateIndexOf(string haystack)
{
    Console.WriteLine("--- string.IndexOf ---");
    int withoutComparison = haystack.IndexOf("quick");
    int withComparison = haystack.IndexOf("quick", StringComparison.OrdinalIgnoreCase);
    Console.WriteLine($"Without StringComparison: {withoutComparison} (not found)");
    Console.WriteLine($"With StringComparison.OrdinalIgnoreCase: {withComparison} (found)");
}

// string.StartsWith
static void DemonstrateStartsWith(Uri url)
{
    Console.WriteLine("--- string.StartsWith ---");
    bool withoutComparison = url.Scheme.StartsWith("HTTP");
    bool withComparison = url.Scheme.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase);
    Console.WriteLine($"Without StringComparison: {withoutComparison}");
    Console.WriteLine($"With StringComparison.OrdinalIgnoreCase: {withComparison}");
}

// string.EndsWith
static void DemonstrateEndsWith(string fileName)
{
    Console.WriteLine("--- string.EndsWith ---");
    bool withoutComparison = fileName.EndsWith(".pdf");
    bool withComparison = fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    Console.WriteLine($"Without StringComparison: {withoutComparison}");
    Console.WriteLine($"With StringComparison.OrdinalIgnoreCase: {withComparison}");
}

// StringBuilder - when you're building a string across many operations, especially in a loop,
// concatenation with += is O(n²) in total allocations: every += discards the old string and
// allocates a brand-new one with the combined contents. StringBuilder maintains a mutable
// internal buffer and avoids that - appending is O(1) amortized, and ToString() is called
// once at the very end to produce the final immutable string.
static void DemonstrateStringBuilder()
{
    Console.WriteLine("--- StringBuilder ---");
    const int iterations = 100_000;

    // String concatenation: each += allocates a new string, copies everything
    // already accumulated into it, then throws the old one away. The total bytes
    // allocated across all the discarded strings grows quadratically.
    var sw = Stopwatch.StartNew();
    string concatenated = "";
    for (int i = 0; i < iterations; i++)
        concatenated += "x";
    sw.Stop();
    Console.WriteLine($"String concatenation ({iterations:N0} iterations): {sw.ElapsedMilliseconds} ms");
    Console.WriteLine($"Result length: {concatenated.Length:N0} characters");

    Console.WriteLine();

    // StringBuilder: writes into an existing buffer. The buffer grows (doubles) only
    // when it fills up. Pre-sizing with the expected final length eliminates even those
    // internal reallocations. ToString() is called once at the end.
    sw.Restart();
    var sb = new StringBuilder(capacity: iterations); // pre-size to avoid internal resizes
    for (int i = 0; i < iterations; i++)
        sb.Append('x'); // char overload - no per-iteration string allocation
    string built = sb.ToString();
    sw.Stop();
    Console.WriteLine($"StringBuilder ({iterations:N0} appends):    {sw.ElapsedMilliseconds} ms");
    Console.WriteLine($"Result length: {built.Length:N0} characters");

    Console.WriteLine();
    Console.WriteLine("Both results are identical. The difference is purely in how the work was done.");
    Console.WriteLine("Use + freely for small, bounded concatenations.");
    Console.WriteLine("Use StringBuilder whenever concatenation happens in a loop or depends on runtime data.");
    Console.WriteLine("Use string.Join when assembling a collection with a constant separator.");
}
#endregion

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
