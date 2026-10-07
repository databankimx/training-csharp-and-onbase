---
title: "Combining Delegates With + and -"
chapter: 6
index: 2
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private delegate void Step(string data);

    private static void StepOne(string s) { Console.Write(s + " "); }
    private static void StepTwo(string s) { Console.WriteLine(s); }

    private static void Main()
    {
        Step one = StepOne;
        Step two = StepTwo;

        // + produces a NEW delegate; originals are unchanged
        Step combined = one + two;
        combined("Test");           // StepOne runs, then StepTwo

        // - also produces a new delegate; combined is unchanged
        Step truncated = combined - one;
        truncated("Test");          // only StepTwo

        // Nothing was mutated -- confirm originals still work independently
        Console.Write("one alone:  "); one("A");
        Console.Write("two alone:  "); two("B");
    }
}
```
