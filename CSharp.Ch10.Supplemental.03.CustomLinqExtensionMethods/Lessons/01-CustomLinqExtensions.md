---
title: "Custom LINQ Extension Methods"
chapter: 10
index: 1
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

// Extension methods must live in a static class.
// These chain with built-in LINQ operators because they share the same type:
// IEnumerable<T> in, IEnumerable<T> out (or a scalar for aggregates).
public static class LinqExtensions
{
    // WhereCustom: two-method split for eager validation.
    // The PUBLIC method validates immediately, BEFORE any iteration starts.
    // The private iterator method contains the yield return -- its body doesn't
    // execute until the first MoveNext() call, so validation must live outside it.
    public static IEnumerable<T> WhereCustom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source    == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return WhereCustomIterator(source, predicate);
    }
    private static IEnumerable<T> WhereCustomIterator<T>(IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (T item in source)
            if (predicate(item)) yield return item;
    }

    // BadWhereCustom: validation INSIDE a yield return method -- deliberately broken.
    // The entire body of a yield return method is deferred. The null check never runs
    // until enumeration starts, potentially far from where the bug was introduced.
    public static IEnumerable<T> BadWhereCustom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source    == null) throw new ArgumentNullException(nameof(source));   // deferred!
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        foreach (T item in source)
            if (predicate(item)) yield return item;
    }

    // DistinctBy: keep only the first element per distinct key.
    // DistinctBy() wasn't added to .NET LINQ until .NET 6 -- on net48 this IS the fix.
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        if (source      == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        return DistinctByIterator(source, keySelector);
    }
    private static IEnumerable<T> DistinctByIterator<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();
        foreach (T item in source)
            if (seen.Add(keySelector(item))) yield return item; // Add() returns false on duplicate
    }

    // Chunk: split a sequence into fixed-size arrays (last batch may be smaller).
    // Chunk() wasn't added to .NET LINQ until .NET 6 -- on net48 this IS the fix.
    public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> source, int size)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (size   <= 0)    throw new ArgumentOutOfRangeException(nameof(size));
        return ChunkIterator(source, size);
    }
    private static IEnumerable<T[]> ChunkIterator<T>(IEnumerable<T> source, int size)
    {
        var buf = new List<T>(size);
        foreach (T item in source)
        {
            buf.Add(item);
            if (buf.Count == size) { yield return buf.ToArray(); buf.Clear(); }
        }
        if (buf.Count > 0) yield return buf.ToArray(); // final partial batch
    }

    // Median: an IMMEDIATE aggregate operator -- must see the whole sequence.
    public static double Median(this IEnumerable<int> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var sorted = source.OrderBy(n => n).ToList();
        if (sorted.Count == 0) throw new InvalidOperationException("Sequence contains no elements.");
        int mid = sorted.Count / 2;
        return sorted.Count % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2.0 : sorted[mid];
    }
}

internal static class Program
{
    private static void Main()
    {
        var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // WhereCustom chains with built-in operators -- callers can't tell the difference
        Console.WriteLine("WhereCustom + built-in OrderByDescending:");
        foreach (int n in numbers.WhereCustom(n => n % 2 == 0).OrderByDescending(n => n))
            Console.Write($"{n} ");
        Console.WriteLine();

        // DistinctBy: first occurrence per genre wins
        var books = new List<(string Title, string Genre)>
        {
            ("1984", "Dystopian"), ("Fahrenheit 451", "Dystopian"),
            ("The Hobbit", "Fantasy"), ("Brave New World", "Dystopian")
        };
        Console.WriteLine("\nDistinctBy(Genre) -- first per genre:");
        foreach (var (t, g) in books.DistinctBy(b => b.Genre))
            Console.WriteLine($"  {g}: {t}");

        // Chunk: last batch smaller when count isn't divisible by size
        Console.WriteLine("\nChunk(3) of 1..10:");
        foreach (int[] chunk in numbers.Chunk(3))
            Console.WriteLine($"  [{string.Join(", ", chunk)}]");

        // Median: immediate, works on sorted copy
        Console.WriteLine($"\nMedian([5,3,1,4,2]): {new List<int>{5,3,1,4,2}.Median()}");
        Console.WriteLine($"Median([5,3,1,4]):   {new List<int>{5,3,1,4}.Median()}");

        // The eager-validation gotcha
        List<int> nullSource = null;

        Console.WriteLine("\nWhereCustom(null): validated eagerly:");
        try   { _ = nullSource.WhereCustom(n => n > 0); }
        catch (ArgumentNullException) { Console.WriteLine("  Threw immediately on call -- correct."); }

        Console.WriteLine("\nBadWhereCustom(null): validation inside yield return:");
        var bad = nullSource.BadWhereCustom(n => n > 0);
        Console.WriteLine("  No exception yet -- body hasn't run at all.");
        try   { foreach (int _ in bad) { } }
        catch (ArgumentNullException) { Console.WriteLine("  Threw NOW, at enumeration -- far from the bug."); }
    }
}
```
