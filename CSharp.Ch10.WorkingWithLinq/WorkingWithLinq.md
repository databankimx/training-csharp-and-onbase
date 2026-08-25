# Working with LINQ

## Introduction

LINQ (Language Integrated Query) lets you query collections, XML, databases, and more using a consistent, readable syntax built right into C#. This lesson covers both of LINQ's two syntaxes side by side, joins, grouping, aggregate functions, and building XML with LINQ.

---

## Query Syntax vs. Method Syntax

```csharp
// Query syntax, reads like SQL
var dystopianBooks = from b in books
                      where b.Genre == "Dystopian"
                      select b;

// Method syntax, the exact same thing
var dystopianBooks = books.Where(b => b.Genre == "Dystopian");
```

These do exactly the same thing, the compiler turns query syntax into method calls behind the scenes. Query syntax often reads more naturally for filtering, ordering, and joins; method syntax is required for a few operations (`Skip`, `Take`, `Distinct`, `Concat`, and things like `Count`/`Sum`/`Average`) that don't have a query-syntax keyword at all. Worth being comfortable reading both.

---

## Filtering, Ordering, and Projecting

```csharp
var dystopianBooks = books.Where(b => b.Genre == "Dystopian");                    // filter
var ordered = books.OrderBy(b => b.Genre).ThenByDescending(b => b.Year);          // order
var titles = books.Select(b => new { b.Title, b.Year });                          // project
```

Filtering picks which items to keep, ordering controls the sequence, and projecting reshapes each item into something new, often an anonymous type (`new { ... }`) holding just the fields you actually need.

---

## Joining Two Collections

```csharp
// Inner join: only books with a matching author
var booksWithAuthors = books.Join(authors, b => b.AuthorId, a => a.AuthorId, (b, a) => new { b.Title, a.Name });
```

Joins combine two collections based on a matching key, exactly like a SQL join. An *outer* join (every author, even ones with zero books) needs the query-syntax `join ... into` form specifically, method syntax's plain `Join()` is always an inner join.

---

## Grouping and Aggregating

```csharp
var booksByGenre = books.GroupBy(b => b.Genre);

decimal average = books.Average(b => b.Price);
decimal total = books.Sum(b => b.Price);
```

`GroupBy()` buckets items by a key, each group has a `Key` and behaves like its own little collection you can iterate or aggregate further. The aggregate functions (`Count`, `Sum`, `Average`, `Min`, `Max`) collapse a sequence down to one value.

---

## Building XML

```csharp
var xmlBooks = new XElement("Books",
    from b in books
    select new XElement("Book", new XElement("Title", b.Title)));
```

LINQ to XML lets you build an XML document directly out of a query, each `select` produces one `XElement`, and wrapping the whole query in an outer `XElement` nests them all inside it automatically.

---

## Try It Yourself

Run the project and compare `JoinWithQuerySyntax()`'s output against `OuterJoinWithQuerySyntax()`'s, notice "Unpublished Author" only shows up in the second one, with a book count of zero. That's the entire practical difference between an inner and outer join, made visible.
