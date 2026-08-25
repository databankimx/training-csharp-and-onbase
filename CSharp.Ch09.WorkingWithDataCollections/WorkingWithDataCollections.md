# Working with Data Collections

## Introduction

Chapter 9 is about working with data, and almost everything else in this chapter (databases, files, serialized data) ends up flowing through the same basic in-memory structures: arrays and collections. This lesson covers those structures, from the `Array` class's utility methods, through the original non-generic collections, to the modern generic ones, and finally a custom collection built to enforce a rule none of the built-in types can express.

---

## Array Utility Methods

```csharp
int[] numbers = { 5, 2, 8, 1, 9, 3 };
Array.Sort(numbers);
Array.Reverse(numbers);
Array.Sort(numbers);   // BinarySearch requires ascending order
int index = Array.BinarySearch(numbers, 8);
int[] evens = Array.FindAll(numbers, n => n % 2 == 0);
Array.ForEach(numbers, n => Console.Write($"{n} "));
```

Beyond basic declaration and indexing (covered back in the `Using and Converting Data Types` lesson), the `Array` class itself provides useful static methods: sorting, reversing, searching, filtering, and iterating. Worth remembering: `BinarySearch` only gives a correct answer on an already-sorted array, it won't warn you if you forget to sort first.

---

## The Original Collections: `System.Collections`

```csharp
var mixedList = new ArrayList();
mixedList.Add("A string");
mixedList.Add(42);
mixedList.Add(new Book("1984", "George Orwell", 1949));
```

`ArrayList`, `Hashtable`, and the non-generic `Queue`/`Stack` predate generics in C#. They store everything as plain `object`, meaning value types get boxed going in and unboxed coming out (real overhead), and there's no compile-time type checking at all, an `ArrayList` will cheerfully hold a string, a number, and a custom object side by side, with nothing stopping you. Worth recognizing these when you see them in older code, but not worth choosing for anything new.

---

## The Modern Collections: `System.Collections.Generic`

```csharp
var books = new List<Book> { ... };                              // resizable, ordered
var byTitle = new Dictionary<string, Book> { ... };                // fast lookup by key
var queue = new Queue<Book>(); var stack = new Stack<Book>();      // FIFO / LIFO
var genres = new HashSet<string> { ... };                          // unique, set operations
var byYear = new SortedList<int, string> { ... };                  // always sorted by key
var timeline = new LinkedList<string>();                           // efficient middle insertion
```

Each one solves a different shape of problem: `List<T>` for an ordered, resizable collection; `Dictionary<TKey, TValue>` for fast lookups by key; `Queue<T>`/`Stack<T>` for strict ordering (first-in-first-out or last-in-first-out); `HashSet<T>` for uniqueness and set math (`UnionWith`, `IntersectWith`, `ExceptWith`); `SortedList<TKey, TValue>` for a dictionary that always enumerates in key order regardless of insertion order; `LinkedList<T>` for efficient insertion in the middle of a sequence, something `List<T>` is comparatively slow at.

---

## Building Your Own: `BoundedCollection<T>`

```csharp
public class BoundedCollection<T> : ICollection<T>
{
    private readonly List<T> items = new List<T>();
    public int MaxCapacity { get; }

    public void Add(T item)
    {
        if (items.Count >= MaxCapacity)
            throw new DatabankException("Cannot add item: collection already holds its maximum.");
        items.Add(item);
    }
}
```

Why build a collection when `List<T>` already does everything you need? Here's a concrete case: what if the *storage* is fine, but you need a *rule* enforced, in this example, a hard cap on how many items are allowed. `BoundedCollection<T>` wraps a `List<T>` (no need to reinvent storage) and adds exactly that one rule, enforced on every `Add()` call, no matter where in the code that call happens. Implementing `ICollection<T>` (rather than just exposing the internal list) means it still behaves like a real collection everywhere one is expected, `foreach` works, LINQ works, but the rule can never be accidentally skipped.

---

## Try It Yourself

Run the project and pay close attention to the `BoundedCollection<T>` demo at the end: it adds three books successfully, then tries to add a fourth and catches the resulting exception. Try changing `maxCapacity: 3` to a different number and confirm the behavior updates accordingly, no other code changes needed.
