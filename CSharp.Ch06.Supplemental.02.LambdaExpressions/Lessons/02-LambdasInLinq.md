---
title: "Lambdas in LINQ"
chapter: 6
index: 2
dependencies: []
---

```csharp
using System;
using System.Linq;

internal static class Program
{
    private static void Main()
    {
        string[] words = ["cherry", "apple", "blueberry"];

        // Method syntax -- lambda passed directly to the LINQ extension method
        int shortest = words.Min(w => w.Length);
        Console.WriteLine($"Shortest word length (method syntax): {shortest}");

        // Query syntax -- compiles to the same thing underneath
        var query = from w in words select w.Length;
        Console.WriteLine($"Shortest word length (query syntax):  {query.Min()}");
        // Note: query is not a result -- it's a deferred IEnumerable<int>.
        // Nothing executes until .Min() is called on it.

        // Where overload that supplies the index as a second parameter
        string[] digits = ["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"];
        var shortDigits = digits.Where((digit, index) => digit.Length < index);
        Console.WriteLine("Digits shorter than their index:");
        foreach (var d in shortDigits)
            Console.WriteLine($"  {d}");
    }
}
```
