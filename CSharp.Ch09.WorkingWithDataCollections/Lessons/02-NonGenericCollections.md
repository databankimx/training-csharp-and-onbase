---
title: "Non-Generic Collections - ArrayList, Hashtable"
chapter: 9
index: 2
dependencies: []
---

```csharp
using System;
using System.Collections;

internal static class Program
{
    private static void Main()
    {
        // ArrayList: the original pre-generics dynamic array.
        // Stores everything as object -- mixing types like this is legal and easy to do by accident.
        // Getting anything out requires a cast that can fail at runtime.
        // Value types like 42 get BOXED on the way in and unboxed on the way out -- a heap
        // allocation per element, entirely absent from List<int>.
        var mixedList = new ArrayList { "A string", 42, 3.14, true };

        Console.WriteLine("ArrayList (object store, no compile-time type safety):");
        foreach (object item in mixedList)
            Console.WriteLine($"  {item,8}  ({item.GetType().Name})");
        Console.WriteLine();

        // Hashtable: the original pre-generics key/value store.
        // Key and Value are both object -- compare to Dictionary<TKey,TValue>'s
        // strongly-typed KeyValuePair<TKey,TValue>.
        // Thread-safe for a single writer with multiple readers (legacy advantage).
        // Today use ConcurrentDictionary<TKey,TValue> instead.
        var byAuthor = new Hashtable
        {
            ["Orwell"]  = "1984",
            ["Huxley"]  = "Brave New World",
            ["Herbert"] = "Dune"
        };

        Console.WriteLine("Hashtable (DictionaryEntry.Key and .Value are both object):");
        foreach (DictionaryEntry entry in byAuthor)
            Console.WriteLine($"  {entry.Key}: {entry.Value}");
        Console.WriteLine();

        Console.WriteLine("Rule: recognise these in legacy code; write List<T> and Dictionary<TKey,TValue> in new code.");
    }
}
```
