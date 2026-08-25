# Chapter 10 Supplemental 04: `IQueryable<T>` vs. `IEnumerable<T>`

## What This Is

Every LINQ query in this chapter's main lesson and Supplementals 01-03 was `IEnumerable<T>`, "LINQ to Objects," running entirely against in-memory data. This Supplemental introduces `IQueryable<T>`, what actually happens when the source of a LINQ query is a database (via Entity Framework) instead of a `List<T>` or array, using the same `ExternalData`/`MurphysLaws` database from `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework`. **Needs that project's database setup done first**, see its `README.md`.

---

## The Real Difference: `Func<T, bool>` vs. `Expression<Func<T, bool>>`

Both `IEnumerable<T>.Where()` and `IQueryable<T>.Where()` are written identically in C#, `.Where(x => x.Something)`, but they take genuinely different parameter types under the hood:

- `IEnumerable<T>.Where()` takes a **`Func<T, bool>`**, an ordinary, already-compiled delegate. Calling it just... calls it, in this process, for each element.
- `IQueryable<T>.Where()` takes an **`Expression<Func<T, bool>>`**, not a compiled delegate at all, a *data structure* (an "expression tree") describing the lambda's logic: which property is being compared, with what operator, against what value. The LINQ provider (Entity Framework, here) walks that data structure and generates something else entirely from it, SQL, in this case, which then runs on a different machine altogether.

---

## Seeing the Actual SQL

```csharp
var shortLaws = db.MurphysLaws.Where(law => law.LawText.Length < 60);
Console.WriteLine(shortLaws.ToString());
```

Worth trying directly: calling `.ToString()` on an EF `IQueryable<T>` prints the *actual generated SQL command text*, not a debugging aid tacked on for this lesson, a real, useful EF6 feature. Read it side by side with the C# that produced it, `.LawText.Length < 60` becomes a `LEN(...) < 60`-style SQL condition. This is the translation the expression tree made possible, in concrete, readable form.

---

## The Real Limitation: Not Everything Translates

```csharp
var palindromicLaws = db.MurphysLaws.Where(law => IsPalindrome(law.LawName)).ToList();
// throws NotSupportedException
```

`IsPalindrome()` is an ordinary, perfectly valid C# method. EF has no way to turn an arbitrary method call into SQL, it can only translate a known, finite set of patterns: comparisons, arithmetic, a specific list of recognized string/date methods (`Contains()`, `StartsWith()`, and similar), and so on. `IsPalindrome()` isn't on that list, so building an expression tree containing a call to it and asking EF to translate it throws `NotSupportedException` the moment the query actually executes. Worth knowing specifically because EF6 refuses outright here, some newer ORMs instead silently fall back to evaluating an untranslatable piece client-side (with a warning), a behavior difference worth being aware of if you work across different ORM generations.

---

## `.AsEnumerable()`: Deliberately Switching Contexts Mid-Query

```csharp
var results = db.MurphysLaws
    .Where(law => law.LawText.Length < 60)    // IQueryable<T>, translated to SQL, server-side
    .AsEnumerable()                           // switch point
    .Where(law => IsPalindrome(law.LawName)); // IEnumerable<T>, ordinary C#, client-side
```

`.AsEnumerable()` converts an `IQueryable<T>` into a plain `IEnumerable<T>` without materializing anything itself (no `.ToList()`-style eager fetch), it's a type-level switch, not a data-fetching operation on its own. Everything in the chain *before* `.AsEnumerable()` is still `IQueryable<T>`, translated to SQL, filtered by the database server. Everything *after* it runs as ordinary LINQ to Objects, in this process, against whatever rows the server-side filter already returned. This is the correct, deliberate way to combine "let the database do what it's good at" (filtering large amounts of data efficiently) with "then do something in C# that SQL genuinely can't express" (like `IsPalindrome()`), rather than either giving up entirely (`UntranslatableExpressionThrows()`) or pulling the *entire* table into memory before filtering anything at all.

---

## The General Lesson

Worth carrying forward past this specific example: whenever you're working with an ORM (Entity Framework here, but this applies broadly), know which parts of your query are `IQueryable<T>` (translated, running on the server) versus `IEnumerable<T>` (running locally). Where that boundary sits determines both what's even possible (only translatable expressions work against `IQueryable<T>`) and how much data crosses the wire (filtering server-side, before the boundary, sends less data than filtering client-side, after pulling everything across first).
