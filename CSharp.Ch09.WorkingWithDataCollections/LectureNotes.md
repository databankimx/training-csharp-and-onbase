# Chapter 9: Working with Data Collections

## What This Lesson Is

The first half of Chapter 9's material, arrays and collections, the foundational in-memory structures the rest of the chapter's topics (ADO.NET, serialization, file I/O, each covered in their own Supplemental project) ultimately read into or write out of. Built from scratch for this migration, using a small, consistent `Book` dataset across every demonstration so the collection types themselves stay the focus.

---

## Arrays: The `Array` Class's Utility Methods

```csharp
Array.Sort(numbers);
Array.Reverse(numbers);
int foundIndex = Array.BinarySearch(numbers, 8);   // requires the array already be sorted
int[] evens = Array.FindAll(numbers, n => n % 2 == 0);
Array.ForEach(numbers, n => Console.Write($"{n} "));
```

Array declaration and basic indexing were already covered back in `CSharp.Ch04.UsingTypes`, this section picks up where that left off: the static utility methods on the `Array` class itself. Worth noting `BinarySearch`'s precondition specifically, it only works correctly on an already-sorted array, calling it on unsorted data returns a meaningless result rather than throwing, an easy mistake to make silently.

---

## `System.Collections`: The Original, Non-Generic Collections

```csharp
var mixedList = new ArrayList();
mixedList.Add("A string");
mixedList.Add(42);
mixedList.Add(new Book("1984", "George Orwell", 1949));
```

`ArrayList`, `Hashtable`, the non-generic `Queue`/`Stack`, all predate generics in C# (introduced in C# 2.0). They store everything as plain `object`, which means two real costs: value types get boxed on the way in and unboxed on the way out (real, measurable overhead, see `CSharp.Ch08.Supplemental.04.ReflectionPerformance` for the same underlying boxing-adjacent cost pattern applied elsewhere), and there's zero compile-time type safety, an `ArrayList` will happily hold a string, an int, and a `Book` side by side, as this demo shows directly. This is exactly why generics exist and why these types are now considered legacy, worth recognizing on sight in older code, but not worth reaching for in anything new.

---

## `System.Collections.Generic`: What You Should Actually Reach For

Six types, each solving a genuinely different shape of problem:

- **`List<T>`**: a resizable, type-safe array. Your default choice for "a bunch of items in order."
- **`Dictionary<TKey, TValue>`**: fast lookup by key. `TryGetValue()` is worth using by habit over `[key]` indexing plus a separate `ContainsKey()` check, one lookup instead of two.
- **`Queue<T>`/`Stack<T>`**: FIFO and LIFO, generic counterparts to the legacy versions, same behavior, real type safety.
- **`HashSet<T>`**: unordered, no duplicates, and genuinely fast set operations:

```csharp
bannedSciFi.IntersectWith(frequentlyBannedBooks);   // in both
everyTitle.UnionWith(frequentlyBannedBooks);         // in either
sciFiOnly.ExceptWith(frequentlyBannedBooks);         // in the first, not the second
```

- **`SortedList<TKey, TValue>`**: like `Dictionary<TKey, TValue>`, but always enumerates in key order, regardless of insertion order, worth confirming for yourself in the demo, three entries added out of chronological order still come back sorted by year.
- **`LinkedList<T>`**: a genuine doubly-linked list, worth reaching for specifically when you need efficient insertion/removal in the *middle* of a sequence (`AddAfter()`), something `List<T>` is comparatively expensive at, since inserting into the middle of a `List<T>` means shifting every element after it.

---

## Custom Collections: `BoundedCollection<T>`

```csharp
public class BoundedCollection<T> : ICollection<T>
{
    private readonly List<T> items = new List<T>();
    public int MaxCapacity { get; }

    public void Add(T item)
    {
        if (items.Count >= MaxCapacity)
            throw new DatabankException($"Cannot add item: collection already holds its maximum of {MaxCapacity} item(s).");
        items.Add(item);
    }
    ...
}
```

This is the concrete answer to "why would I ever build my own collection, when the built-in ones already do everything?" `BoundedCollection<T>` doesn't reimplement storage or iteration, it wraps a perfectly good `List<T>` and adds exactly one rule the built-in type has no way to express: a hard cap on how many items it will ever hold. The value isn't in the storage mechanism, `List<T>` already handles that fine, it's in making a business rule impossible to accidentally violate, rather than something every caller has to remember to check themselves.

Implementing `ICollection<T>` (rather than just exposing an internal `List<T>` directly) matters too: it means `BoundedCollection<T>` genuinely behaves like a first-class collection everywhere a collection is expected, `foreach` works on it, LINQ methods work on it, anything expecting an `ICollection<T>` accepts it, while still enforcing its own rule on every `Add()` call, from anywhere in the codebase, not just wherever the original author remembered to check.

Compare this against `CSharp.Ch05.TextbookCode.IEnumerableTree`/`TreeEnumerator`, which implemented `IEnumerable<T>` for a structure that *isn't* naturally collection-shaped at all (a tree). `BoundedCollection<T>` is the opposite scenario: a genuinely collection-shaped structure that needs one extra rule enforced consistently.
