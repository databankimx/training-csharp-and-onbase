---
title: "Assertions vs. Exceptions - The Actual Rule"
chapter: 6
index: 2
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private static decimal ApplyDiscount(decimal price, decimal discountPercentage)
    {
        // EXCEPTION: validates external input that can legitimately be wrong at runtime.
        // Must survive to production -- Debug.Assert would silently vanish in Release.
        if (discountPercentage < 0 || discountPercentage > 1)
            throw new ArgumentOutOfRangeException(
                nameof(discountPercentage),
                discountPercentage,
                "Discount percentage must be between 0 and 1.");

        decimal discounted = price * (1 - discountPercentage);

        // ASSERTION: guards an internal invariant.
        // Given already-validated input, a negative result would mean a bug in THIS method.
        // Compiled out of Release -- appropriate, because this guards the developer, not the caller.
        Debug.Assert(discounted >= 0,
            $"Discounted price should never be negative given validated input. Got: {discounted}");

        return discounted;
    }

    private static void Main()
    {
        Console.WriteLine(ApplyDiscount(100m, 0.2m));   // 80 -- succeeds

        try
        {
            Console.WriteLine(ApplyDiscount(100m, 1.5m));  // throws
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Caught: {ex.Message}");
        }

        // The rule in one line:
        // Exceptions handle bad INPUT.  Assertions catch broken LOGIC.
        //
        // Three-argument ArgumentOutOfRangeException is worth memorising:
        //   new ArgumentOutOfRangeException(nameof(param), offendingValue, "message")
        // It includes the actual value in the exception message automatically.
        // nameof() means a renamed parameter updates the message without a string-hunt.
    }
}
```
