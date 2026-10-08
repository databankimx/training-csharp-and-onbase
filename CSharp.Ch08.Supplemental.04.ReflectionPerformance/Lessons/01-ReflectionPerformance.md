---
title: "Reflection Performance - The Cost of the Lookup"
chapter: 8
index: 1
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Reflection;

public class Counter
{
    public int Value { get; set; }
    public void Increment() => Value++;
}

internal static class Program
{
    private const int Iterations = 1_000_000;

    private static void Main()
    {
        var counter     = new Counter();
        var counterType = typeof(Counter);

        // --- Direct vs. reflected property set (PropertyInfo cached) ---
        Console.WriteLine($"Setting a property {Iterations:N0} times...");

        var directTimer = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++) counter.Value = i;
        directTimer.Stop();

        // Lookup happens ONCE, before the timed loop.
        PropertyInfo valueProp = counterType.GetProperty("Value");
        var reflectedTimer = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++) valueProp?.SetValue(counter, i);
        reflectedTimer.Stop();

        PrintRatio("Direct set", directTimer.Elapsed, "Reflected set (cached)", reflectedTimer.Elapsed);

        // --- Direct vs. reflected method call (MethodInfo cached) ---
        Console.WriteLine($"\nCalling a method {Iterations:N0} times...");

        directTimer.Restart();
        for (int i = 0; i < Iterations; i++) counter.Increment();
        directTimer.Stop();

        MethodInfo incMethod = counterType.GetMethod("Increment");
        reflectedTimer.Restart();
        for (int i = 0; i < Iterations; i++) incMethod?.Invoke(counter, null);
        reflectedTimer.Stop();

        PrintRatio("Direct call", directTimer.Elapsed, "Reflected call (cached)", reflectedTimer.Elapsed);

        // --- Cached vs. uncached lookup -- the real culprit ---
        // This comparison shows WHERE the cost actually lives.
        Console.WriteLine($"\nCached vs. uncached lookup, {Iterations:N0} iterations...");

        PropertyInfo cachedProp = counterType.GetProperty("Value");
        var cachedTimer = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++) cachedProp?.SetValue(counter, i);
        cachedTimer.Stop();

        var uncachedTimer = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
            counterType.GetProperty("Value")?.SetValue(counter, i); // lookup inside the loop
        uncachedTimer.Stop();

        PrintRatio("Cached PropertyInfo", cachedTimer.Elapsed, "Uncached (re-looked-up every iteration)", uncachedTimer.Elapsed);

        Console.WriteLine();
        Console.WriteLine("The expensive part is the LOOKUP, not the call.");
        Console.WriteLine("Cache the PropertyInfo / MethodInfo and reuse it -- that's it.");
        Console.WriteLine("Never put GetProperty() or GetMethod() inside a hot loop.");
    }

    private static void PrintRatio(string label1, TimeSpan t1, string label2, TimeSpan t2)
    {
        Console.WriteLine($"  {label1}: {t1.TotalMilliseconds:N1} ms");
        Console.WriteLine($"  {label2}: {t2.TotalMilliseconds:N1} ms");
        if (t1.TotalMilliseconds > 0)
            Console.WriteLine($"  {label2} took ~{t2.TotalMilliseconds / t1.TotalMilliseconds:N1}x as long.");
    }
}
```
