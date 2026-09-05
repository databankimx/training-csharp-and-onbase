# Chapter 9: Working with Data Collections

## What This Is

Chapter 9 is "Working with Data." This project covers the first, foundational half of that: **arrays and collections** — the in-memory structures almost every other data technique in this chapter ultimately reads into or writes out of.

The remaining sections of the chapter each get their own Supplemental project:

- `Supplemental.01.AdoNetAndEntityFramework` — reading data from a database
- `Supplemental.02.SqlInjection` — doing that safely
- `Supplemental.03.ConnectingToOtherDatabases` — beyond SQL Server
- `Supplemental.04.FileIO` — reading and writing files
- `Supplemental.05.Serialization` — converting objects to and from transportable formats

Every demonstration here uses the same small `Book` dataset, deliberately, so the collection types themselves stay the focus rather than the data.

---

## The Four Categories

From the Chapter Notes:

```
- Arrays: fixed-size, contiguous, fastest option, but the size can't change after creation.
- System.Collections: the ORIGINAL, non-generic collection types (ArrayList, Hashtable,
	Queue, Stack). They store everything as plain "object", meaning boxing/unboxing for
	value types and no compile-time type safety at all. Largely superseded.
- System.Collections.Generic: the modern, type-safe versions (List<T>, Dictionary<TKey,
	TValue>, Queue<T>, Stack<T>, HashSet<T>, SortedList<TKey,TValue>, LinkedList<T>).
	This is what you should reach for by default today.
- Custom Collections: when a built-in collection's storage is fine but you need to
	enforce a rule the built-in type doesn't (a maximum size, a required sort order, a
	validation check on every add), you wrap or implement a collection interface
	yourself.
```

That's the whole chapter in outline. The middle two categories tell a story about C#'s own history — generics arrived in C# 2.0, and everything before them had to store `object`.

---

## Part 1: Arrays and the `Array` Class

Array declaration and indexing were covered back in `CSharp.Ch04.UsingTypes`. This section picks up where that left off: the **static utility methods** on the `Array` class itself.

`ArrayUtilityMethods()` runs through them in sequence:

```csharp
int[] numbers = [5, 2, 8, 1, 9, 3];

Array.Sort(numbers);
Array.Reverse(numbers);
```

Note that these mutate **in place** and return `void`. They do not return a new array. This trips people up coming from LINQ, where `OrderBy()` returns a new sequence and leaves the original alone.

### `BinarySearch` and Its Precondition

```csharp
// Array.BinarySearch requires the array to already be sorted ASCENDING to work correctly
Array.Sort(numbers);
int foundIndex = Array.BinarySearch(numbers, 8);
```

Note the explicit `Array.Sort()` call immediately before. That isn't redundant — the preceding `Array.Reverse()` left the array in descending order, so the sort is genuinely required here.

**This is the trap worth remembering:** calling `BinarySearch` on unsorted data does not throw. It returns a meaningless result — possibly a wrong index, possibly a negative number — and does so silently. The algorithm works by repeatedly halving the search range based on comparisons, which only makes sense if the data is ordered.

Contrast with `Array.IndexOf(numbers, 3)` on the next line, which scans linearly and works on any array. `BinarySearch` is dramatically faster on large sorted arrays (O(log n) vs O(n)); `IndexOf` is correct always. Choose accordingly.

Also worth knowing: when the value isn't found, `BinarySearch` returns a **negative number** whose bitwise complement (`~result`) is the index where the value *would* be inserted. That's genuinely useful for insertion-point logic, and a surprise if you assume `-1` means "not found."

### Copying and Resizing

```csharp
var copy = new int[numbers.Length];
Array.Copy(numbers, copy, numbers.Length);

Array.Resize(ref copy, 3);
```

Note `Array.Resize` takes `ref`. That's the API being honest about something important: **arrays cannot actually be resized.** `Resize` allocates a brand-new array, copies the elements over, and reassigns your variable to point at it. The `ref` is required because the variable itself must change.

This is exactly why `List<T>` exists. If you find yourself calling `Array.Resize` in a loop, you're hand-rolling a worse `List<T>`.

