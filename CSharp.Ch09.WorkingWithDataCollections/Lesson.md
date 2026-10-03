# Chapter 9: Working with Data Collections

## What This Is

Chapter 9 is "Working with Data." This project covers the foundational half: arrays and collections - the in-memory structures almost every other data technique in this chapter ultimately reads into or writes out of. The supplementals cover the rest: ADO.NET and Entity Framework, SQL injection, connecting to other databases, file I/O, and serialization.

Every demonstration here uses the same small `Book` dataset so the collection types stay in focus rather than the data. Same three books, eleven different data structures.

The story this chapter tells is actually C# history. Before generics arrived in C# 2.0, everything went into `ArrayList` and `Hashtable` as plain `object`. Then generics showed up and made type-safe collections possible. The non-generic types are still in the BCL, still in legacy codebases, and still worth recognizing - just not writing.

---

## How to Write This Program

Add `Models/Objects/Book.cs` - it's the shared dataset for everything:

```csharp
public class Book
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int Year { get; set; }

    public Book() { }
    public Book(string title, string author, int year)
    {
        Title = title; Author = author; Year = year;
    }

    public override string ToString() => $"{Title} by {Author} ({Year})";
}
```

---

## Part 1: Arrays and the Array Class

### Mini-Program 1: Array Utility Methods

Clear `Main()` and write:

```csharp
int[] numbers = [5, 2, 8, 1, 9, 3];
Console.WriteLine($"Original: {string.Join(", ", numbers)}");

Array.Sort(numbers);
Console.WriteLine($"After Sort:    {string.Join(", ", numbers)}");

Array.Reverse(numbers);
Console.WriteLine($"After Reverse: {string.Join(", ", numbers)}");

// BinarySearch requires the array to be sorted ASCENDING -- the Reverse above
// left it descending, so sort again first.
Array.Sort(numbers);
int foundIndex = Array.BinarySearch(numbers, 8);
Console.WriteLine($"\nBinarySearch(numbers, 8): index {foundIndex}");

int indexOfThree = Array.IndexOf(numbers, 3);
Console.WriteLine($"IndexOf(numbers, 3): index {indexOfThree}");

var copy = new int[numbers.Length];
Array.Copy(numbers, copy, numbers.Length);
Console.WriteLine($"\nCopy: {string.Join(", ", copy)}");

Array.Resize(ref copy, 3);
Console.WriteLine($"After Resize(ref copy, 3): {string.Join(", ", copy)}");

bool hasLargeNumber = Array.Exists(numbers, n => n > 8);
Console.WriteLine($"\nExists(n => n > 8): {hasLargeNumber}");

int[] evens = Array.FindAll(numbers, n => n % 2 == 0);
Console.WriteLine($"FindAll(n => n % 2 == 0): {string.Join(", ", evens)}");

Console.Write("ForEach: ");
Array.ForEach(numbers, n => Console.Write($"{n} "));
Console.WriteLine();

Array.Clear(numbers, 0, numbers.Length);
Console.WriteLine($"\nAfter Clear: [{string.Join(", ", numbers)}]");

GenericFunctions.Pause();
```

Run it and note a few things.

`Sort` and `Reverse` mutate in place and return `void`. They do not return a new array. This trips people up coming from LINQ, where `OrderBy()` returns a new sequence and leaves the original alone.

`BinarySearch` has a silent failure mode worth memorizing: calling it on unsorted data does not throw - it returns a meaningless result, possibly a wrong index, possibly a negative number. The algorithm works by halving the search range based on comparisons, which only makes sense on sorted data. `IndexOf` scans linearly and works on anything. When `BinarySearch` doesn't find the value, the negative return value's bitwise complement (`~result`) is the index where the value would be inserted - genuinely useful for insertion-point logic, and a surprise if you assume `-1` means "not found."

`Array.Resize` takes `ref` because arrays cannot actually be resized. `Resize` allocates a new array, copies elements, and reassigns your variable. The `ref` is the API being honest about it. If you find yourself calling `Array.Resize` in a loop, you're hand-rolling a worse `List<T>`.

`Array.Clear` doesn't remove elements or shrink the array - the length is fixed. It resets each element to its default: `0` for numerics, `false` for `bool`, `null` for reference types.

---

## Part 2: System.Collections - The Non-Generic Types

Exist to be recognized, not written.

### Mini-Program 2: ArrayList

Clear `Main()` and write:

