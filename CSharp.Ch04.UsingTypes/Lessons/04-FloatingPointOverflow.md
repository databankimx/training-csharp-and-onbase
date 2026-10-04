---
title: "Floating-Point Overflow"
chapter: 4
index: 4
dependencies: []
---

```csharp
using System;
using System.Globalization;

internal static class Program
{
    private static void Main()
    {
        double big = -1E40;
        float small = (float)big;

        Console.WriteLine(float.IsInfinity(small)
            ? "Whoops! Must have overflowed the type..."
            : small.ToString(CultureInfo.InvariantCulture));
    }
}
```
