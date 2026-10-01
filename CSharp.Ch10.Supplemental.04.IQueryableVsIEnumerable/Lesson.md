# Chapter 10 Supplemental 04: IQueryable vs IEnumerable

## What This Is

Every LINQ query throughout this chapter has actually been one of two genuinely different things, even though they're written with the exact same syntax.

**`IEnumerable<T>` (LINQ to Objects):** A `Where()` clause takes a `Func<T, bool>` -- an ordinary, already-compiled C# delegate. Filtering happens in this process, one item at a time, by literally calling that delegate for each element.

**`IQueryable<T>` (LINQ to Entities, when the source is an EF `DbSet<T>`):** A `Where()` clause takes an `Expression<Func<T, bool>>` instead -- not a compiled delegate, but a data structure describing the lambda's logic. EF walks that data structure and translates it into SQL, which then runs on the database server, not in this process at all.

This project requires the same database setup as `Supplemental.01.AdoNetAndEntityFramework`. See that project's `README.md` if you haven't restored `ExternalData.bak` yet.

---

## How to Write This Program

The `ExternalDataContext` and `MurphysLaw` model are already in the project, same as `Supplemental.01`.

### Mini-Program 1: IEnumerable -- Filtering In This Process

Clear `Main()` and write:

```csharp
using var db = new ExternalDataContext();

// .ToList() materializes every row from the database RIGHT NOW.
// The result is a real, in-memory List<MurphysLaw> -- an IEnumerable<T>.
var allLaws = db.MurphysLaws.ToList();

// This Where() takes a Func<MurphysLaw, bool> -- an ordinary compiled delegate.
// Filtering happens HERE, in this process, one object at a time.
// No SQL is involved in this line. The data already left the database.
var shortLaws = allLaws.Where(law => law.LawText.Length < 60);

Console.WriteLine("Laws with short text, filtered client-side (IEnumerable<T>):");
foreach (var law in shortLaws)
    Console.WriteLine($" - {law.LawName}");

GenericFunctions.Pause();
```

Run it. Every row was fetched from the database, then filtered in memory.

That `ToList()` is doing real work: it fetches the entire `MurphysLaws` table into memory. The subsequent `Where()` is ordinary C# executing against a `List<MurphysLaw>`. Convenient, but not scalable -- every row leaves the database even though most will be discarded.

### Mini-Program 2: IQueryable -- Filtering on the Server

Clear `Main()` and write:

```csharp
using var db = new ExternalDataContext();

// No .ToList(). db.MurphysLaws is IQueryable<MurphysLaw>.
// This Where() takes an Expression<Func<MurphysLaw, bool>> -- a data structure.
// Nothing has hit the database yet.
var shortLaws = db.MurphysLaws.Where(law => law.LawText.Length < 60);

// On an EF IQueryable<T>, .ToString() prints the actual SQL EF generated.
Console.WriteLine("SQL EF generated from the C# expression:");
Console.WriteLine(shortLaws.ToString());

// The query executes HERE, when enumeration starts.
Console.WriteLine($"\nResults (filtered server-side, IQueryable<T>):");
foreach (var law in shortLaws)
    Console.WriteLine($" - {law.LawName}");

GenericFunctions.Pause();
```

Run it. Read the generated SQL -- `LawText.Length < 60` became a `LEN()` check in the SQL. Only the matching rows came across the network. The rest never left the server.

That's the difference. `IQueryable<T>` doesn't just translate the operator (`Where`); it translates the entire lambda expression tree into SQL. `law.LawText.Length` became `LEN(LawText)`. `< 60` became `< 60` in the SQL `WHERE` clause. EF understood the semantics of the C# expression and found an equivalent in SQL.

### Mini-Program 3: An Untranslatable Expression Throws

Clear `Main()` and write:

```csharp
// A perfectly ordinary C# method -- but EF has no way to turn it into SQL.
static bool IsPalindrome(string text)
{
    if (string.IsNullOrEmpty(text)) return false;
    string normalized = new string(text.Where(char.IsLetter).ToArray()).ToLowerInvariant();
    return normalized == new string(normalized.Reverse().ToArray());
}

using var db = new ExternalDataContext();

try
{
    // IQueryable<T>: EF tries to translate IsPalindrome() into SQL and fails.
    var palindromicLaws = db.MurphysLaws
        .Where(law => IsPalindrome(law.LawName))
        .ToList();

    Console.WriteLine($"Found {palindromicLaws.Count} palindromic laws (this line should not print).");
}
catch (NotSupportedException ex)
{
    Console.WriteLine("Threw NotSupportedException, as expected:");
    Console.WriteLine($" - {ex.Message}");
}

GenericFunctions.Pause();
```

Run it. `NotSupportedException` with a message about being unable to translate the expression.

EF can only translate a known set of patterns: comparisons, arithmetic, string methods it specifically recognizes (`Contains()`, `StartsWith()`, `EndsWith()`), and so on. An arbitrary C# method like `IsPalindrome()` is not in that set. EF6 refuses outright -- some newer ORMs fall back to evaluating unsupported expressions client-side with a warning, which can be worse: silently fetching the entire table and filtering in memory.

### Mini-Program 4: AsEnumerable() -- Switching to Client-Side Partway Through

Clear `Main()` and write:

```csharp
using var db = new ExternalDataContext();

// Everything BEFORE .AsEnumerable() is IQueryable<T> -- translated to SQL, runs on server.
// .AsEnumerable() is the switch point -- after it, operators are IEnumerable<T> in-process C#.
var results = db.MurphysLaws
    .Where(law => law.LawText.Length < 60)  // translated to SQL, runs on the server
    .AsEnumerable()                         // switch point
    .Where(law => IsPalindrome(law.LawName)); // ordinary C#, runs in this process

Console.WriteLine("Short laws (server-side) with palindromic names (client-side):");
foreach (var law in results)
    Console.WriteLine($" - {law.LawName}");

Console.WriteLine("\n(Probably zero results with this dataset -- the point is it runs at all,");
Console.WriteLine("unlike Mini-Program 3 above.)");
GenericFunctions.Pause();
```

Run it. No `NotSupportedException` this time.

`.AsEnumerable()` forces the query built so far to execute on the server and converts the result into an `IEnumerable<T>`. From that point on, all further operators are ordinary in-memory LINQ. The server did the `LawText.Length < 60` filter; your process does the palindrome check on whatever rows came back.

This is the standard fix when you need a mix of server-side and client-side filtering: push as much as possible to the server first (to limit the data transferred), then call `.AsEnumerable()` and continue with any logic EF can't translate.

The order matters enormously. If you put the untranslatable filter first on `IQueryable<T>`, EF throws. If you switch to `IEnumerable<T>` first and then apply both filters client-side, the entire table comes across the network. The correct pattern is server-translatable filters first, `.AsEnumerable()`, then client-only filters.

---

## Takeaways

- `IEnumerable<T>` LINQ takes a compiled `Func<T, bool>` -- filtering runs in this process.
- `IQueryable<T>` LINQ takes an `Expression<Func<T, bool>>` -- filtering is translated to SQL and runs on the server.
- The same syntax, written against different sources, is doing fundamentally different things.
- EF can only translate a known set of expression patterns. An arbitrary C# method throws `NotSupportedException`.
- `.ToString()` on an EF `IQueryable<T>` prints the generated SQL -- useful for understanding and debugging.
- `.AsEnumerable()` is the deliberate switch from server-side to client-side evaluation.
- Put server-translatable filters before `.AsEnumerable()` to limit the data transferred. Client-only filters go after.
