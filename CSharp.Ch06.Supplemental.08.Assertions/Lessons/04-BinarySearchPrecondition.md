---
title: "A Realistic Use Case - Binary Search Precondition"
chapter: 6
index: 4
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    // IsSorted costs O(n) -- too expensive to enforce with an exception on every call.
    // An assertion resolves the conflict: catches violations during development for free,
    // vanishes entirely in Release so binary search runs at full speed.
    private static bool IsSorted(int[] array)
    {
        for (int i = 1; i < array.Length; i++)
            if (array[i] < array[i - 1]) return false;
        return true;
    }

    private static int BinarySearch(int[] sortedArray, int target)
    {
        Debug.Assert(sortedArray != null,    "sortedArray must not be null.");
        Debug.Assert(IsSorted(sortedArray),  "BinarySearch requires a sorted array.");

        int low = 0, high = sortedArray.Length - 1;
        while (low <= high)
        {
            // low + (high - low) / 2 avoids integer overflow vs. (low + high) / 2
            int mid = low + (high - low) / 2;
            if (sortedArray[mid] == target) return mid;
            if (sortedArray[mid] <  target) low  = mid + 1;
            else                            high = mid - 1;
        }
        return -1;
    }

    private static void Main()
    {
        int[] sorted   = [1, 3, 5, 7, 9, 11, 13];
        int[] unsorted = [5, 1, 9, 3, 7];

        Console.WriteLine($"BinarySearch(sorted,   7) = {BinarySearch(sorted,   7)}");  //  3
        Console.WriteLine($"BinarySearch(sorted,   4) = {BinarySearch(sorted,   4)}");  // -1
        Console.WriteLine($"BinarySearch(sorted,  13) = {BinarySearch(sorted,  13)}");  //  6

        // Precondition violated -- assertion fires in Debug, vanishes in Release
        // (binary search on an unsorted array produces wrong answers silently in Release,
        //  which is exactly why the assertion exists to catch it during development)
        Console.WriteLine($"BinarySearch(unsorted, 5) = {BinarySearch(unsorted, 5)}");

        // The assertion is also documentation the compiler participates in.
        // It states the contract more precisely than a comment,
        // and unlike a comment it complains when someone violates it.
    }
}
```
