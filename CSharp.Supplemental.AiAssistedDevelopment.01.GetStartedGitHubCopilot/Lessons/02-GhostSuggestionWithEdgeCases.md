---
title: "Ghost Suggestion: Edge Cases"
chapter: 0
index: 2
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        var numbers = new List<int> { 1, 2, 3, 4, 5, 9, 11, 12 };
        Console.WriteLine(AddPrimeNumbersInNumericList(numbers));
    }

    private static int AddPrimeNumbersInNumericList(List<int> numbers)
    {
        int sum = 0;

        foreach (int number in numbers)
            if (IsPrime(number))
                sum += number;

        return sum;
    }

    private static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        for (int divisor = 3; divisor * divisor <= number; divisor += 2)
            if (number % divisor == 0)
                return false;

        return true;
    }
}
```
