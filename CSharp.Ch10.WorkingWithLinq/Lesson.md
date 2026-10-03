# Chapter 10: Working With LINQ

## What This Is

LINQ (Language Integrated Query) is really two different syntaxes that compile down to exactly the same thing. Query syntax (`from x in collection where ... select ...`) reads like SQL and is what most people reach for first. Method syntax (`collection.Where(...).Select(...)`) uses ordinary extension methods and chains more naturally with the rest of C#.

The compiler translates query syntax into method syntax. Every query-syntax example below has a matching method-syntax version doing the identical thing - comparing them directly is more instructive than reading either one in isolation.

All examples run against a small in-memory `Author`/`Book` dataset. The fourth author ("Unpublished Author") has zero books, which is deliberate - it exists to make the outer join example show something interesting.

---

## How to Write This Program

Add `Models/Author.cs` and `Models/Book.cs`, then add the shared data helpers to `Program.cs`. Every mini-program calls these to get fresh copies:

```csharp
private static List<Author> GetAuthors()
{
    return
    [
        new Author { AuthorId = 1, Name = "George Orwell",      Country = "United Kingdom" },
        new Author { AuthorId = 2, Name = "Ray Bradbury",       Country = "United States"  },
        new Author { AuthorId = 3, Name = "Aldous Huxley",      Country = "United Kingdom" },
        new Author { AuthorId = 4, Name = "Unpublished Author", Country = "Canada"         }
    ];
}

private static List<Book> GetBooks()
{
    return
    [
        new Book { Title = "1984",                   AuthorId = 1, Year = 1949, Genre = "Dystopian",       Price = 9.99m  },
        new Book { Title = "Animal Farm",            AuthorId = 1, Year = 1945, Genre = "Satire",          Price = 7.99m  },
        new Book { Title = "Fahrenheit 451",         AuthorId = 2, Year = 1953, Genre = "Dystopian",       Price = 8.99m  },
        new Book { Title = "The Martian Chronicles", AuthorId = 2, Year = 1950, Genre = "Science Fiction",  Price = 10.99m },
        new Book { Title = "Brave New World",        AuthorId = 3, Year = 1932, Genre = "Dystopian",       Price = 9.49m  }
    ];
}
```

---

## Part 1: Query Expression Syntax

### Mini-Program 1: Filtering

Clear `Main()` and write:

```csharp
var books = GetBooks();

var dystopianBooks = from b in books
                     where b.Genre == "Dystopian"
                     select b;

Console.WriteLine("Dystopian books (query syntax):");
foreach (var book in dystopianBooks)
    Console.WriteLine($" - {book.Title} ({book.Year})");

GenericFunctions.Pause();
```

Run it. Three books - `1984`, `Fahrenheit 451`, `Brave New World`.

The `from` clause declares the range variable (`b`), the `where` clause filters, the `select` clause projects. When `select b` projects the whole element unchanged, you're still required to write it - query syntax always ends in a `select` or a `group ... by`.

Nothing executes until the `foreach`. The query is an `IEnumerable<Book>` expression sitting in `dystopianBooks`, not a result set. This is deferred execution - `Supplemental.01.DeferredExecution` demonstrates exactly what that means in practice.

### Mini-Program 2: Ordering

Clear `Main()` and write:

```csharp
var books = GetBooks();

var orderedBooks = from b in books
                   orderby b.Genre, b.Year descending
                   select b;

Console.WriteLine("Books ordered by Genre, then Year descending (query syntax):");
foreach (var book in orderedBooks)
    Console.WriteLine($" - {book.Genre}: {book.Title} ({book.Year})");

GenericFunctions.Pause();
```

Run it. Primary sort by genre ascending, secondary sort by year descending within each genre.

Multiple `orderby` keys are comma-separated. The `descending` keyword applies to the immediately preceding key only - `b.Genre` sorts ascending because it has no modifier.

### Mini-Program 3: Projection

Clear `Main()` and write:

```csharp
var books = GetBooks();

var titleAndYear = from b in books
                   select new { b.Title, b.Year };

Console.WriteLine("Title/Year projection (query syntax):");
foreach (var book in titleAndYear)
    Console.WriteLine($" - {book.Title}, {book.Year}");

GenericFunctions.Pause();
```