### Predicate-Based Methods

```csharp
bool hasLargeNumber = Array.Exists(numbers, n => n > 8);
int[] evenNumbers = Array.FindAll(numbers, n => n % 2 == 0);
Array.ForEach(numbers, n => Console.Write($"{n} "));
```

These take delegates — the `Predicate<T>` and `Action<T>` types from Chapter 6. They predate LINQ and are largely superseded by it: `Any()`, `Where()`, and a plain `foreach` do the same jobs with more flexibility.

They're worth recognizing in older code. In new code, LINQ is generally the better choice, notably because it composes — `Array.FindAll` returns an array, forcing an allocation, while `Where()` returns a lazy sequence you can chain further. (Chapter 10 covers deferred execution in depth.)

### `Array.Clear` Doesn't Empty Anything

```csharp
Array.Clear(numbers, 0, numbers.Length);
// prints: [0, 0, 0, 0, 0, 0]
```

The output makes the point. `Clear` doesn't remove elements or shrink the array — the length is fixed. It **resets each element to its default value**: `0` for numerics, `false` for `bool`, `null` for reference types.

---

## Part 2: `System.Collections` — The Legacy, Non-Generic Types

These predate generics. They exist to be recognized, not used.

### `ArrayList`: No Type Safety At All

```csharp
var mixedList = new ArrayList
{
	"A string",
	42,
	new Book("1984", "George Orwell", 1949)
};

// Note: ArrayList stores everything as plain "object", so mixing wildly different
//   types like this is perfectly legal, and perfectly easy to do by accident.
foreach (object item in mixedList)
{
	Console.WriteLine($" - {item} (actual type: {item.GetType().Name})");
}
```

A `string`, an `int`, and a `Book` living in the same collection — and the compiler is entirely satisfied. Note the `item.GetType().Name` call, using the reflection from Chapter 8 to reveal what each element actually is at runtime, since the compile-time type is just `object` for all three.

The second half of that comment is the real point: **"and perfectly easy to do by accident."** Nobody deliberately mixes types like this. What happens in practice is that an `ArrayList` intended to hold `Book` objects picks up something else through a code path nobody checked, and the failure appears later as an `InvalidCastException` in unrelated code.

### The Two Costs

**No compile-time type safety.** Getting anything back out requires a cast, and that cast can fail at runtime:

```csharp
Book book = (Book)mixedList[1];   // compiles fine, throws at runtime — index 1 is an int
```

**Boxing.** Every value type added to an `ArrayList` gets boxed — wrapped in a heap-allocated object — and unboxed on the way out. That's an allocation per element plus GC pressure, entirely absent from `List<int>`. This is the same boxing cost `Chapter 8 Supplemental.04` measured in a different context.

### `Hashtable` and the Legacy `Queue`/`Stack`

```csharp
var byAuthor = new Hashtable
{
	["Orwell"] = new Book("1984", "George Orwell", 1949),
	["Bradbury"] = new Book("Fahrenheit 451", "Ray Bradbury", 1953)
};

foreach (DictionaryEntry entry in byAuthor)
{
	Console.WriteLine($" - {entry.Key}: {entry.Value}");
}
```

Note the loop variable type: `DictionaryEntry`, whose `Key` and `Value` are both `object`. The generic `Dictionary<TKey, TValue>` uses `KeyValuePair<TKey, TValue>` instead, with properly typed members — the direct comparison appears later in `UsingDictionary()`.

`UsingLegacyQueueAndStack()` demonstrates FIFO and LIFO behavior with strings. Same semantics as the generic versions, same `object`-based drawbacks.

> **One historical footnote:** `Hashtable` has a genuine property `Dictionary<TKey, TValue>` lacks — it's thread-safe for a single writer with multiple concurrent readers. That's why it lingered longer than the other legacy types. Today, `ConcurrentDictionary` (Chapter 7, `Supplemental.09`) is the correct answer for concurrent access.

---

## Part 3: `System.Collections.Generic` — What to Actually Use

Six types, each solving a genuinely different shape of problem.

### `List<T>` — The Default Choice

