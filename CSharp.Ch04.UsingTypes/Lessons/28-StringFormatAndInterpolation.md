---
title: "string.Format and Interpolation"
chapter: 4
index: 28
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int i = 163;
        Console.WriteLine(string.Format("{0} = {1,4} or 0x{2:X}", (char)i, i, i));
        Console.WriteLine($"{(char)i} = {i,4} or 0x{i:X}");

        string text = string.Format("{1} {4} {2} {1} {3}", "who", "I", "therefore", "am", "think");
        Console.WriteLine(text);
    }
}
```
