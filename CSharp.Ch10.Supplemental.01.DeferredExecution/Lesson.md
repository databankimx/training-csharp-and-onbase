# Chapter 10 Supplemental 01: Deferred Execution

## What This Is

The main lesson noted that LINQ queries are deferred - nothing executes until the sequence is enumerated - and moved on. This project is entirely about that one property and its practical consequences, because misunderstanding it is the source of a surprisingly large number of LINQ bugs in real code.

What deferred execution means concretely: writing `var query = collection.Where(...)` doesn't filter anything. It builds a description of the work to do. That work only happens when the query is enumerated - a `foreach` loop, a `ToList()`, or anything else that pulls values out. That gap between "defining the query" and "running the query" has two real consequences that bite people regularly:

1. The query sees whatever the source collection looks like at enumeration time, not when the query was written.
2. Enumerating the same query variable twice runs the underlying work twice, not once.

Both are demonstrable. Both have clear fixes.

---

## How to Write This Program

### Mini-Program 1: Deferred Execution Sees Later Changes

Clear `Main()` and write:

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5 };

// Nothing has run yet. This just describes "even numbers from numbers".
var evenNumbers = numbers.Where(n => n % 2 == 0);

// Modify the source AFTER defining the query, but BEFORE enumerating it.
numbers.Add(6);
numbers.Add(8);

Console.WriteLine("evenNumbers, enumerated AFTER adding 6 and 8:");
foreach (int n in evenNumbers)
    Console.WriteLine($" - {n}");

GenericFunctions.Pause();
```

Run it. You'll see 2, 4, 6, and 8 - even though 6 and 8 were added after `evenNumbers` was defined. The `Where()` clause didn't run until the `foreach` did, and by then the list already had the extra elements.

This works in your favor when you want a "live view" of a collection. It works against you when you expect `evenNumbers` to represent a snapshot of the state when you wrote the query. If you want a snapshot, call `ToList()`.

### Mini-Program 2: Multiple Enumeration Re-Runs the Query

Clear `Main()` and write:

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5 };

// Side effects in a predicate are bad practice in real code.
// Here they make the re-execution visible.
var evenNumbers = numbers.Where(n =>
{
    Console.WriteLine($"   (evaluating {n})");
    return n % 2 == 0;
});

Console.WriteLine("First enumeration:");
foreach (int n in evenNumbers) Console.WriteLine($" - {n}");

Console.WriteLine($"\nSecond enumeration of the SAME query variable:");
foreach (int n in evenNumbers) Console.WriteLine($" - {n}");

GenericFunctions.Pause();
```

Run it. Every "(evaluating N)" line prints twice - the predicate runs completely on both passes. The query has no memory of the previous enumeration.

This matters for performance. Enumerating a deferred query more than once against an expensive source - a database query, a slow API call, a heavy computation - does that expensive work again each time. If you need the results more than once, materialize them with `ToList()` and keep the list.

### Mini-Program 3: Forcing Immediate Execution

Clear `Main()` and write:

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5 };

// .ToList() forces Where() to run RIGHT NOW.
// evenNumbersSnapshot is a real List<int>, not a description of future work.
var evenNumbersSnapshot = numbers.Where(n => n % 2 == 0).ToList();

numbers.Add(6);
numbers.Add(8);

Console.WriteLine("evenNumbersSnapshot, after adding 6 and 8 to the source list:");
foreach (int n in evenNumbersSnapshot)
    Console.WriteLine($" - {n}");

GenericFunctions.Pause();
```

Run it. Only 2 and 4 appear - the snapshot was captured before 6 and 8 existed.

`ToList()`, `ToArray()`, `ToDictionary()`, and the aggregate operators (`Count()`, `Sum()`, `First()`, etc.) are all immediate. They force evaluation right then and return a concrete result, not a lazy sequence.

### Mini-Program 4: Modifying During Enumeration Throws

Clear `Main()` and write:

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var evenNumbers = numbers.Where(n => n % 2 == 0);

try
{
    foreach (int n in evenNumbers)
    {
        Console.WriteLine($" - {n}");
        if (n == 2) numbers.Add(100);
    }
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"\nThrew as expected: {ex.Message}");
    Console.WriteLine("Fix: materialize with .ToList() before the loop, or collect changes");
    Console.WriteLine("and apply them after the loop finishes.");
}

GenericFunctions.Pause();
```

Run it. `List<T>`'s enumerator detects the modification and throws `InvalidOperationException: Collection was modified; enumeration operation may not execute.`

The enumerator tracks a version counter that increments on every structural change to the list. If it doesn't match what the enumerator saw when it started, it throws rather than risk yielding inconsistent or corrupted results.

The fix is always one of two things: materialize first (`foreach (int n in evenNumbers.ToList())`), so the loop iterates over a copy; or collect the changes into a separate list and apply them after the loop.

---

## Try It Yourself

Run Mini-Program 2 and watch the `(evaluating ...)` lines print twice - once per loop. That's deferred execution made visible: the predicate is genuinely running twice because the query has no stored results to return the second time.

---

## Summary: When Queries Execute

| Operation | Execution |
|---|---|
| `var q = source.Where(...)` | Deferred - nothing runs |
| `foreach (var x in q)` | Runs when the first element is requested |
| `q.ToList()` | Immediate - runs the whole query now |
| `q.ToArray()` | Immediate |
| `q.Count()`, `q.Sum()`, etc. | Immediate |
| `q.First()`, `q.Last()` | Immediate |
| Second `foreach` over same `q` | Re-runs the query from scratch |

---

## Takeaways

- LINQ queries are deferred by default - they describe work, not results.
- The query sees the source as it exists at enumeration time, not when the query was written.
- Enumerating a deferred query twice runs the underlying work twice. Materialize with `ToList()` if you need the results more than once or from an expensive source.
- `ToList()`, `ToArray()`, `ToDictionary()`, and aggregate operators are immediate - they force evaluation right then.
- Modifying a `List<T>` while a deferred query over it is being enumerated throws `InvalidOperationException`.
