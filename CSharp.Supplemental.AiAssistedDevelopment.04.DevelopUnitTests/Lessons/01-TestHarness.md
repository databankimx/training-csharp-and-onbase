---
title: "Review Test Coverage"
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
        AssertEqual(6, SumPositiveNumbers(new[] { 1, 2, 3 }), "sums positive numbers");
        AssertEqual(0, SumPositiveNumbers(new[] { -1, -2, 0 }), "ignores non-positive numbers");
        AssertEqual(5, SumPositiveNumbers(new[] { 5 }), "handles single positive number");
        AssertThrows<ArgumentNullException>(() => SumPositiveNumbers(null), "rejects null input");

        Console.WriteLine("All tests passed.");
    }

    private static int SumPositiveNumbers(int[] numbers)
    {
        if (numbers == null)
            throw new ArgumentNullException(nameof(numbers));

        int sum = 0;
        foreach (int number in numbers)
            if (number > 0)
                sum += number;

        return sum;
    }

    private static void AssertEqual<T>(T expected, T actual, string name)
    {
        if (!Equals(expected, actual))
            throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
    }

    private static void AssertThrows<TException>(Action action, string name)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"{name}: expected {typeof(TException).Name}");
    }
}
```
