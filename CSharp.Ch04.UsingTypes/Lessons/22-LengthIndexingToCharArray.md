---
title: "Length, Indexing, and ToCharArray"
chapter: 4
index: 22
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        string value = "12345";
        Console.WriteLine(value.Length);
        Console.WriteLine(value[3]);

        char[] valueChars = value.ToCharArray();
        valueChars[3] = '9';
        string modified = new string(valueChars);
        Console.WriteLine(modified);
    }
}
```
