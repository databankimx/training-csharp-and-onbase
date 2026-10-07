---
title: "Static String Methods"
chapter: 4
index: 23
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine(string.Compare("A", "A"));
        Console.WriteLine(string.Compare("A", "B"));
        Console.WriteLine(string.Compare("A", "a", StringComparison.CurrentCultureIgnoreCase));

        string[] words = ["Development ", "is ", "fun!"];
        Console.WriteLine(string.Concat(words));

        string original = "12345";
        string copied = string.Copy(original);
        Console.WriteLine(original == copied);

        string nullString = null;
        Console.WriteLine(string.IsNullOrEmpty(nullString));
        Console.WriteLine(string.IsNullOrWhiteSpace("   "));
    }
}
```
