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
#endregion

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