```csharp
var mixedList = new ArrayList
{
    "A string",
    42,
    new Book("1984", "George Orwell", 1949)
};

Console.WriteLine("ArrayList contents (mixed types, no compile-time safety):");
foreach (object item in mixedList)
    Console.WriteLine($" - {item} (type: {item.GetType().Name})");

GenericFunctions.Pause();
```

Run it. A string, an int, and a Book in the same collection - and the compiler is entirely satisfied. Nobody deliberately mixes types like this. What happens in practice is that an `ArrayList` intended to hold one type picks up something else through a code path nobody checked, and the failure appears later as an `InvalidCastException` in code that had nothing to do with the insert.

Getting anything out requires a cast that can fail at runtime. Value types like that `42` get boxed on the way in and unboxed on the way out - a heap allocation per element, entirely absent from `List<int>`.

### Mini-Program 3: Hashtable and Legacy Queue/Stack

Clear `Main()` and write:

```csharp
var byAuthor = new Hashtable
{
    ["Orwell"]   = new Book("1984", "George Orwell", 1949),
    ["Bradbury"] = new Book("Fahrenheit 451", "Ray Bradbury", 1953)
};

Console.WriteLine("Hashtable contents:");
foreach (DictionaryEntry entry in byAuthor)
    Console.WriteLine($" - {entry.Key}: {entry.Value}");

var queue = new Queue();
queue.Enqueue("First in line");
queue.Enqueue("Second in line");
queue.Enqueue("Third in line");
Console.WriteLine($"\nQueue.Dequeue(): {queue.Dequeue()}");

var stack = new Stack();
stack.Push("Pushed first");
stack.Push("Pushed second");
stack.Push("Pushed third");
Console.WriteLine($"Stack.Pop(): {stack.Pop()}");

GenericFunctions.Pause();
```

Run it. Note the `DictionaryEntry` loop variable - `Key` and `Value` are both `object`, versus `Dictionary<TKey, TValue>`'s strongly-typed `KeyValuePair<TKey, TValue>`. `Hashtable` is thread-safe for a single writer with multiple concurrent readers - that's why it survived as long as it did. Today the answer is `ConcurrentDictionary` from Chapter 7.

---

## Part 3: System.Collections.Generic - What to Actually Use

### Mini-Program 4: List\<T\>

Clear `Main()` and write:

```csharp
var books = new List<Book>
{
    new("1984", "George Orwell", 1949),
    new("Brave New World", "Aldous Huxley", 1932),
    new("Fahrenheit 451", "Ray Bradbury", 1953)
};

books.Sort((a, b) => a.Year.CompareTo(b.Year));
Console.WriteLine("Sorted by year:");
foreach (var book in books) Console.WriteLine($" - {book}");

var found = books.Find(b => b.Author == "Ray Bradbury");
Console.WriteLine($"\nFind(b => b.Author == \"Ray Bradbury\"): {found}");

bool anyPre1940 = books.Exists(b => b.Year < 1940);
Console.WriteLine($"Exists(b => b.Year < 1940): {anyPre1940}");

GenericFunctions.Pause();
```

Run it. Your default for "a bunch of items in order." Resizable, type-safe, indexed.

Internally `List<T>` is an array that doubles its capacity when full. Indexing is O(1), appending is amortized O(1), but **inserting into the middle is O(n)** because everything after the insertion point shifts. Remember that when you get to `LinkedList<T>`.

### Mini-Program 5: Dictionary\<TKey, TValue\>

Clear `Main()` and write:

```csharp
var byTitle = new Dictionary<string, Book>
{
    ["1984"]           = new Book("1984", "George Orwell", 1949),
    ["Fahrenheit 451"] = new Book("Fahrenheit 451", "Ray Bradbury", 1953)
};

if (byTitle.TryGetValue("1984", out var book))
    Console.WriteLine($"TryGetValue(\"1984\"): {book}");

Console.WriteLine($"ContainsKey(\"Dune\"): {byTitle.ContainsKey("Dune")}");

Console.WriteLine("\nAll entries:");
foreach (KeyValuePair<string, Book> pair in byTitle)
    Console.WriteLine($" - {pair.Key} => {pair.Value}");

GenericFunctions.Pause();
```

Run it. Use `TryGetValue()` by habit. The pattern `if (ContainsKey(k)) { var v = dict[k]; }` hashes the key twice. `TryGetValue` does it once. The indexer alone throws `KeyNotFoundException` on a miss. Enumeration order is not guaranteed.