Run it. Each element in the result is an **anonymous type** with exactly two properties.

`new { b.Title, b.Year }` is property-name inference - the property names come from the source expression. Anonymous types are immutable (get-only properties), sealed, and only comparable by value if you use the compiler-generated `Equals()`.

The important practical consequence: anonymous types cannot cross method boundaries as their static type. You either use `var` (which works within the same scope) or you define a named type. For returning query results from a method, define a real class or record.

### Mini-Program 4: Inner Join

Clear `Main()` and write:

```csharp
var books   = GetBooks();
var authors = GetAuthors();

var booksWithAuthors = from b in books
                       join a in authors on b.AuthorId equals a.AuthorId
                       select new { b.Title, AuthorName = a.Name };

Console.WriteLine("Books joined to their Author (query syntax):");
foreach (var book in booksWithAuthors)
    Console.WriteLine($" - {book.Title} by {book.AuthorName}");

GenericFunctions.Pause();
```

Run it. Five rows - one per book, matched to its author. "Unpublished Author" doesn't appear because an inner join only emits rows where both sides match.

The `equals` keyword is required instead of `==` in a `join` clause - it's a special keyword that tells the compiler which side is the "outer" key (`b.AuthorId`) and which is the "inner" key (`a.AuthorId`), information used for optimization.

### Mini-Program 5: Outer Join

Clear `Main()` and write:

```csharp
var books   = GetBooks();
var authors = GetAuthors();

var authorsWithBookCount = from a in authors
                           join b in books on a.AuthorId equals b.AuthorId into authorBooks
                           select new { a.Name, BookCount = authorBooks.Count() };

Console.WriteLine("Every Author with their book count (query syntax):");
foreach (var author in authorsWithBookCount)
    Console.WriteLine($" - {author.Name}: {author.BookCount} book(s)");

GenericFunctions.Pause();
```

Run it. All four authors appear, including "Unpublished Author" with 0 books.

`join ... into` creates a **group join**: for each `Author`, `authorBooks` is an `IEnumerable<Book>` of all matching books, or an empty sequence if there are none. That empty sequence - rather than a dropped row - is what makes this behave like an outer join. Calling `.Count()` on an empty sequence returns `0`, so every author appears in the output.

### Mini-Program 6: Grouping

Clear `Main()` and write:

```csharp
var books = GetBooks();

var booksByGenre = from b in books
                   group b by b.Genre;

Console.WriteLine("Books grouped by Genre (query syntax):");
foreach (var genreGroup in booksByGenre)
{
    Console.WriteLine($" - {genreGroup.Key} ({genreGroup.Count()}):");
    foreach (var book in genreGroup)
        Console.WriteLine($"     {book.Title}");
}

GenericFunctions.Pause();
```

Run it. Three genre groups: Dystopian (3 books), Satire (1), Science Fiction (1).

`group b by b.Genre` produces an `IEnumerable<IGrouping<string, Book>>`. Each `IGrouping<TKey, TElement>` is both a key (`genreGroup.Key` - the genre string) and a sequence of elements (the books in that genre).

---

## Part 2: Method Syntax

### Mini-Program 7: Where, OrderBy, Select

Clear `Main()` and write:

```csharp
var books = GetBooks();

var filtered  = books.Where(b => b.Genre == "Dystopian");
var ordered   = books.OrderBy(b => b.Genre).ThenByDescending(b => b.Year);
var projected = books.Select(b => new { b.Title, b.Year });

Console.WriteLine("Filtered (method syntax):");
foreach (var b in filtered) Console.WriteLine($" - {b.Title}");

Console.WriteLine("\nOrdered (method syntax):");
foreach (var b in ordered) Console.WriteLine($" - {b.Genre}: {b.Title} ({b.Year})");

Console.WriteLine("\nProjected (method syntax):");
foreach (var b in projected) Console.WriteLine($" - {b.Title}, {b.Year}");

GenericFunctions.Pause();
```

