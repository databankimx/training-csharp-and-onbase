---
title: "Array - Fixed Size, Index Access"
chapter: 0
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("--- Array ---");

        int[] numbers = [10, 20, 30, 40, 50];

        Console.WriteLine($"Size is fixed at creation: {numbers.Length} elements");
        Console.WriteLine($"Access by index is instant: numbers[2] = {numbers[2]}");

        // Finding a value (rather than a known index) means checking each element in turn.
        int target = 40, foundAt = -1;
        for (int i = 0; i < numbers.Length; i++)
        {
            if (numbers[i] != target) continue;
            foundAt = i;
            break;
        }
        Console.WriteLine($"Finding value {target} took checking up to index {foundAt} - a linear search");
        Console.WriteLine();
        Console.WriteLine("Key properties:");
        Console.WriteLine("  - Access by index: O(1)");
        Console.WriteLine("  - Finding a value: O(n) in the worst case");
        Console.WriteLine("  - Fixed size - cannot grow or shrink after creation");
        Console.WriteLine("  - Elements are contiguous in memory (good cache locality)");
    }
}
```
