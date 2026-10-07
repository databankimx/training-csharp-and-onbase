---
title: "Building Strings From Characters"
chapter: 4
index: 21
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        char[] fNameParts = ['M', 'a', 'r', 'i', 'a'];
        string fName = new string(fNameParts);

        char[] lNameParts = ['W', 'a', 'r', 'd', 'e', 'n'];
        string lName = new string(lNameParts, 0, 6);

        string padding = new string('*', 5);

        Console.WriteLine(fName);
        Console.WriteLine(lName);
        Console.WriteLine(padding);
    }
}
```
