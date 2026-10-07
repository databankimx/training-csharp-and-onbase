---
title: "Query Syntax - Filter, Order, Project, Join, Group"
chapter: 10
index: 1
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public class Author { public int AuthorId { get; set; } public string Name { get; set; } public string Country { get; set; } }
public class Book   { public string Title { get; set; } public int AuthorId { get; set; } public int Year { get; set; } public string Genre { get; set; } public decimal Price { get; set; } }

internal static class Program
{
    private static List<Author> GetAuthors() =>
    [
        new() { AuthorId = 1, Name = "George Orwell",      Country = "United Kingdom" },
        new() { AuthorId = 2, Name = "Ray Bradbury",       Country = "United States"  },
        new() { AuthorId = 3, Name = "Aldous Huxley",      Country = "United Kingdom" },
        new() { AuthorId = 4, Name = "Unpublished Author", Country = "Canada"         }
    ];

    private static List<Book> GetBooks() =>
    [
        new() { Title = "1984",                   AuthorId = 1, Year = 1949, Genre = "Dystopian",       Price = 9.99m  },
        new() { Title = "Animal Farm",            AuthorId = 1, Year = 1945, Genre = "Satire",          Price = 7.99m  },
        new() { Title = "Fahrenheit 451",         AuthorId = 2, Year = 1953, Genre = "Dystopian",       Price = 8.99m  },
        new() { Title = "The Martian Chronicles", AuthorId = 2, Year = 1950, Genre = "Science Fiction", Price = 10.99m },
        new() { Title = "Brave New World",        AuthorId = 3, Year = 1932, Genre = "Dystopian",       Price = 9.49m  }
    ];

    private static void Main()
    {
        var books   = GetBooks();
        var authors = GetAuthors();

        // --- Filter ---
        var dystopian = from b in books where b.Genre == "Dystopian" select b;
        Console.WriteLine("Dystopian books:");
        foreach (var b in dystopian) Console.WriteLine($"  {b.Title} ({b.Year})");

        // --- Order: primary ascending, secondary descending ---
        var ordered = from b in books orderby b.Genre, b.Year descending select b;
        Console.WriteLine("\nBy Genre then Year desc:");
        foreach (var b in ordered) Console.WriteLine($"  {b.Genre}: {b.Title} ({b.Year})");

        // --- Project into an anonymous type ---
        // Anonymous types infer property names from the source expression, are immutable,
        // and cannot be returned from a method as their static type. Use var within scope.
        var projected = from b in books select new { b.Title, b.Year };
        Console.WriteLine("\nTitle/Year projection:");
        foreach (var b in projected) Console.WriteLine($"  {b.Title}, {b.Year}");

        // --- Inner join: only books with a matching author; "Unpublished Author" is absent ---
        // 'equals' is required (not ==) -- it signals which side is outer vs. inner key.
        var joined = from b in books
                     join a in authors on b.AuthorId equals a.AuthorId
                     select new { b.Title, AuthorName = a.Name };
        Console.WriteLine("\nInner join (no Unpublished Author):");
        foreach (var b in joined) Console.WriteLine($"  {b.Title} by {b.AuthorName}");

        // --- Outer join (group join): every author, even those with zero books ---
        // 'join ... into' gives each author an IEnumerable<Book> of their books (empty if none).
        var outer = from a in authors
                    join b in books on a.AuthorId equals b.AuthorId into authorBooks
                    select new { a.Name, Count = authorBooks.Count() };
        Console.WriteLine("\nOuter join (Unpublished Author appears with 0):");
        foreach (var a in outer) Console.WriteLine($"  {a.Name}: {a.Count} book(s)");

        // --- Group ---
        // group b by b.Genre produces IEnumerable<IGrouping<string, Book>>
        var grouped = from b in books group b by b.Genre;
        Console.WriteLine("\nGrouped by Genre:");
        foreach (var g in grouped)
        {
            Console.WriteLine($"  {g.Key} ({g.Count()}):");
            foreach (var b in g) Console.WriteLine($"    {b.Title}");
        }

        // Nothing above executed until its foreach -- that's deferred execution.
        // Supplemental 01 demonstrates what that means in practice.
    }
}
```
