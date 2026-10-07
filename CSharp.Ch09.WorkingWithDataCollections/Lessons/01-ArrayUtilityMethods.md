---
title: "Array Utility Methods"
chapter: 9
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int[] numbers = [5, 2, 8, 1, 9, 3];
        Console.WriteLine($"Original: {string.Join(", ", numbers)}");

        // Sort and Reverse mutate in place and return void -- they do not return a new array.
        Array.Sort(numbers);
        Console.WriteLine($"After Sort:    {string.Join(", ", numbers)}");

        Array.Reverse(numbers);
        Console.WriteLine($"After Reverse: {string.Join(", ", numbers)}");

        // BinarySearch requires the array to be sorted ASCENDING.
        // Calling it on unsorted data does not throw -- it returns a meaningless result.
        // Sort again before searching.
        Array.Sort(numbers);
        int foundIndex = Array.BinarySearch(numbers, 8);
        Console.WriteLine($"\nBinarySearch(8): index {foundIndex}");
        // If not found, the negative return value's bitwise complement (~result)
        // is the insertion point -- useful, but easy to mistake for -1 meaning "not found."

        Console.WriteLine($"IndexOf(3): index {Array.IndexOf(numbers, 3)}");

        // Array.Resize takes ref because arrays cannot actually be resized --
        // it allocates a new array, copies elements, and reassigns your variable.
        // The ref is the API being honest about what it's doing.
        var copy = new int[numbers.Length];
        Array.Copy(numbers, copy, numbers.Length);
        Console.WriteLine($"\nCopy: {string.Join(", ", copy)}");
        Array.Resize(ref copy, 3);
        Console.WriteLine($"After Resize(ref copy, 3): {string.Join(", ", copy)}");

        // Array.Clear resets elements to default (0/false/null) -- it does NOT shrink the array.
        Console.WriteLine($"\nExists(n => n > 8): {Array.Exists(numbers, n => n > 8)}");
        Console.WriteLine($"FindAll(n => n % 2 == 0): {string.Join(", ", Array.FindAll(numbers, n => n % 2 == 0))}");

        Array.Clear(numbers, 0, numbers.Length);
        Console.WriteLine($"After Clear: [{string.Join(", ", numbers)}]  <-- zeroed, not removed");
    }
}
```
