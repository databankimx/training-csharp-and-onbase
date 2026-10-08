---
title: "Padding, Trimming, and Case"
chapter: 4
index: 25
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine($"[{"1".PadLeft(5, ' ')}]");
        Console.WriteLine($"[{"1000".PadLeft(5, ' ')}]");

        string padded = "          information          ";
        Console.WriteLine($"[{padded.Trim()}]");
        Console.WriteLine($"[{padded.TrimStart()}]");
        Console.WriteLine($"[{padded.TrimEnd()}]");

        Console.WriteLine("DataBank".ToUpper());
        Console.WriteLine("DataBank".ToLower());
    }
}
```
