---
title: "Boxing and Unboxing"
chapter: 4
index: 13
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int num = 10;
        object boxedNum = num;            // boxing
        int unboxedNum = (int)boxedNum;   // unboxing
        Console.WriteLine(unboxedNum);

        object boxed = 42;
        try
        {
            long wrong = (long)boxed; // InvalidCastException
        }
        catch (InvalidCastException ex)
        {
            Console.WriteLine(ex.Message);
        }

        long right = (long)(int)boxed; // unbox to original type, then widen
        Console.WriteLine(right);
    }
}
```
