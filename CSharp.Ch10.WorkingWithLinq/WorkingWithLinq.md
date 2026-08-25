# Working with LINQ (Language Integrated Query)

## Introduction

LINQ lets you write queries directly in C#, against collections, XML, databases, all using the same consistent syntax. This lesson works through both of LINQ's syntaxes, joins, grouping, and every aggregate function, using a small catalog of books and their authors.

---

## Two Syntaxes, One Technology

```csharp
// Query syntax
var dystopian = from b in books where b.Genre == "Dystopian" select b;

// Method syntax
var dystopian = books.Where(b => b.Genre == "Dystopian");
```

These do exactly the same thing, query syntax actually *compiles into* method syntax under the hood. Query syntax often reads more naturally for anything with several steps chained together; method syntax is required for things query syntax simply has no keyword for, `Skip()`, `Take()`, `Distinct()`, and most aggregate functions.

---

## Filtering, Ordering, and Projection

```csharp
var recent = from b in books where b.Year > 1950 select b;                  // filter
var byYear = from b in books orderby b.Year descending select b;            // order
var titles = from b in books select b.Title;                                 // project
```

"Projection" just means shaping the output, picking specific fields, or building something new (like an anonymous type: `select new { b.Title, b.Year }`) instead of returning the whole original object.

---

## Joining Two Collections

```csharp
// Inner join: only books that HAVE a matching author
var withAuthors = from b in books
                   join a in authors on b.AuthorId equals a.AuthorId
                   select new { b.Title, a.Name };
```

An inner join drops anything without a match on both sides. If you want to *keep* unmatched rows too (an "outer join," or specifically a "left join" here), LINQ needs a slightly different shape:

```csharp
var withAuthors = from b in books
                   join a in authors on b.AuthorId equals a.AuthorId into bookAuthors
                   from author in bookAuthors.DefaultIfEmpty(new Author { Name = "(unknown)" })
                   select new { b.Title, author.Name };
```

`into` groups the matches instead of flattening immediately, and `DefaultIfEmpty()` fills in a placeholder when there's nothing to match, rather than just dropping the row entirely.

---

## Grouping

```csharp
var byGenre = from b in books group b by b.Genre;
// each item in byGenre is a group: .Key is the genre, and you can foreach over its members

var summary = from b in books
              group b by b.Genre into g
              select new { Genre = g.Key, AveragePrice = g.Average(b => b.Price) };
```

`group ... into` lets you keep querying *after* the grouping, here collapsing each group down to just its name and an average.

---

## Aggregate Functions and Everything Method-Syntax-Only

```csharp
int count = books.Count();
decimal average = books.Average(b => b.Price);
var firstBook = books.OrderBy(b => b.Year).First();
var page2 = books.OrderBy(b => b.Year).Skip(2).Take(2);   // pagination
var genres = books.Select(b => b.Genre).Distinct();
```

None of these have a query-syntax keyword, method syntax is the only way to write them. `Skip().Take()` is the standard pattern for pagination: skip past earlier pages, then take just the current page's worth.

---

## LINQ to XML: Same Syntax, Works on XML Too

```csharp
var xml = new XElement("Catalog",
    from b in books
    select new XElement("Book", new XElement("Title", b.Title)));
```

You can build XML directly from a query, and, just as usefully, query data back *out* of XML using the exact same `from`/`where`/`select` syntax, no separate XML query language to learn.

---

## Try It Yourself

Add a new author and a new book referencing them, then run the project again, every query (filtering, ordering, the joins, the groupings) should reflect the new data automatically, no other code changes needed. Then try removing an `AuthorId` from an existing book and compare how the inner join and outer join demos each handle it differently.
