# Chapter 10 Supplemental 03: Custom LINQ Extension Methods

## What This Is

Every LINQ operator you've used - `Where()`, `Select()`, `OrderBy()`, all of them - is just an extension method over `IEnumerable<T>`. There's nothing magic about them. Writing your own is entirely reasonable when you have a genuinely reusable query shape that isn't already covered.

What's being extended here is the `IEnumerable<T>` extension point itself. Because LINQ operators are ordinary extension methods returning `IEnumerable<T>`, any custom method with the same signature chains seamlessly with built-in operators. A caller can't tell the difference between `WhereCustom()` and `Where()` from the call site.

Two of the operators here (`DistinctByCustom` and `ChunkCustom`) aren't just teaching exercises. `DistinctBy()` and `Chunk()` were only added to .NET's own LINQ in .NET 6. This project targets `net48`, so those built-ins don't exist. Building them by hand is the actual, practical fix for any `net48` codebase that needs this functionality.

---

## How to Write This Program

### Step 1: The Extension Class

Create `CustomLinqExtensions.cs`. Extension methods must live in a `static` class:

```csharp
public static class CustomLinqExtensions
{
    // Methods go here
}
```

Build all five methods before moving to the mini-programs.

**WhereCustom - with the eager/deferred split:**

```csharp
public static IEnumerable<T> WhereCustom<T>(
    this IEnumerable<T> source, Func<T, bool> predicate)
{
    // Validation here, in the public method, runs IMMEDIATELY when called.
    if (source == null)    throw new ArgumentNullException(nameof(source));
    if (predicate == null) throw new ArgumentNullException(nameof(predicate));

    return WhereCustomIterator(source, predicate);
}

// The actual work is in a separate iterator method.
// yield return methods don't start executing until their first MoveNext() call.
// Splitting them out is what makes the validation above run eagerly.
private static IEnumerable<T> WhereCustomIterator<T>(
    IEnumerable<T> source, Func<T, bool> predicate)
{
    foreach (T item in source)
        if (predicate(item)) yield return item;
}
```

**BadWhereCustom - without the split (deliberately broken for the demo):**

```csharp
public static IEnumerable<T> BadWhereCustom<T>(
    this IEnumerable<T> source, Func<T, bool> predicate)
{
    // This method itself uses yield return, so NONE of its body runs until
    // the first MoveNext() call. That includes these validation checks.
    if (source == null)    throw new ArgumentNullException(nameof(source));
    if (predicate == null) throw new ArgumentNullException(nameof(predicate));

    foreach (T item in source)
        if (predicate(item)) yield return item;
}
```

**DistinctByCustom:**

```csharp
public static IEnumerable<T> DistinctByCustom<T, TKey>(
    this IEnumerable<T> source, Func<T, TKey> keySelector)
{
    if (source == null)      throw new ArgumentNullException(nameof(source));
    if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

    return DistinctByCustomIterator(source, keySelector);
}

private static IEnumerable<T> DistinctByCustomIterator<T, TKey>(
    IEnumerable<T> source, Func<T, TKey> keySelector)
{
    // HashSet<TKey>.Add() returns false if the value was already present.
    // Yield the item only the first time its key is seen.
    var seenKeys = new HashSet<TKey>();
    foreach (T item in source)
        if (seenKeys.Add(keySelector(item))) yield return item;
}
```

**ChunkCustom:**

```csharp
public static IEnumerable<T[]> ChunkCustom<T>(this IEnumerable<T> source, int size)
{
    if (source == null) throw new ArgumentNullException(nameof(source));
    if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size), "Chunk size must be greater than zero.");

    return ChunkCustomIterator(source, size);
}

private static IEnumerable<T[]> ChunkCustomIterator<T>(IEnumerable<T> source, int size)
{
    var buffer = new List<T>(size);
    foreach (T item in source)
    {
        buffer.Add(item);
        if (buffer.Count == size)
        {
            yield return buffer.ToArray();
            buffer.Clear();
        }
    }
    // Final, possibly-smaller batch
    if (buffer.Count > 0) yield return buffer.ToArray();
}
```

**Median - an immediate aggregate operator:**

```csharp
public static double Median(this IEnumerable<int> source)
{
    if (source == null) throw new ArgumentNullException(nameof(source));

    var sorted = source.OrderBy(n => n).ToList();
    if (sorted.Count == 0) throw new InvalidOperationException("Sequence contains no elements.");

    int mid = sorted.Count / 2;
    return sorted.Count % 2 == 0
        ? (sorted[mid - 1] + sorted[mid]) / 2.0
        : sorted[mid];
}
```

---

### Mini-Program 1: WhereCustom Chains With Real LINQ

Clear `Main()` and write:

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

var result = numbers.WhereCustom(n => n % 2 == 0).OrderByDescending(n => n);

Console.WriteLine("Even numbers via WhereCustom(), chained with real OrderByDescending():");
foreach (int n in result) Console.WriteLine($" - {n}");
GenericFunctions.Pause();
```

Run it. Even numbers in descending order.

Custom extension methods chain with built-in LINQ operators because they share the same type: `IEnumerable<T>` in, `IEnumerable<T>` out. The caller can't tell the difference between `WhereCustom` and `Where` from the call site.

### Mini-Program 2: DistinctByCustom

Clear `Main()` and write:

```csharp
var books = new List<(string Title, string Genre)>
{
    ("1984",                 "Dystopian"),
    ("Fahrenheit 451",       "Dystopian"),
    ("The Hobbit",           "Fantasy"),
    ("Brave New World",      "Dystopian"),
    ("The Name of the Wind", "Fantasy")
};

