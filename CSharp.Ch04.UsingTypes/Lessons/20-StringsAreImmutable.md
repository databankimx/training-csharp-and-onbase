---
title: "Strings Are Immutable"
chapter: 4
index: 20
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        string s = "hello";
        s.ToUpper();         // result discarded -- s is unchanged
        Console.WriteLine(s);

        s = s.ToUpper();     // assign the return value
        Console.WriteLine(s);
    }
}
```
