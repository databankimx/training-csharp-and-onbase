---
title: "Instance String Methods"
chapter: 4
index: 24
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        string original = "one two three four five";

        Console.WriteLine(original.Contains("one"));
        Console.WriteLine(original.EndsWith("FIVE", StringComparison.CurrentCultureIgnoreCase));
        Console.WriteLine(original.IndexOf("two", StringComparison.CurrentCultureIgnoreCase));
        Console.WriteLine(original.Insert(4, "half "));
        Console.WriteLine(original.Remove(7, 6));
        Console.WriteLine(original.Replace("two", "222"));
        Console.WriteLine(original.Substring(4, 3));
        Console.WriteLine(original.StartsWith("ONE", StringComparison.CurrentCultureIgnoreCase));

        Console.WriteLine(original); // unchanged
    }
}
```