### Mini-Program 6: Queue\<T\> and Stack\<T\>

Clear `Main()` and write:

```csharp
var queue = new Queue<Book>();
queue.Enqueue(new Book("1984", "George Orwell", 1949));
queue.Enqueue(new Book("Brave New World", "Aldous Huxley", 1932));
Console.WriteLine($"Queue<Book>.Dequeue(): {queue.Dequeue()}");

var stack = new Stack<Book>();
stack.Push(new Book("1984", "George Orwell", 1949));
stack.Push(new Book("Brave New World", "Aldous Huxley", 1932));
Console.WriteLine($"Stack<Book>.Pop(): {stack.Pop()}");

GenericFunctions.Pause();
```

Run it. `Dequeue()` returns the first item added (FIFO). `Pop()` returns the last (LIFO). Both throw `InvalidOperationException` when empty - use `TryDequeue()`/`TryPop()` for the safe alternatives.

### Mini-Program 7: HashSet\<T\>

Clear `Main()` and write:

```csharp
var scienceFiction = new HashSet<string> { "1984", "Brave New World", "Fahrenheit 451", "Dune" };
var frequentlyBanned = new HashSet<string> { "Fahrenheit 451", "Brave New World", "Beloved" };

// Set operations mutate the set they're called on. Copy first or you'll corrupt the inputs.
var bannedSciFi = new HashSet<string>(scienceFiction);
bannedSciFi.IntersectWith(frequentlyBanned);
Console.WriteLine($"IntersectWith (in both):       {string.Join(", ", bannedSciFi)}");

var everyTitle = new HashSet<string>(scienceFiction);
everyTitle.UnionWith(frequentlyBanned);
Console.WriteLine($"UnionWith (in either):         {string.Join(", ", everyTitle)}");

var sciFiOnly = new HashSet<string>(scienceFiction);
sciFiOnly.ExceptWith(frequentlyBanned);
Console.WriteLine($"ExceptWith (sciFi not banned): {string.Join(", ", sciFiOnly)}");

GenericFunctions.Pause();
```

Run it. Unordered, no duplicates, O(1) `Contains()`. The defensive copying before each operation is not paranoia - `IntersectWith`, `UnionWith`, and `ExceptWith` all mutate the set they're called on. LINQ's `Intersect()`, `Union()`, and `Except()` return new sequences instead - same concepts, non-destructive.

### Mini-Program 8: SortedList\<TKey, TValue\>

Clear `Main()` and write:

```csharp
var byYear = new SortedList<int, string>
{
    { 1953, "Fahrenheit 451" },
    { 1932, "Brave New World" },
    { 1949, "1984" }
};

Console.WriteLine("SortedList<int, string>, added out of order:");
foreach (var pair in byYear)
    Console.WriteLine($" - {pair.Key}: {pair.Value}");

GenericFunctions.Pause();
```

Run it. Added 1953, 1932, 1949 - enumerated 1932, 1949, 1953. The sort is maintained on insert, not computed on read. Insertion is O(n); `SortedDictionary<TKey, TValue>` offers O(log n) insertion via a tree at the cost of higher memory use.

### Mini-Program 9: LinkedList\<T\>

Clear `Main()` and write:

```csharp
var timeline = new LinkedList<string>();
var middleNode = timeline.AddFirst("Brave New World (1932)");
timeline.AddAfter(middleNode, "1984 (1949)");
timeline.AddLast("Fahrenheit 451 (1953)");
timeline.AddFirst("The Time Machine (1895)");

Console.WriteLine("LinkedList<string> timeline:");
foreach (var entry in timeline) Console.WriteLine($" - {entry}");

GenericFunctions.Pause();
```

Run it. `AddFirst()` returns the node it created - that `LinkedListNode<string>` handle is what makes `AddAfter()` possible. Given a node reference, inserting next to it is O(1). `List<T>.Insert()` in the middle is O(n). The cost is no indexer - reaching the Nth element means walking from one end. Each element also allocates a node object scattered across the heap, which is worse for CPU cache performance than `List<T>`'s contiguous array. In practice, `List<T>` wins more often than the theory suggests.

---

## Part 4: Custom Collections

### Step 1: BoundedCollection\<T\>

Add `Models/Collections/BoundedCollection.cs`:

