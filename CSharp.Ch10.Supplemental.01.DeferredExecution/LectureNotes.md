# Chapter 10 Supplemental 01: Deferred Execution

## What This Is

One of the most commonly misunderstood, and most practically important, LINQ concepts: most query operators don't run when you write them, they run when the query is actually enumerated. This lesson makes that distinction concrete, and shows both the useful side of it and a real way it can bite you.

---

## The Core Idea

```csharp
var evenNumbers = numbers.Where(n => n % 2 == 0);
```

This line does **not** filter `numbers`. It builds an object describing "filter `numbers` down to evens, whenever someone actually asks for the results." Nothing in `numbers` has been touched yet. The filtering only happens when `evenNumbers` is enumerated, a `foreach` loop, `.ToList()`, or anything else that pulls values out one at a time.

---

## Consequence One: The Query Sees the Source *At Enumeration Time*

```csharp
var evenNumbers = numbers.Where(n => n % 2 == 0);
numbers.Add(6);
numbers.Add(8);

foreach (int n in evenNumbers) { ... }   // 6 and 8 ARE included
```

Since `Where()` hasn't actually run yet when `6` and `8` get added, by the time the `foreach` loop finally does run it, those new numbers are already part of `numbers`, so they show up in the filtered results too. This can be genuinely useful (a query that automatically reflects the latest state of its source), or genuinely surprising if you expected the query to be a snapshot taken at the moment it was written.

---

## Consequence Two: Enumerating Twice Runs the Work Twice

```csharp
var evenNumbers = numbers.Where(n => { Console.WriteLine($"(evaluating {n})"); return n % 2 == 0; });

foreach (int n in evenNumbers) { ... }   // predicate runs for every element
foreach (int n in evenNumbers) { ... }   // predicate runs AGAIN for every element
```

`evenNumbers` isn't a stored list of results, it's a recipe. Enumerating it twice follows that recipe twice, from scratch, both times. Worth taking seriously as a real performance concern: if the source (or an earlier step in the query chain) does anything expensive, a database round trip, a slow computation, enumerating the same deferred query more than once repeats that expense every single time, not once.

---

## Forcing Immediate Execution: `.ToList()`/`.ToArray()`

```csharp
var evenNumbersSnapshot = numbers.Where(n => n % 2 == 0).ToList();
numbers.Add(6);
numbers.Add(8);
// evenNumbersSnapshot does NOT include 6 or 8, it's a real, independent list, captured
//   the moment .ToList() ran.
```

`.ToList()`/`.ToArray()`/`.ToDictionary()` are the fix for both consequences above: they force the query to actually run right then, and the result is stored, ordinary data, no longer connected to the original query at all. Reach for these specifically when you need either a genuine snapshot (immune to later changes in the source) or a result you're going to enumerate more than once (to avoid redoing the work each time).

---

## A Real Failure Mode: Modifying the Source Mid-Enumeration

```csharp
foreach (int n in evenNumbers)
{
    if (n == 2) numbers.Add(100);   // throws InvalidOperationException
}
```

`List<T>`'s enumerator actively detects being modified while it's still in use and throws `InvalidOperationException` ("Collection was modified; enumeration operation may not execute") rather than risk returning corrupted or inconsistent results. This is a real, common bug shape: looping over a collection (directly, or through a deferred LINQ query built on it) and trying to add to or remove from that same collection inside the loop. The fix is one of: materialize first with `.ToList()` if you need to modify the source during the loop, or collect the changes you want to make in a separate list and apply them after the loop finishes.
