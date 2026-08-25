# Deferred Execution

## Introduction

Most LINQ queries don't run when you write them, they run when you actually use their results. This is called deferred execution, and understanding it helps you avoid two genuinely common bugs.

---

## Queries Are Recipes, Not Results

```csharp
var evenNumbers = numbers.Where(n => n % 2 == 0);
```

This line doesn't filter anything yet. It's a description of work to do later, the actual filtering happens the moment something enumerates `evenNumbers`, a `foreach` loop, `.ToList()`, or similar.

---

## Why This Matters: Two Real Consequences

**1. The query sees whatever the source looks like when it's finally enumerated, not when it was written:**

```csharp
var evenNumbers = numbers.Where(n => n % 2 == 0);
numbers.Add(6);
foreach (int n in evenNumbers) { ... }   // 6 shows up!
```

**2. Enumerating the same query twice does the work twice:**

```csharp
foreach (int n in evenNumbers) { ... }   // does the filtering
foreach (int n in evenNumbers) { ... }   // does it AGAIN, from scratch
```

If the source is expensive to work with (a database call, a slow calculation), this means doing that expensive thing twice, not once, easy to miss if you're not aware of it.

---

## The Fix: `.ToList()` Takes a Real Snapshot

```csharp
var snapshot = numbers.Where(n => n % 2 == 0).ToList();
```

`.ToList()` (or `.ToArray()`) forces the query to run immediately and stores the actual results. From that point on, `snapshot` is just an ordinary list, no longer connected to the original query at all, immune to later changes and safe to loop over as many times as you want.

---

## A Bug Worth Recognizing: Modifying a List While Looping Over It

```csharp
foreach (int n in numbers.Where(n => n % 2 == 0))
{
    if (n == 2) numbers.Add(100);   // throws!
}
```

Adding to (or removing from) a list while you're still looping over it throws an exception, the list detects it's being changed underneath the loop and refuses to continue rather than risk giving you wrong results. If you need to change a collection while looping over it, either call `.ToList()` first to get an independent snapshot, or collect what you want to change into a separate list and apply those changes after the loop is done.

---

## Try It Yourself

Run `MultipleEnumerationReRunsTheQuery()` and watch the `(evaluating ...)` lines print twice, once per loop. That's the entire concept, made visible.
