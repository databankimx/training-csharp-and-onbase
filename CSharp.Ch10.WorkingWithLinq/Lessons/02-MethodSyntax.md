---
title: "Method Syntax - Where, OrderBy, Join, Aggregates, Pagination"
chapter: 10
index: 2
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class Author { public int AuthorId { get; set; } public string Name { get; set; } }
public class Book   { public string Title { get; set; } public int AuthorId { get; set; } public int Year { get; set; } public string Genre { get; set; } public decimal Price { get; set; } }

internal static class Program
{
    private static List<Author> GetAuthors() =>
    [
        new() { AuthorId = 1, Name = "George Orwell"      },
        new() { AuthorId = 2, Name = "Ray Bradbury"       },
        new() { AuthorId = 3, Name = "Aldous Huxley"      },
        new() { AuthorId = 4, Name = "Unpublished Author" }
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

        // --- Filter, Order, Project (identical output to query syntax step) ---
        Console.WriteLine("Where + OrderBy + ThenByDescending:");
        foreach (var b in books.Where(b => b.Genre == "Dystopian"))
            Console.WriteLine($"  {b.Title}");

        Console.WriteLine("\nOrderBy + ThenByDescending:");
        foreach (var b in books.OrderBy(b => b.Genre).ThenByDescending(b => b.Year))
            Console.WriteLine($"  {b.Genre}: {b.Title} ({b.Year})");

        // --- Join: four-argument form -- most people prefer query syntax for joins ---
        Console.WriteLine("\nJoin:");
        foreach (var b in books.Join(authors, b => b.AuthorId, a => a.AuthorId,
                                     (b, a) => new { b.Title, AuthorName = a.Name }))
            Console.WriteLine($"  {b.Title} by {b.AuthorName}");

        // --- GroupBy ---
        Console.WriteLine("\nGroupBy:");
        foreach (var g in books.GroupBy(b => b.Genre))
        {
            Console.WriteLine($"  {g.Key} ({g.Count()}):");
            foreach (var b in g) Console.WriteLine($"    {b.Title}");
        }

        // --- Aggregates: terminal operations -- force evaluation immediately ---
        Console.WriteLine($"\nCount(Dystopian): {books.Count(b => b.Genre == "Dystopian")}");
        Console.WriteLine($"Sum(Price):       {books.Sum(b => b.Price):C}");
        Console.WriteLine($"Average(Price):   {books.Average(b => b.Price):C}");
        Console.WriteLine($"Min(Price):       {books.Min(b => b.Price):C}");
        Console.WriteLine($"Max(Price):       {books.Max(b => b.Price):C}");

        // First/Last throw on no match; FirstOrDefault/LastOrDefault return null.
        // Rule: use First when absence is a bug; FirstOrDefault when it's expected.
        Console.WriteLine($"\nFirst Dystopian:     {books.First(b => b.Genre == "Dystopian").Title}");
        Console.WriteLine($"Last Dystopian:      {books.Last(b => b.Genre == "Dystopian").Title}");
        Console.WriteLine($"FirstOrDefault(Fantasy): {books.FirstOrDefault(b => b.Genre == "Fantasy")?.Title ?? "(none)"}");

        // --- Concat, Distinct, Skip/Take ---
        var extra = new List<Book> { new() { Title = "Dune", Year = 1965, Genre = "Science Fiction" } };
        Console.WriteLine("\nConcat (no dedup -- use Union for distinct):");
        foreach (var b in books.Concat(extra)) Console.WriteLine($"  {b.Title} ({b.Year})");

        Console.WriteLine("\nDistinct genres:");
        foreach (var g in books.Select(b => b.Genre).Distinct())
            Console.WriteLine($"  {g}");

        // Skip/Take: the standard pagination pattern
        var alphabetical = books.OrderBy(b => b.Title).ToList();
        Console.WriteLine("\nPage 2 (Skip 2, Take 2), alphabetical:");
        foreach (var b in alphabetical.Skip(2).Take(2)) Console.WriteLine($"  {b.Title}");

        // --- LINQ to XML: query results passed directly as XElement constructor arguments ---
        var xml = new XElement("Books",
            from b in books
            select new XElement("Book",
                new XAttribute("year", b.Year),
                new XElement("Title", b.Title),
                new XElement("Genre", b.Genre)));
        Console.WriteLine($"\nLINQ to XML ({xml.Elements().Count()} Book elements):");
        Console.WriteLine(xml);
    }
}
```
