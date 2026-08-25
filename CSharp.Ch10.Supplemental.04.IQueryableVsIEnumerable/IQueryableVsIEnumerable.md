# IQueryable vs. IEnumerable

## Introduction

Every LINQ query so far in this chapter ran against in-memory lists. This lesson shows what changes when the source is a database instead, using Entity Framework against the same database from the ADO.NET lesson. **Needs that lesson's database setup done first.**

---

## Same Syntax, Very Different Things Happening

```csharp
list.Where(x => x.Something);       // IEnumerable<T>: runs right here, in C#
dbSet.Where(x => x.Something);      // IQueryable<T>: translated into SQL, runs on the server
```

Both lines look identical. But `Where()` on a `List<T>` takes an ordinary compiled function and just calls it. `Where()` on an Entity Framework `DbSet<T>` takes something different, a description of the lambda's logic that Entity Framework reads and turns into an actual SQL query, which then runs on the database server, not in your program at all.

---

## You Can See the Actual SQL

```csharp
var query = db.MurphysLaws.Where(law => law.LawText.Length < 60);
Console.WriteLine(query.ToString());
```

Calling `.ToString()` on this kind of query prints the real SQL Entity Framework generated from your C#. Genuinely useful for understanding (or debugging) what your LINQ code is actually asking the database to do.

---

## Not Everything Can Become SQL

```csharp
db.MurphysLaws.Where(law => IsPalindrome(law.LawName));   // throws!
```

If you use a plain C# method inside a query against the database, and that method isn't something Entity Framework knows how to translate into SQL, it throws an error the moment the query runs. Entity Framework can only translate a limited, known set of patterns, comparisons, math, a handful of recognized string methods, not arbitrary code.

---

## The Fix: `.AsEnumerable()`

```csharp
var results = db.MurphysLaws
    .Where(law => law.LawText.Length < 60)     // still runs on the database
    .AsEnumerable()                            // switch to plain C# from here on
    .Where(law => IsPalindrome(law.LawName));  // now this works fine
```

`.AsEnumerable()` marks the point where a query stops being translated to SQL and starts running as ordinary C#. Everything before it still happens on the database (fast, filters a lot of data efficiently); everything after it runs locally, letting you use whatever C# logic you need, even things SQL has no way to express.

---

## Try It Yourself

Run `UntranslatableExpressionThrows()` and read the exception message closely, then compare it against `AsEnumerableSwitchesToClientSideEvaluation()`, which uses the exact same `IsPalindrome()` check but doesn't throw, because it's positioned after `.AsEnumerable()` instead of directly against the database query.
