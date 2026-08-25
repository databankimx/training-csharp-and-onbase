# Chapter 10 Supplemental 03: Custom LINQ Extension Methods

## What This Is

Every LINQ operator used throughout this chapter, `Where()`, `Select()`, `OrderBy()`, all of it, is just an extension method over `IEnumerable<T>`. Nothing more magical than that. This Supplemental writes a few from scratch: a reimplementation of `Where()` (to show the mechanics), and two genuinely useful operators, `DistinctByCustom()` and `ChunkCustom()`, that fill a real gap in this project's target framework.

---

## Not Just an Exercise: `DistinctBy()`/`Chunk()` Don't Exist on net48

```csharp
var oneBookPerGenre = books.DistinctByCustom(b => b.Genre);
var batches = numbers.ChunkCustom(3);
```

.NET only added `DistinctBy()` and `Chunk()` to its own LINQ starting in .NET 6. This project (like the rest of this training set) targets net48, so those built-ins genuinely don't exist here, calling `.DistinctBy(...)` or `.Chunk(...)` on net48 is a compile error, not a style choice avoided for teaching purposes. `CustomLinqExtensions.cs`'s versions are the actual, practical fix, worth keeping in an internal utility library for any net48 codebase that wants this functionality.

---

## Writing a Deferred Operator: `WhereCustom()`

```csharp
public static IEnumerable<T> WhereCustom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
{
    if (source == null) throw new ArgumentNullException(nameof(source));
    if (predicate == null) throw new ArgumentNullException(nameof(predicate));
    return WhereCustomIterator(source, predicate);
}

private static IEnumerable<T> WhereCustomIterator<T>(IEnumerable<T> source, Func<T, bool> predicate)
{
    foreach (T item in source)
    {
        if (predicate(item)) yield return item;
    }
}
```

`yield return` is what makes `WhereCustomIterator()` deferred (see `CSharp.Ch10.Supplemental.01.DeferredExecution` for what that means in practice): the compiler turns a `yield return` method into a state machine that only advances one step at a time, whenever something calls `MoveNext()` on it (which a `foreach` loop, `.ToList()`, etc. all do). Worth noticing this method is split into two: a public, ordinary method (`WhereCustom()`) and a private iterator method (`WhereCustomIterator()`). That split is deliberate, not incidental, see the next section for why.

---

## The Real Gotcha: Validating Arguments Inside an Iterator Method

```csharp
public static IEnumerable<T> BadWhereCustom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
{
    if (source == null) throw new ArgumentNullException(nameof(source));   // doesn't run yet!
    ...
    foreach (T item in source) { if (predicate(item)) yield return item; }
}
```

Because this whole method uses `yield return`, **none of its body runs until the caller starts enumerating the result**, including the `ArgumentNullException` checks at the top. Calling `BadWhereCustom(null, ...)` doesn't throw. The exception only fires later, whenever a `foreach` loop (or `.ToList()`, or anything else) actually starts pulling values out, which could be far away from where the bad call originally happened, making the real bug much harder to trace back to its source.

`WhereCustom()`'s split fixes this: the public method itself is *not* an iterator (no `yield return` in its own body), so its argument checks run immediately, the moment it's called, exactly like a normal method. It then calls the private iterator method (`WhereCustomIterator()`) to do the actual deferred work. This eager-validation / deferred-execution split is the standard, correct pattern for any custom LINQ operator that validates its arguments, worth recognizing it in other people's code and using it in your own.

---

## Deferred vs. Immediate, One More Time: `Median()`

```csharp
public static double Median(this IEnumerable<int> source)
{
    var sorted = source.OrderBy(n => n).ToList();
    ...
    return sorted.Count % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2.0 : sorted[mid];
}
```

No `yield return` here at all, `Median()` genuinely can't produce a partial answer one element at a time the way `WhereCustom()`/`ChunkCustom()` can, it needs the entire sorted sequence before it can compute anything. This is exactly the same category as the built-in aggregate functions (`Sum()`, `Average()`, `Count()`), immediate rather than deferred, for the same underlying reason: there's no meaningful "next item" to yield when the answer is a single value that depends on the whole sequence.
