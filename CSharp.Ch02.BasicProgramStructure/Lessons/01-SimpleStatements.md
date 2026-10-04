---
title: "Simple Statements"
chapter: 2
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int counter;
        float distance;
        string firstName;

        counter = 0;
        distance = 4.5f;
        firstName = "Bill";

        const string instructorName = "Scotty Mac";

        Console.WriteLine($"counter = {counter}");
        Console.WriteLine($"distance = {distance}");
        Console.WriteLine($"firstName = {firstName}");
        Console.WriteLine($"instructorName = {instructorName}");
    }
}
```