var oneBookPerGenre = books.DistinctByCustom(b => b.Genre);

Console.WriteLine("One book per genre (first occurrence wins):");
foreach (var (Title, Genre) in oneBookPerGenre)
    Console.WriteLine($" - {Genre}: {Title}");
GenericFunctions.Pause();
```

Run it. One Dystopian, one Fantasy - first occurrence of each genre wins.

The `HashSet<TKey>.Add()` trick inside the iterator is worth understanding: `Add()` returns `false` if the key was already present, which makes "first occurrence wins" a natural one-liner per element.

### Mini-Program 3: ChunkCustom

Clear `Main()` and write:

```csharp
var numbers = Enumerable.Range(1, 10);

Console.WriteLine("Numbers 1-10, chunked into groups of 3:");
foreach (int[] chunk in numbers.ChunkCustom(3))
    Console.WriteLine($" - [{string.Join(", ", chunk)}]");
GenericFunctions.Pause();
```

Run it. Groups of [1,2,3], [4,5,6], [7,8,9], [10] - the last group is smaller, as expected.

Chunking is a common real-world need: sending API requests in batches, processing database rows in pages, splitting a large file into segments. The iterator approach using a buffer and `yield return` handles the "last partial chunk" case cleanly without special-casing.

### Mini-Program 4: Median

Clear `Main()` and write:

```csharp
var oddCount  = new List<int> { 5, 3, 1, 4, 2 };
var evenCount = new List<int> { 5, 3, 1, 4 };

Console.WriteLine($"Median of [5, 3, 1, 4, 2]: {oddCount.Median()}");
Console.WriteLine($"Median of [5, 3, 1, 4]:    {evenCount.Median()}");
GenericFunctions.Pause();
```

Run it. `3` for the odd-count list, `3.5` for the even-count list (average of the two middle values).

`Median` is an **immediate** operator - it has to see the whole sequence to produce its one answer. The implementation calls `ToList()` immediately and works from there. This is the same reason `Count()`, `Sum()`, and `Average()` are immediate: aggregation requires full knowledge of the sequence.

### Mini-Program 5: The Eager-Validation Gotcha

Clear `Main()` and write:

```csharp
List<int> nullSource = null;

Console.WriteLine("Calling WhereCustom(null, ...) - validated eagerly:");
try
{
    var query = nullSource.WhereCustom(n => n > 0);
    Console.WriteLine(" - This line should not print.");
}
catch (ArgumentNullException)
{
    Console.WriteLine(" - Threw immediately, when WhereCustom() was called. Correct.");
}

Console.WriteLine($"\nCalling BadWhereCustom(null, ...) - validated inside the iterator:");
var badQuery = nullSource.BadWhereCustom(n => n > 0);
Console.WriteLine(" - No exception yet, even though the source is null.");
Console.WriteLine("   The body of a yield return method doesn't run until enumeration starts.");

try
{
    foreach (int n in badQuery) { }
}
catch (ArgumentNullException)
{
    Console.WriteLine(" - NOW it throws, only once the foreach started pulling values.");
    Console.WriteLine("   The error location is far from where badQuery was created.");
}

GenericFunctions.Pause();
```

Run it. `WhereCustom` throws immediately. `BadWhereCustom` throws only when the `foreach` starts - potentially far from where the null was introduced.

This is why real LINQ operators use the two-method split: a public method that validates eagerly, calling a private `*Iterator` method that contains the `yield return`. The validation runs when you call the operator. The iteration runs when you enumerate.

`BadWhereCustom` has `yield return` directly in the public method. Calling it doesn't execute the body - the body of a `yield return` method doesn't start until the first `MoveNext()` call on the returned enumerator. That includes the null check. The exception is delayed to a point that may not look related to the original mistake at all.

---

## Try It Yourself

Run Mini-Program 5 and watch the difference: `WhereCustom(null, ...)` throws immediately, `BadWhereCustom(null, ...)` doesn't throw until the `foreach` loop runs. Same mistake, very different debugging experience. That's the whole case for the eager-validation split.

---

## Summary: Deferred vs. Immediate, and the Two-Method Pattern

| Operator type | Has `yield return`? | Runs when? |
|---|---|---|
| Deferred (like `WhereCustom`) | Yes, in private iterator | On first element request |
| Immediate (like `Median`) | No | When called |

| Pattern | Validation runs | When |
|---|---|---|
| Public method + private iterator | Eagerly | When the operator is called |
| `yield return` in public method (`BadWhereCustom`) | Lazily | When enumeration starts |

---

## Takeaways

- Custom LINQ operators are ordinary extension methods over `IEnumerable<T>`. Nothing magic.
- Return `IEnumerable<T>` and chain with built-in operators exactly as if your method were built-in.
- Split into a public method (for eager validation) and a private iterator method (for deferred work). The public method validates immediately; the iterator's body doesn't run until enumeration starts.
- `yield return` in the public method defers the validation along with everything else - a real, hard-to-diagnose bug.
- Immediate operators (like `Median`) call `ToList()` and work from there.
- `HashSet<TKey>.Add()` returning `false` on duplicates is the natural building block for "first occurrence wins" deduplication.
- `DistinctBy()` and `Chunk()` don't exist on `net48` - hand-rolling them is the practical answer, not just an exercise.
