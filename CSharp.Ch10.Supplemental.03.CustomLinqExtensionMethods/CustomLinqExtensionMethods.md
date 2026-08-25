# Custom LINQ Extension Methods

## Introduction

Every LINQ method you've used, `Where()`, `Select()`, `OrderBy()`, is just an ordinary extension method. There's no special magic making them work, which means you can write your own the same way. This lesson builds a few, including two that genuinely don't exist in this project's target framework at all.

---

## LINQ Methods Are Just Extension Methods

```csharp
public static IEnumerable<T> WhereCustom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
{
    ...
    foreach (T item in source)
    {
        if (predicate(item)) yield return item;
    }
}
```

`yield return` is the key ingredient for a deferred, lazy operator like `Where()`, it turns this method into something that produces one value at a time, only when asked, instead of computing everything up front.

---

## A Real Gap This Fills: `DistinctBy()` and `Chunk()`

```csharp
var oneBookPerGenre = books.DistinctByCustom(b => b.Genre);  // first book per genre
var batches = numbers.ChunkCustom(3);                         // groups of 3
```

Modern .NET has these built in, but only since .NET 6. This project targets an older framework that doesn't have them at all, so these aren't just practice, they're the actual way to get this functionality here.

---

## A Real Gotcha: Validating Arguments Too Late

```csharp
// BAD: validation is INSIDE the yield-return method
public static IEnumerable<T> BadWhereCustom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
{
    if (source == null) throw new ArgumentNullException(nameof(source));   // doesn't run yet!
    foreach (T item in source) { ... }
}
```

Because this whole method uses `yield return`, **nothing in it runs until you actually start looping over the result**. Calling this with a `null` source doesn't throw right away, it throws later, inside whatever `foreach` loop first tries to use it, which can be confusing to debug since the error shows up somewhere completely different from where the actual mistake was made.

The fix: split the method in two, an ordinary method that validates immediately, and a separate `yield return` method it calls for the actual work:

```csharp
public static IEnumerable<T> WhereCustom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
{
    if (source == null) throw new ArgumentNullException(nameof(source));   // runs immediately
    return WhereCustomIterator(source, predicate);
}
```

---

## Not Every Custom Operator Can Be Deferred

```csharp
public static double Median(this IEnumerable<int> source)
{
    var sorted = source.OrderBy(n => n).ToList();
    ...
}
```

`Median()` has no `yield return` at all, it needs to see the entire sequence before it can produce its one answer, the same way `Sum()` or `Average()` do. Some operations just don't have a "next value" to hand out one at a time.

---

## Try It Yourself

Run `DemonstrateEagerValidationGotcha()` and watch the difference: `WhereCustom(null, ...)` throws immediately, `BadWhereCustom(null, ...)` doesn't throw until the `foreach` loop underneath it actually runs. Same mistake, very different debugging experience.
