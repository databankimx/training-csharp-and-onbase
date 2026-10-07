---
title: "Custom Conversion - ToBoolean"
chapter: 4
index: 14
dependencies: []
---

```csharp
using System;

// Inline version of GenericExtensions.ToBoolean / TryParse from CSharp.SharedLibrary.
// The real implementations live in CSharp.SharedLibrary/HelperClasses/GenericExtensions.cs
// and are available to every project via the shared library reference.
public static class StringExtensions
{
    public static bool ToBoolean(this string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        if (int.TryParse(value, out int num)) return num > 0;
        string[] trueValues = ["t", "y"];
        return Array.IndexOf(trueValues, value.Substring(0, 1).ToLower()) > -1;
    }

    public static bool TryParse(this string value, out bool result)
    {
        result = false;
        if (string.IsNullOrEmpty(value)) { result = false; return true; }
        if (int.TryParse(value, out int num)) { result = num > 0; return true; }
        string[] trueValues  = ["t", "y"];
        string[] falseValues = ["f", "n"];
        if (Array.IndexOf(trueValues,  value.Substring(0, 1).ToLower()) > -1) { result = true;  return true; }
        if (Array.IndexOf(falseValues, value.Substring(0, 1).ToLower()) > -1) { result = false; return true; }
        return false;
    }
}

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine(bool.Parse("true"));

        try
        {
            bool.Parse("yes");
        }
        catch (FormatException ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("yes".ToBoolean());
        Console.WriteLine("no".ToBoolean());
        Console.WriteLine("1".ToBoolean());
        Console.WriteLine("0".ToBoolean());
    }
}
```
