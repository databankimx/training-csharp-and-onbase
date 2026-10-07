---
title: "Deferred Execution - Four Concrete Consequences"
chapter: 10
index: 1
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

internal static class Program
{
    private static void Main()
    {
        // --- 1. The query sees changes made AFTER it was defined ---
        var numbers = new List<int> { 1, 2, 3, 4, 5 };
        var evens   = numbers.Where(n => n % 2 == 0); // nothing runs yet

        numbers.Add(6);
        numbers.Add(8); // added AFTER defining the query

        Console.WriteLine("Evens, enumerated AFTER adding 6 and 8 (both appear):");
        foreach (int n in evens) Console.WriteLine($"  {n}");
        // 2, 4, 6, 8 -- the query saw the list as it was at enumeration time

        // --- 2. Enumerating twice re-runs the query (not cached) ---
        var withSideEffect = numbers.Where(n =>
        {
            Console.Write($"(eval {n}) ");
            return n % 2 == 0;
        });

        Console.WriteLine("\n\nFirst enumeration:");
        foreach (int n in withSideEffect) Console.Write($"{n} ");

        Console.WriteLine("\nSecond enumeration of the same variable:");
        foreach (int n in withSideEffect) Console.Write($"{n} ");
        // The predicate ran twice for every element. There's no cache.
        // Against an expensive source (database, HTTP, heavy computation),
        // this cost doubles. Materialize with .ToList() if you need results more than once.

        // --- 3. .ToList() forces execution immediately -- snapshot, not live view ---
        numbers = new List<int> { 1, 2, 3, 4, 5 };
        var snapshot = numbers.Where(n => n % 2 == 0).ToList(); // executes NOW

        numbers.Add(6);
        numbers.Add(8);

        Console.WriteLine("\n\nSnapshot (.ToList()), after adding 6 and 8 (only 2 and 4):");
        foreach (int n in snapshot) Console.WriteLine($"  {n}");

        // --- 4. Modifying during enumeration throws ---
        numbers = new List<int> { 1, 2, 3, 4, 5 };
        var query = numbers.Where(n => n % 2 == 0);
        try
        {
            foreach (int n in query)
            {
                Console.WriteLine($"\n  {n}");
                if (n == 2) numbers.Add(100); // mutate the source mid-enumeration
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  Threw: {ex.Message}");
            Console.WriteLine("  Fix: foreach (int n in query.ToList()) -- iterate a copy.");
        }

        Console.WriteLine("\nWhen a query is cheap: keep it deferred (live view).");
        Console.WriteLine("When it's expensive or used multiple times: .ToList() once and reuse.");
    }
}
```
