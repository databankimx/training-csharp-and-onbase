---
title: "Delegate Type and Variable"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;

// A delegate type -- any method returning float and taking a float satisfies this
internal delegate float FunctionDelegate(float x);

internal static class Program
{
    private static float DelegatedFunctionForLoad(float x) =>
        (float)(12 * Math.Sin(3 * x) / (1 + Math.Abs(x)));

    private static float DelegatedFunctionForUnload(float x) =>
        (float)(12 * Math.Sin(2 * x) / (1 + Math.Abs(x)));

    private static void Main()
    {
        // Declare a variable of the delegate type
        FunctionDelegate theFunction;

        // Assign a method WITHOUT parentheses -- stores the method itself, doesn't call it
        theFunction = DelegatedFunctionForLoad;
        Console.WriteLine($"Load result for x=1:   {theFunction(1):F4}");

        // Reassign to a completely different method -- same call site, different behavior
        theFunction = DelegatedFunctionForUnload;
        Console.WriteLine($"Unload result for x=1: {theFunction(1):F4}");
    }
}
```
