---
title: "Arithmetic Exceptions - Integer vs. Float"
chapter: 6
index: 5
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // Case 1: integer overflow, unchecked (default) -- silent wrong answer
        int a = 1000000000, b = 1000000000;
        int c = a * b;
        Console.WriteLine($"Unchecked int overflow: {a} * {b} = {c}");

        // Case 2: integer overflow, checked -- throws OverflowException
        checked
        {
            try
            {
                c = a * b;
            }
            catch (OverflowException ex)
            {
                Console.WriteLine($"Checked int overflow caught: {ex.Message}");
            }
        }

        // Case 3: float overflow -- produces Infinity, never throws
        // checked/unchecked has NO effect on floating-point (IEEE 754 only)
        float fa = 1e30f, fb = 1e30f;
        Console.WriteLine($"Float overflow:     {fa} * {fb} = {fa * fb}");

        // Case 4: float 0/0 -- produces NaN, never throws
        // NaN poisons every subsequent calculation, and NaN == NaN is false
        float fd = 0f, fe = 0f;
        float ff = fd / fe;
        Console.WriteLine($"Float 0/0:          {ff}");
        Console.WriteLine($"NaN == NaN is:      {float.IsNaN(ff) && float.IsNaN(ff) && ff == ff}");
        Console.WriteLine($"Use float.IsNaN():  {float.IsNaN(ff)}");

        // Integer 0/0 would throw DivideByZeroException -- left as a comment to avoid crashing
        // int zero = 0; int bad = 1 / zero;
    }
}
```
