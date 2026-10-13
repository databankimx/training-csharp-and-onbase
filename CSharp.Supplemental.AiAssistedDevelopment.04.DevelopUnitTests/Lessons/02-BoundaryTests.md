---
title: "Boundary Tests"
chapter: 0
index: 2
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        AssertEqual(100m, ApplyDiscount(100m, 0m), "zero discount");
        AssertEqual(0m, ApplyDiscount(100m, 1m), "full discount");
        AssertThrows<ArgumentOutOfRangeException>(() => ApplyDiscount(100m, -0.01m), "negative discount");
        AssertThrows<ArgumentOutOfRangeException>(() => ApplyDiscount(100m, 1.01m), "discount above one");

        Console.WriteLine("Boundary tests passed.");
    }

    private static decimal ApplyDiscount(decimal price, decimal discountPercentage)
    {
        if (discountPercentage < 0m || discountPercentage > 1m)
            throw new ArgumentOutOfRangeException(nameof(discountPercentage));

        return price * (1m - discountPercentage);
    }

    private static void AssertEqual(decimal expected, decimal actual, string name)
    {
        if (expected != actual)
            throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
    }

    private static void AssertThrows<TException>(Action action, string name)
        where TException : Exception
    {
        try { action(); }
        catch (TException) { return; }
        throw new InvalidOperationException($"{name}: expected {typeof(TException).Name}");
    }
}
```
