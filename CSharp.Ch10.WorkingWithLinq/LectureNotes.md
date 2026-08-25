# Chapter 10: Working with Language Integrated Query (LINQ)

## What This Lesson Is

LINQ lets you write SQL-like queries directly in C#, against almost any data source, in-memory collections, XML, databases, using one consistent syntax. This lesson covers both of LINQ's two equivalent syntaxes (query and method), joins (inner and outer), grouping, every aggregate function the chapter calls out, and LINQ to XML, all demonstrated against a small `Book`/`Author` catalog built specifically to give every operation something real to work with, including a book with no matched author, for the outer join demonstrations.

---

## Query Syntax vs. Method Syntax: The Same Thing, Written Two Ways

```csharp
// Query syntax
var dystopian = from b in books where b.Genre == "Dystopian" select b;

// Method syntax
var dystopian = books.Where(b => b.Genre == "Dystopian");
```

These aren't two different technologies. Every query syntax expression is compiled by the C# compiler into the exact same method syntax chain, `from`/`where`/`select` are pure syntactic sugar over `Where()`/`Select()` and friends. Query syntax tends to read more clearly for anything with several clauses chained together, especially joins and groupings; method syntax is required for anything query syntax has no keyword for at all, `Skip()`, `Take()`, `Distinct()`, `Concat()`, and nearly every aggregate function (`Count()`, `Sum()`, `Average()`, `Min()`, `Max()`).

---

## Joins: Inner vs. Outer, and Why the Sample Data Has a Gap

```csharp
// Inner join (query syntax)
var booksWithAuthors = from b in books
                        join a in authors on b.AuthorId equals a.AuthorId
                        select new { b.Title, a.Name };
```

`Book.AuthorId` is deliberately nullable, and one book (`"Unattributed Anthology"`) deliberately has no author at all, purely so the outer join demonstration has something genuine to show. An inner join only returns rows with a match on *both* sides, that unattributed book simply never appears in `QueryInnerJoin()`'s output.

```csharp
// Outer join (query syntax): "join ... into" + DefaultIfEmpty()
var booksWithAuthors = from b in books
                        join a in authors on b.AuthorId equals a.AuthorId into bookAuthors
                        from author in bookAuthors.DefaultIfEmpty(new Author { Name = "(unknown)" })
                        select new { b.Title, author.Name };
```

LINQ has no dedicated `LEFT JOIN` keyword. The pattern that produces the same result: `join ... into` groups the matches (zero or one `Author` per `Book` here) into a nested collection instead of flattening immediately, then a second `from` clause flattens it back out, `DefaultIfEmpty()` substitutes a placeholder when that nested group is empty rather than dropping the row. Compare this against `MethodJoining()`'s method-syntax equivalent (`GroupJoin()` + `SelectMany()`), the exact same two-step shape, just expressed as method calls instead.

---

## `group ... into`: Continuing a Query Past a Grouping

```csharp
var genreSummary = from b in books
                    group b by b.Genre into g
                    select new { Genre = g.Key, AveragePrice = g.Average(b => b.Price) };
```

Plain `group b by b.Genre` (without `into`) produces the groups themselves as the query's final result, useful when you want to enumerate each group's members directly (see `QueryGrouping()`'s first example). Adding `into g` lets the query keep going *past* the grouping step, here, collapsing each group down to just its key and one aggregate value computed over its members. Worth comparing both forms side by side in `QueryGrouping()`, same underlying grouping, two different things done with the result afterward.

---

## Aggregate Functions, `First()`/`Last()`, `Skip()`/`Take()`, `Distinct()`

All method-syntax only, no query syntax equivalent exists for any of these. `Skip(2).Take(2)` in particular (`SkipAndTake()`) is worth recognizing as the standard building block for pagination, "skip past however many items belong to earlier pages, then take just this page's worth." `Distinct()` compares by default equality (for `string` genres here, that's straightforward value equality; for a custom reference type without an `Equals()`/`GetHashCode()` override, `Distinct()` would compare by reference instead, worth keeping in mind if you ever reach for it against your own classes).

---

## LINQ to XML: The Same Query Syntax, Both Directions

```csharp
var xmlCatalog = new XElement("Catalog",
    from b in books
    select new XElement("Book", new XAttribute("id", b.BookId), new XElement("Title", b.Title), ...));
```

Building XML directly from a LINQ query's results, `XElement`'s constructor accepts an `IEnumerable<XElement>` and flattens it in as child elements automatically. Worth noticing the second half of `LinqToXml()`: querying data back *out* of the XML uses the identical `from`/`where`/`select` syntax used everywhere else in this lesson, `XElement`/`XDocument` implement `IEnumerable<T>` too (via `.Elements()`), so there's no separate "XML query language" to learn, the same LINQ you already know works here directly.
