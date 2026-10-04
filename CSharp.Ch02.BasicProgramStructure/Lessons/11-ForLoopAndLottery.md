---
title: "The for Loop and Lottery Numbers"
chapter: 2
index: 11
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // Basic for loop
        for (int i = 1; i <= 10; i++)
        {
            Console.WriteLine($"i = {i}");
        }

        // Lottery number picker
        int[] range = new int[49];
        int[] picked = new int[6];
        Random rnd = new();

        for (int i = 0; i < 49; i++)
        {
            range[i] = i + 1;
        }

        for (int select = 0; select < 6; select++)
        {
            picked[select] = range[rnd.Next(49)];
        }

        Console.WriteLine("Your lotto numbers are:");
        for (int j = 0; j < 6; j++)
        {
            Console.Write(" " + picked[j] + " ");
        }
        Console.WriteLine();
    }
}
```