```csharp
var books = new List<Book> { ... };

books.Sort((a, b) => a.Year.CompareTo(b.Year));

var found = books.Find(b => b.Author == "Ray Bradbury");
bool anyPre1940 = books.Exists(b => b.Year < 1940);
```

A resizable, type-safe array. Your default for "a bunch of items in order."

`Sort()` takes a `Comparison<T>` delegate here rather than requiring `Book` to implement `IComparable<T>` — useful when you want to sort by different criteria in different places. (Chapter 5's `IComparableCars` and `IComparerCars` cover the alternative approaches.)

Note that `Sort()` mutates in place, matching `Array.Sort` and differing from LINQ's `OrderBy()`.

Internally, `List<T>` is an array that reallocates to double its capacity when full. That means indexing is O(1) and appending is amortized O(1), but **inserting into the middle is O(n)** because everything after the insertion point shifts. Remember that for the `LinkedList<T>` comparison below.

### `Dictionary<TKey, TValue>` — Fast Lookup by Key

```csharp
if (byTitle.TryGetValue("1984", out var book))
{
	Console.WriteLine($"TryGetValue(\"1984\"): {book}");
}

Console.WriteLine($"ContainsKey(\"Dune\"): {byTitle.ContainsKey("Dune")}");

foreach (KeyValuePair<string, Book> pair in byTitle)
{
	Console.WriteLine($" - {pair.Key} => {pair.Value}");
}
```

**Use `TryGetValue()` by habit.** The lecture notes make this point and it's worth reinforcing: the alternative pattern

```csharp
if (byTitle.ContainsKey(key)) { var b = byTitle[key]; ... }   // two lookups
```

hashes the key and walks the bucket **twice**. `TryGetValue` does it once. The indexer alone (`byTitle[key]`) throws `KeyNotFoundException` on a miss, which is why people reach for the `ContainsKey` guard in the first place.

Note the loop type here is `KeyValuePair<string, Book>` — strongly typed on both sides, versus `Hashtable`'s all-`object` `DictionaryEntry`.

One property worth knowing: **enumeration order is not guaranteed.** It's neither insertion order nor sorted order, and you should not depend on it. If you need ordering, that's what `SortedList` is for.

### `Queue<T>` and `Stack<T>` — FIFO and LIFO

```csharp
var queue = new Queue<Book>();
queue.Enqueue(...);
Console.WriteLine($"Queue<Book>.Dequeue(): {queue.Dequeue()}");

var stack = new Stack<Book>();
stack.Push(...);
Console.WriteLine($"Stack<Book>.Pop(): {stack.Pop()}");
```

The generic counterparts to the legacy versions — same behavior, real type safety. `Dequeue()` returns the *first* item added; `Pop()` returns the *last*.

Both throw `InvalidOperationException` when empty. `TryDequeue()`/`TryPop()` are the safe alternatives, following the same pattern as `TryGetValue()`.

### `HashSet<T>` — Uniqueness and Set Operations

```csharp
var scienceFiction = new HashSet<string> { "1984", "Brave New World", "Fahrenheit 451", "Dune" };
var frequentlyBannedBooks = new HashSet<string> { "Fahrenheit 451", "Brave New World", "Beloved" };

var bannedSciFi = new HashSet<string>(scienceFiction);
bannedSciFi.IntersectWith(frequentlyBannedBooks);      // in both

var everyTitle = new HashSet<string>(scienceFiction);
everyTitle.UnionWith(frequentlyBannedBooks);            // in either

var sciFiOnly = new HashSet<string>(scienceFiction);
sciFiOnly.ExceptWith(frequentlyBannedBooks);            // in the first, not the second
```

Unordered, no duplicates, and O(1) `Contains()` — dramatically faster than `List<T>.Contains()`, which scans linearly.

Note the defensive copying: each operation starts with `new HashSet<string>(scienceFiction)` rather than operating on `scienceFiction` directly. **These methods mutate the set they're called on**, so without the copy, the first operation would corrupt the input for the two that follow. That's a genuine and easy mistake.

(LINQ offers non-mutating `Intersect()`, `Union()`, and `Except()` that return new sequences instead. Same concepts, different tradeoff.)

### `SortedList<TKey, TValue>` — Always in Key Order

```csharp
var byYear = new SortedList<int, string>
{
	{ 1953, "Fahrenheit 451" },
	{ 1932, "Brave New World" },
	{ 1949, "1984" }
};

// Even though the entries above were added out of chronological order,
//   SortedList<TKey, TValue> always enumerates in key order automatically.
```

Added as 1953, 1932, 1949 — enumerated as 1932, 1949, 1953. The sort is maintained on insert, not computed on read.

The tradeoff: insertion is O(n) rather than `Dictionary`'s O(1), because the new entry has to be placed in the right position. Worth it when you read in sorted order frequently and insert rarely; not worth it for a write-heavy workload you sort once at the end.

> **Related type:** `SortedDictionary<TKey, TValue>` offers the same sorted enumeration with different performance characteristics — O(log n) insertion via a tree, versus `SortedList`'s array-backed O(n) insert and lower memory use.

### `LinkedList<T>` — Efficient Middle Insertion

```csharp
var timeline = new LinkedList<string>();
var middleNode = timeline.AddFirst("Brave New World (1932)");
timeline.AddAfter(middleNode, "1984 (1949)");
timeline.AddLast("Fahrenheit 451 (1953)");
timeline.AddFirst("The Time Machine (1895)");
```

A genuine doubly-linked list. Note that `AddFirst()` **returns the node it created** — that `LinkedListNode<string>` handle is what makes `AddAfter()` possible.

That's the whole value proposition: given a node reference, inserting next to it is O(1), just pointer rewiring. `List<T>.Insert()` in the middle is O(n) because every subsequent element shifts.

The cost is real, though, and worth stating plainly: **`LinkedList<T>` has no indexer.** There is no `timeline[2]`. Reaching the Nth element means walking from one end. It also allocates a node object per element, and those nodes are scattered across the heap rather than contiguous, which is much worse for CPU cache performance.

In practice `List<T>` wins more often than the theory suggests, precisely because of that cache behavior. Reach for `LinkedList<T>` when you're genuinely doing many middle insertions **and** you already hold node references.

---

## Part 4: Custom Collections — `BoundedCollection<T>`

This is the concrete answer to "why would I ever build my own collection, when the built-in ones already do everything?"

```csharp
public class BoundedCollection<T> : ICollection<T>
{
	// The actual backing storage. BoundedCollection doesn't reimplement storage/iteration
	//   logic itself, it wraps an existing, well-tested collection and adds one rule on top.
	private readonly List<T> items = [];

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

### It Wraps, It Doesn't Reimplement

Note what this class does *not* do: it doesn't manage an internal array, handle resizing, or write iteration logic. It delegates all of that to a `List<T>`.

Every other `ICollection<T>` member is a one-line forward:

```csharp
public int Count => items.Count;
public void Clear() => items.Clear();
public bool Contains(T item) => items.Contains(item);
public bool Remove(T item) => items.Remove(item);
public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
```

`Add()` is the only method with actual behavior. **The value isn't in the storage mechanism — `List<T>` already handles that fine. It's in making a business rule impossible to accidentally violate.**

Compare the alternative: exposing a plain `List<Book>` and requiring every caller to check the count before adding. That works exactly as long as everyone remembers. `BoundedCollection<T>` enforces it centrally, on every `Add()` from anywhere in the codebase.

### Why Implement `ICollection<T>` Rather Than Expose a `List<T>`

Implementing the interface means `BoundedCollection<T>` genuinely *behaves* like a collection everywhere one is expected:

- `foreach` works on it
- LINQ methods work on it
- Anything accepting an `ICollection<T>` accepts it
- Collection initializer syntax works on it — which is exactly what the demo uses:

```csharp
var recentReads = new BoundedCollection<Book>(maxCapacity: 3)
{
	new("1984", "George Orwell", 1949),
	...
};
```

That initializer syntax works **because** the class has a public `Add(T)` method and implements `IEnumerable`. The compiler translates the braces into `Add()` calls — which means the capacity rule is enforced even here.

### The Two-Enumerator Requirement

```csharp
public IEnumerator<T> GetEnumerator()
{
	return items.GetEnumerator();
}

// Explicit non-generic IEnumerable implementation, required because ICollection<T>
//   inherits from the non-generic IEnumerable as well as IEnumerable<T>
IEnumerator IEnumerable.GetEnumerator()
{
	return GetEnumerator();
}
```

This pattern confuses people the first time. `ICollection<T>` inherits `IEnumerable<T>`, which inherits the **non-generic** `IEnumerable` — a holdover from the pre-generics era, kept so modern collections still work with legacy code.

Since both interfaces declare a `GetEnumerator()` differing only in return type (which C# won't allow as an overload), the non-generic one is implemented **explicitly** — note the lack of an access modifier and the `IEnumerable.` prefix. It's only callable through an `IEnumerable` reference, so it stays out of the way, and it just delegates to the generic version.

### `IsReadOnly` Returns `false`

```csharp
/// Always false: this collection is never read-only, it can always be added to and
/// removed from, up to MaxCapacity
public bool IsReadOnly => false;
```

Worth a note on semantics: `IsReadOnly` means "this collection cannot be modified at all," not "this collection is currently full." A `BoundedCollection<T>` at capacity still supports `Remove()` and `Clear()`, so `false` is correct.

### The Demo

```csharp
Console.WriteLine($"{Environment.NewLine}Attempting to add a fourth book...");
try
{
	recentReads.Add(new Book("Dune", "Frank Herbert", 1965));
}
catch (DatabankException ex)
{
	Console.WriteLine(ex.Message);
}
```

Three books fill a capacity-3 collection; the fourth throws. Note the use of `DatabankException` rather than a built-in type, consistent with the standards applied throughout these lessons.

### A Design Question Worth Considering

Throwing on a full collection is one valid choice. Others exist, and which is right depends entirely on the business rule:

- **Throw** (what this does) — adding past capacity is a programming error
- **Return `false`** — adding past capacity is an expected outcome the caller handles
- **Evict the oldest** — a most-recently-used cache, where "recentReads" arguably wants this

Note that `ICollection<T>.Add()` returns `void`, which rules out the second option without deviating from the interface. That's a real constraint interfaces impose, and worth noticing.

### Contrast With Chapter 5

The lecture notes draw a useful comparison:

> Compare this against `CSharp.Ch05.TextbookCode.IEnumerableTree`/`TreeEnumerator`, which implemented `IEnumerable<T>` for a structure that *isn't* naturally collection-shaped at all (a tree). `BoundedCollection<T>` is the opposite scenario: a genuinely collection-shaped structure that needs one extra rule enforced consistently.

Two different reasons to implement a collection interface: to make something non-collection-shaped *iterable*, or to make something collection-shaped *enforce a rule*.

---

## What to Take Away

**Arrays are fixed-size, and `Array.Resize` doesn't change that** — it allocates a new array and reassigns your reference, which is why it needs `ref`.

**`BinarySearch` silently returns garbage on unsorted data.** It doesn't throw. Sort first, or use `IndexOf`.

**The non-generic collections cost you type safety and boxing.** Recognize `ArrayList` and `Hashtable` in old code; don't write them in new code.

**Pick the generic collection that matches your access pattern.** `List<T>` for ordered items, `Dictionary<TKey, TValue>` for key lookup, `HashSet<T>` for uniqueness and set math, `SortedList` for always-sorted enumeration, `Queue<T>`/`Stack<T>` for FIFO/LIFO, `LinkedList<T>` for middle insertion with node references in hand.

**Prefer `TryGetValue()` over `ContainsKey()` plus indexing.** One lookup instead of two, and no exception on a miss.

**`HashSet<T>`'s set operations mutate the set they're called on.** Copy first, as the demo does, or use LINQ's non-mutating equivalents.

**Build a custom collection to enforce a rule, not to reinvent storage.** Wrap a `List<T>`, implement `ICollection<T>`, and put the rule in `Add()` — then it holds everywhere, not just where callers remember to check.
