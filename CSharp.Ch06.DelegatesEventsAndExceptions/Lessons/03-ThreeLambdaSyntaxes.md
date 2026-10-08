---
title: "Three Lambda Syntaxes, One Variable"
chapter: 6
index: 3
dependencies: []
---

```csharp
using System;

internal static class Program
{
    // GraphForm uses Func<float, float> directly rather than a custom delegate type.
    // This is the same concept as FunctionDelegate in step 1 -- just named using
    // the framework's built-in generic delegate instead of declaring our own.
    private static void Main()
    {
        Func<float, float> theFunction;

        // Case 0: expression lambda -- one expression, implicit return, no braces
        theFunction = x => (float)(12 * Math.Sin(3 * x) / (1 + Math.Abs(x)));
        PrintSamples("Expression lambda", theFunction);

        // Case 1: anonymous method -- C# 2.0 syntax, types written explicitly, legacy
        theFunction = delegate (float x)
        {
            x = Math.Abs(x);
            if (x < 0.001f) return 20f;
            return (float)Math.Abs(20 * Math.Cos(x) / (x + 1));
        };
        PrintSamples("Anonymous method ", theFunction);

        // Case 2: statement lambda -- braces, explicit return, multiple statements allowed
        theFunction = x =>
        {
            const float a = -0.0003f;
            const float b = -0.0024f;
            const float c =  0.02f;
            const float d =  0.09f;
            const float e = -0.5f;
            const float f =  0.3f;
            const float g =  3f;
            return (((((a * x + b) * x + c) * x + d) * x + e) * x + f) * x + g;
        };
        PrintSamples("Statement lambda  ", theFunction);
    }

    private static void PrintSamples(string label, Func<float, float> fn)
    {
        Console.WriteLine($"{label}: f(-5)={fn(-5):F3}  f(0)={fn(0):F3}  f(5)={fn(5):F3}");
    }
}
```
