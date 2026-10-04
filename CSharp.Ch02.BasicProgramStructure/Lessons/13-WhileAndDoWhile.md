---
title: "While and Do While"
chapter: 2
index: 13
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // while -- checks condition before each iteration
        int num = 0;
        var r = new Random();
        while (num != 10)
        {
            num = r.Next(0, 11);
            Console.WriteLine($"num = {num}");
        }

        // do while -- checks condition after each iteration,
        // so the body always runs at least once
        do
        {
            Console.WriteLine("Note: Even though I made the condition false, this loop ran once.");
        } while (false);
    }
}
```
