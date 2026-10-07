---
title: "The Modified Closure Gotcha"
chapter: 6
index: 3
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // Two closures sharing the SAME outer variable.
        // Neither stores the value -- both store a reference to the variable itself.
        int exp = 2;
        Func<int, int> square = x => (int)Math.Pow(x, exp);
        Console.WriteLine($"square(2) = {square(2)}");   // 4 -- correct so far

        exp = 3;
        Func<int, int> cube = x => (int)Math.Pow(x, exp);
        Console.WriteLine($"cube(2)   = {cube(2)}");     // 8 -- correct

        Console.WriteLine($"square(2) = {square(2)}");   // 8 -- WRONG. exp is now 3.

        // square was never told to remember 2. It remembered exp, and exp changed.
        // Both closures read from the same shared variable, so whichever wrote last wins.

        // Fix: give each closure its own private copy.
        int squareExp = 2;
        Func<int, int> squareFixed = x => (int)Math.Pow(x, squareExp);

        int cubeExp = 3;
        Func<int, int> cubeFixed = x => (int)Math.Pow(x, cubeExp);

        Console.WriteLine($"\nsquareFixed(2) = {squareFixed(2)}"); // 4
        Console.WriteLine($"cubeFixed(2)   = {cubeFixed(2)}");    // 8
        Console.WriteLine($"squareFixed(2) = {squareFixed(2)}");  // 4 -- stays correct
    }
}
```
