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

        // Typed cast: independent value-type copy
        int[] array2 = (int[])array1.Clone();
        array2[0] = 99;
        Console.WriteLine($"array1[0] = {array1[0]}, array2[0] = {array2[0]}");

        // dynamic: skips the cast, loses compile-time checking
        dynamic array3 = array1.Clone();
        Console.WriteLine(array3[9]);

        try
        {
            array3[0] = "one"; // compiles fine, throws at runtime
        }
        catch (InvalidCastException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
```
