---
title: "Cloning Arrays"
chapter: 4
index: 19
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int[] array1 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        // Typed clone: independent value-type copy
        int[] array2 = (int[])array1.Clone();
        array2[0] = 99;
        Console.WriteLine($"array1[0] = {array1[0]}, array2[0] = {array2[0]}");

        // Clone returns object -- must cast before use
        object cloned = array1.Clone();
        Console.WriteLine(cloned.GetType().Name); // Int32[]

        // Casting to the wrong type compiles fine but throws at runtime
        try
        {
            string[] wrong = (string[])cloned;
        }
        catch (InvalidCastException ex)
        {
            Console.WriteLine(ex.Message);
        }

        // Correct cast -- works fine
        int[] array3 = (int[])cloned;
        Console.WriteLine(array3[9]);
    }
}
```
