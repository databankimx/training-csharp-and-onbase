---
title: "HashSet, SortedList, LinkedList, and Custom Collection"
chapter: 9
index: 4
dependencies: []
---

```csharp
using System;
using System.Collections;
using System.Collections.Generic;

// A simple custom collection that enforces a maximum capacity.
// The rule lives in Add() -- callers can't bypass it.
// ICollection<T> is the minimal interface that enables collection-initializer syntax.
public class BoundedCollection<T> : ICollection<T>
{
    private readonly List<T> _items = [];
    public int MaxCapacity { get; }
    public BoundedCollection(int maxCapacity) { MaxCapacity = maxCapacity; }

    public void Add(T item)
    {
        if (_items.Count >= MaxCapacity)
            throw new InvalidOperationException(
                $"Cannot add item: collection is already at maximum capacity ({MaxCapacity}).");
        _items.Add(item);
    }

    public int  Count      => _items.Count;
    public bool IsReadOnly => false;
    public void Clear()                           => _items.Clear();
    public bool Contains(T item)                  => _items.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    public bool Remove(T item)                    => _items.Remove(item);
    public IEnumerator<T> GetEnumerator()         => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()       => GetEnumerator();
}

internal static class Program
{
    private static void Main()
    {
        // --- HashSet<T>: unordered, unique, O(1) Contains ---
        // Set operations mutate the set they're called on -- copy first or you corrupt the input.
        // LINQ's Intersect/Union/Except return new sequences and are non-destructive.
        var sciFi   = new HashSet<string> { "1984", "Brave New World", "Fahrenheit 451", "Dune" };
        var banned  = new HashSet<string> { "Fahrenheit 451", "Brave New World", "Beloved" };

        var bannedSciFi = new HashSet<string>(sciFi); bannedSciFi.IntersectWith(banned);
        Console.WriteLine($"IntersectWith (in both):  {string.Join(", ", bannedSciFi)}");

        var all = new HashSet<string>(sciFi); all.UnionWith(banned);
        Console.WriteLine($"UnionWith (in either):    {string.Join(", ", all)}");

        var sciFiOnly = new HashSet<string>(sciFi); sciFiOnly.ExceptWith(banned);
        Console.WriteLine($"ExceptWith (sciFi !banned): {string.Join(", ", sciFiOnly)}");

        // --- SortedList<TKey,TValue>: always sorted by key ---
        // Insertion is O(n) (shift). SortedDictionary<K,V> offers O(log n) via a tree.
        Console.WriteLine("\nSortedList<int,string> (inserted out of order, enumerated in order):");
        var byYear = new SortedList<int, string>
        {
            { 1953, "Fahrenheit 451" }, { 1932, "Brave New World" }, { 1949, "1984" }
        };
        foreach (var p in byYear) Console.WriteLine($"  {p.Key}: {p.Value}");

        // --- LinkedList<T>: O(1) insert given a node reference; no indexer ---
        // Given a node, AddBefore/AddAfter is O(1). List<T>.Insert in the middle is O(n).
        // Trade-off: no indexer (can't do timeline[2]); elements are scattered objects,
        // worse CPU-cache locality than List<T>'s contiguous array.
        Console.WriteLine("\nLinkedList<string>:");
        var timeline = new LinkedList<string>();
        var node1932 = timeline.AddFirst("Brave New World (1932)");
        timeline.AddAfter(node1932, "1984 (1949)");
        timeline.AddLast("Fahrenheit 451 (1953)");
        timeline.AddFirst("The Time Machine (1895)");
        foreach (var entry in timeline) Console.WriteLine($"  {entry}");

        // --- BoundedCollection<T>: custom rule in Add() ---
        // Collection-initializer syntax works because BoundedCollection<T> has Add(T) and IEnumerable.
        // The capacity rule is enforced even during initialization.
        Console.WriteLine("\nBoundedCollection<string>(3):");
        var top3 = new BoundedCollection<string>(3) { "1984", "Brave New World", "Fahrenheit 451" };
        Console.WriteLine($"  Count: {top3.Count} / Max: {top3.MaxCapacity}");
        try { top3.Add("Dune"); }
        catch (InvalidOperationException ex) { Console.WriteLine($"  Add 4th: {ex.Message}"); }
    }
}
```
