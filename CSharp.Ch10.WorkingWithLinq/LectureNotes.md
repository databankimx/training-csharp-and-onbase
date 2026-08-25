# Chapter 10: Working with Language Integrated Query (LINQ)

## What This Is

LINQ (Language Integrated Query) covered as the textbook structures it: query expression syntax, the equivalent method-based syntax, and LINQ to XML. Every query-syntax example has a matching method-syntax example doing the exact same thing, side by side, since that's genuinely what the compiler does with query syntax anyway, translates it into method calls before anything else happens.

---

## Two Syntaxes, One Compiler Output

```csharp
// Query syntax
var dystopianBooks = from b in books
                      where b.Genre == "Dystopian"
                      select b;

// Method syntax
var dystopianBooks = books.Where(b => b.Genre == "Dystopian");
```

These produce identical IL. Query syntax reads close to SQL and tends to read more naturally for anything with a `join` or multiple `where`/`orderby` clauses; method syntax chains more naturally with everything else in C# (a `.Where()` you can tack directly onto a method call's return value, for instance) and is required for a handful of operations query syntax has no keyword for at all (`Skip()`, `Take()`, `Distinct()`, `Concat()`, and the aggregate functions all only exist as methods). Most real code ends up mixing both, worth being fluent in reading either.

---

## Joining: Inner vs. Outer

```csharp
// Inner join, authors with no matching books are silently dropped
var booksWithAuthors = from b in books
                        join a in authors on b.AuthorId equals a.AuthorId
                        select new { b.Title, AuthorName = a.Name };

// Outer join, every author appears, even with zero books
var authorsWithBookCount = from a in authors
                            join b in books on a.AuthorId equals b.AuthorId into authorBooks
                            select new { a.Name, BookCount = authorBooks.Count() };
```

The `join ... into` clause creates what's called a *group join*, each outer element (`a`) paired with a *collection* of matching inner elements (`authorBooks`), rather than one row per match. That alone is still effectively an inner join in spirit, an author with zero matches gets an empty group, not a dropped row, but once you're projecting off of `authorBooks` directly (`.Count()`, or iterating it), every author shows up in the output regardless of match count, which is what makes it behave like a genuine outer join. Worth testing directly against this lesson's sample data: `authorsWithBookCount` includes "Unpublished Author" with `BookCount: 0`, `booksWithAuthors`' plain inner join would never mention that author at all.

---

## Aggregate Functions, First/Last, and Their `OrDefault` Counterparts

```csharp
var firstDystopian = books.First(b => b.Genre == "Dystopian");           // throws if none match
var firstFantasy = books.FirstOrDefault(b => b.Genre == "Fantasy");      // returns null if none match
```

`First()`/`Last()`/`Single()` all throw `InvalidOperationException` when nothing matches; their `OrDefault()` counterparts return the type's default (`null` for reference types, `0`/`false`/etc. for value types) instead. Worth choosing deliberately: use the throwing version when a missing match genuinely indicates something has gone wrong and should surface loudly, use the `OrDefault()` version when "nothing found" is an expected, ordinary outcome your code already handles (as `FirstAndLast()` does here, checking `firstFantasy?.Title ?? "(none found)"`).

---

## `Concat()`, `Skip()`/`Take()`, and `Distinct()`: Method-Only Operations

```csharp
var allBooks = books.Concat(recentReleases);        // combine two sequences end to end
var secondPage = books.Skip(2).Take(2);              // classic pagination pattern
var genres = books.Select(b => b.Genre).Distinct();  // unique values only
```

None of these three have query-syntax keywords, they're method-syntax only, one of the concrete reasons fluency in both syntaxes matters rather than picking one and never touching the other. `Skip()`/`Take()` together are worth recognizing as the standard building block for pagination: skip past however many earlier "pages" you've already shown, then take exactly one page's worth.

---

## LINQ to XML

```csharp
var xmlBooks = new XElement("Books",
    from b in books
    select new XElement("Book",
        new XAttribute("year", b.Year),
        new XElement("Title", b.Title),
        new XElement("Genre", b.Genre)));
```

`System.Xml.Linq`'s `XElement`/`XAttribute` construct XML using ordinary object construction syntax, and a LINQ query can be embedded directly inside that construction, exactly as shown here, since a query expression is just a value (an `IEnumerable<XElement>` in this case) like any other. Worth noticing this is the same fundamental idea as building an anonymous-type projection elsewhere in this lesson, `select new { ... }` and `select new XElement(...)` are the same LINQ mechanism, just projecting into a different kind of object.