Run it. Identical output to Mini-Programs 1-3.

Method syntax uses ordinary extension methods on `IEnumerable<T>`, chained together. `ThenByDescending` is how multi-key ordering is expressed. Query syntax's `orderby a, b descending` compiles to exactly this chain.

### Mini-Program 8: Join

Clear `Main()` and write:

```csharp
var books   = GetBooks();
var authors = GetAuthors();

var booksWithAuthors = books.Join(
    authors,
    b => b.AuthorId,
    a => a.AuthorId,
    (b, a) => new { b.Title, AuthorName = a.Name });

Console.WriteLine("Books joined to their Author (method syntax):");
foreach (var book in booksWithAuthors)
    Console.WriteLine($" - {book.Title} by {book.AuthorName}");

GenericFunctions.Pause();
```

Run it. Same five rows as Mini-Program 4.

`Join` takes four arguments: the inner sequence, the outer key selector, the inner key selector, and a result selector. Most people find query syntax more readable for joins - the alignment of the key selectors is harder to parse in the four-argument form.

### Mini-Program 9: GroupBy

Clear `Main()` and write:

```csharp
var books = GetBooks();

var booksByGenre = books.GroupBy(b => b.Genre);

Console.WriteLine("Books grouped by Genre (method syntax):");
foreach (var genreGroup in booksByGenre)
{
    Console.WriteLine($" - {genreGroup.Key} ({genreGroup.Count()}):");
    foreach (var book in genreGroup)
        Console.WriteLine($"     {book.Title}");
}

GenericFunctions.Pause();
```

Run it. Same output as Mini-Program 6.

### Mini-Program 10: Aggregate Functions

Clear `Main()` and write:

```csharp
var books = GetBooks();

int count     = books.Count(b => b.Genre == "Dystopian");
decimal sum   = books.Sum(b => b.Price);
decimal avg   = books.Average(b => b.Price);
decimal min   = books.Min(b => b.Price);
decimal max   = books.Max(b => b.Price);

Console.WriteLine($"Dystopian count: {count}");
Console.WriteLine($"Total price:     {sum:C}");
Console.WriteLine($"Average price:   {avg:C}");
Console.WriteLine($"Cheapest:        {min:C}");
Console.WriteLine($"Most expensive:  {max:C}");

GenericFunctions.Pause();
```

Run it. These are **terminal operations** - they force evaluation immediately and return a scalar value, not another `IEnumerable`. `Count()` with a predicate is equivalent to `.Where(predicate).Count()` but slightly more efficient since it avoids materializing the filtered sequence.

### Mini-Program 11: First, Last, FirstOrDefault

Clear `Main()` and write:

```csharp
var books = GetBooks();

var firstDystopian = books.First(b => b.Genre == "Dystopian");
var lastDystopian  = books.Last(b => b.Genre == "Dystopian");
var firstFantasy   = books.FirstOrDefault(b => b.Genre == "Fantasy");

Console.WriteLine($"First Dystopian:  {firstDystopian.Title}");
Console.WriteLine($"Last Dystopian:   {lastDystopian.Title}");
Console.WriteLine($"First Fantasy:    {firstFantasy?.Title ?? "(none found)"}");

GenericFunctions.Pause();
```

Run it. `First()` and `Last()` both throw `InvalidOperationException` when nothing matches. `FirstOrDefault()` / `LastOrDefault()` return `null` instead.

The rule: use `First`/`Last` when absence of a match is a programming error. Use `FirstOrDefault`/`LastOrDefault` when absence is a legitimate, expected outcome.

### Mini-Program 12: Concat, Skip, Take, Distinct

Clear `Main()` and write:

```csharp
var books = GetBooks();

var recentReleases = new List<Book>
{
    new() { Title = "Klara and the Sun", AuthorId = 5, Year = 2021, Genre = "Science Fiction", Price = 14.99m }
};
var allBooks = books.Concat(recentReleases);
Console.WriteLine("Concat:");
foreach (var b in allBooks) Console.WriteLine($" - {b.Title} ({b.Year})");

var alphabetical = books.OrderBy(b => b.Title).ToList();
var secondPage = alphabetical.Skip(2).Take(2);
Console.WriteLine("\nPage 2 (skip 2, take 2), alphabetical:");
foreach (var b in secondPage) Console.WriteLine($" - {b.Title}");

var genres = books.Select(b => b.Genre).Distinct();
Console.WriteLine("\nDistinct genres:");
foreach (var genre in genres) Console.WriteLine($" - {genre}");

GenericFunctions.Pause();
```