```csharp
public class BoundedCollection<T> : ICollection<T>
{
    private readonly List<T> items = [];

    public int MaxCapacity { get; }

    public BoundedCollection(int maxCapacity) { MaxCapacity = maxCapacity; }

    public void Add(T item)
    {
        if (items.Count >= MaxCapacity)
            throw new DatabankException(
                $"Cannot add item: collection already holds its maximum of {MaxCapacity} item(s).");
        items.Add(item);
    }

    public int Count => items.Count;
    public bool IsReadOnly => false;
    public void Clear() => items.Clear();
    public bool Contains(T item) => items.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
    public bool Remove(T item) => items.Remove(item);
    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

    // Explicit non-generic implementation required because ICollection<T> inherits
    // from both IEnumerable<T> and the non-generic IEnumerable, which both declare
    // GetEnumerator() with different return types (not a valid C# overload).
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

### Mini-Program 10: Using BoundedCollection\<T\>

Clear `Main()` and write:

```csharp
var recentReads = new BoundedCollection<Book>(maxCapacity: 3)
{
    new("1984", "George Orwell", 1949),
    new("Brave New World", "Aldous Huxley", 1932),
    new("Fahrenheit 451", "Ray Bradbury", 1953)
};

Console.WriteLine($"Count: {recentReads.Count} / MaxCapacity: {recentReads.MaxCapacity}");
foreach (var book in recentReads) Console.WriteLine($" - {book}");

Console.WriteLine("\nAttempting to add a fourth book...");
try
{
    recentReads.Add(new Book("Dune", "Frank Herbert", 1965));
}
catch (DatabankException ex)
{
    Console.WriteLine(ex.Message);
}

GenericFunctions.Pause();
```

Run it. Three books go in, the fourth throws.

The collection initializer syntax works because `BoundedCollection<T>` implements `IEnumerable` and has a public `Add(T)` method - the compiler translates the braces into `Add()` calls, which means the capacity rule is enforced even during initialization.

The value isn't the storage mechanism - `List<T>` handles that fine and `BoundedCollection<T>` doesn't reimplement any of it. The value is making a business rule impossible to accidentally violate, centrally, rather than relying on every caller to remember to check first.

`IsReadOnly` returns `false` because the collection is never read-only - it can always have items removed. Being at capacity isn't the same thing as being read-only.

The two-enumerator pattern is required because `ICollection<T>` inherits both the generic and non-generic `IEnumerable`, which both declare `GetEnumerator()` with different return types. C# won't allow them as overloads, so the non-generic one is implemented explicitly - callable only through an `IEnumerable` reference, invisible otherwise, delegating to the generic version.

---

## Summary: Which Collection for Which Problem

| Collection | Ordered? | Indexed? | Unique? | Best for |
|---|---|---|---|---|
| `Array` | Yes | Yes | No | Fixed-size, high-performance indexed access |
| `List<T>` | Yes | Yes | No | General-purpose ordered collection |
| `Dictionary<TKey,TValue>` | No | By key | Keys only | Fast key-based lookup |
| `HashSet<T>` | No | No | Yes | Uniqueness enforcement, set operations |
| `SortedList<TKey,TValue>` | By key | By key | Keys only | Always-sorted key enumeration |
| `Queue<T>` | FIFO | No | No | First-in-first-out processing |
| `Stack<T>` | LIFO | No | No | Last-in-first-out processing |
| `LinkedList<T>` | Yes | No | No | Frequent middle insertion with node references |

---

## Takeaways

- Arrays are fixed-size. `Array.Resize` allocates a new array and reassigns your reference - that's what the `ref` is for.
- `BinarySearch` silently returns garbage on unsorted data. Sort first, or use `IndexOf`.
- The non-generic collections cost type safety and boxing. Recognize them in old code; don't write them in new code.
- `TryGetValue()` over `ContainsKey()` plus indexing. One lookup, no exception on a miss.
- `HashSet<T>` set operations mutate in place. Copy first, or use LINQ's non-mutating equivalents.
- Build a custom collection to enforce a rule, not to reinvent storage. Wrap a `List<T>`, implement `ICollection<T>`, put the rule in `Add()`.

---

## Also in Chapter 9

Five supplemental projects accompany this one:

1. `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework`
2. `CSharp.Ch09.Supplemental.02.SqlInjection`
3. `CSharp.Ch09.Supplemental.03.ConnectingToOtherDatabases`
4. `CSharp.Ch09.Supplemental.04.FileIO`
5. `CSharp.Ch09.Supplemental.05.Serialization`