Run it.

`Concat` doesn't deduplicate - it just chains. Use `Union` instead if you want distinct elements from both sequences.

`Skip(n).Take(m)` is the standard pagination pattern. `Skip(pageIndex * pageSize).Take(pageSize)` produces any page.

`Distinct()` uses the default equality comparer. For reference types that don't implement `Equals()`/`GetHashCode()`, it falls back to reference equality.

---

## Part 3: LINQ to XML

### Mini-Program 13: Building XML From a Query

Clear `Main()` and write:

```csharp
var books = GetBooks();

var xmlBooks = new XElement("Books",
    from b in books
    select new XElement("Book",
        new XAttribute("year", b.Year),
        new XElement("Title", b.Title),
        new XElement("Genre", b.Genre)));

Console.WriteLine("Books as XML:");
Console.WriteLine(xmlBooks);

GenericFunctions.Pause();
```

Run it. A well-formed XML document printed to the console.

`XElement` and `XAttribute` are the LINQ to XML API. The `XElement` constructor accepts a name and then a `params object[]` of content - other `XElement`s, `XAttribute`s, strings, or sequences of any of these. A LINQ query producing a sequence of `XElement`s can be passed directly as a constructor argument. `Supplemental.02.LinqToXmlDeepDive` goes further into reading, modifying, and querying XML.

---

## Try It Yourself

Run `JoinWithQuerySyntax()` and compare its output against `OuterJoinWithQuerySyntax()`. "Unpublished Author" only shows up in the second one, with a book count of zero. That's the entire practical difference between an inner and outer join, made visible on your own machine.

---

## Summary: Query Syntax vs. Method Syntax

| Operation | Query syntax | Method syntax |
|---|---|---|
| Filter | `where b.Genre == "X"` | `.Where(b => b.Genre == "X")` |
| Order | `orderby b.Genre, b.Year descending` | `.OrderBy(b => b.Genre).ThenByDescending(b => b.Year)` |
| Project | `select new { b.Title }` | `.Select(b => new { b.Title })` |
| Inner join | `join a in authors on b.Id equals a.Id` | `.Join(authors, b => b.Id, a => a.Id, (b,a) => ...)` |
| Outer join | `join b in books ... into group` | `.GroupJoin(...)` |
| Group | `group b by b.Genre` | `.GroupBy(b => b.Genre)` |
| No query syntax | N/A | `Skip`, `Take`, `Distinct`, `Concat`, `Count`, `Sum`, etc. |

---

## Takeaways

- Query syntax and method syntax compile to the same thing. Know both; use whichever is clearer for the case at hand.
- LINQ is deferred - nothing executes until the sequence is enumerated. Terminal operations (`Count`, `ToList`, `First`) force immediate evaluation.
- `select` is mandatory in query syntax. `where`, `orderby`, `join`, and `group` are optional.
- `join ... into` is a group join - the "inner" side is a sequence, not a single element. This is what makes outer joins possible.
- `First`/`Last` throw on no match. `FirstOrDefault`/`LastOrDefault` return the default.
- Anonymous types infer property names from the source expression, are immutable, and can't cross method boundaries as their static type.
- `Distinct` uses the default equality comparer.
- LINQ to XML accepts query results directly in `XElement` constructors.

---

## Also in Chapter 10

Four supplemental projects accompany this one:

1. `CSharp.Ch10.Supplemental.01.DeferredExecution`
2. `CSharp.Ch10.Supplemental.02.LinqToXmlDeepDive`
3. `CSharp.Ch10.Supplemental.03.CustomLinqExtensionMethods`
4. `CSharp.Ch10.Supplemental.04.IQueryableVsIEnumerable`
