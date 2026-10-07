---
title: "LINQ to XML - Parse, Query, Transform, Mutate, Namespaces"
chapter: 10
index: 1
dependencies: []
---

```csharp
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

internal static class Program
{
    private static XElement GetLibrary() => XElement.Parse(@"
        <Library>
            <Book year=""1949""><Title>1984</Title><Author>George Orwell</Author><Genre>Dystopian</Genre></Book>
            <Book year=""1953""><Title>Fahrenheit 451</Title><Author>Ray Bradbury</Author><Genre>Dystopian</Genre></Book>
            <Book year=""1932""><Title>Brave New World</Title><Author>Aldous Huxley</Author><Genre>Dystopian</Genre></Book>
        </Library>");

    private static void Main()
    {
        var library = GetLibrary();

        // --- Navigate ---
        // Elements("Book"): immediate children only.
        // Descendants("Book"): all descendants at any depth -- safer when nesting isn't fixed.
        Console.WriteLine("All books (Elements):");
        foreach (var book in library.Elements("Book"))
        {
            string title = book.Element("Title")?.Value;
            string year  = book.Attribute("year")?.Value;
            Console.WriteLine($"  {title} ({year})");
        }

        // --- Query with LINQ -- same operators as any IEnumerable<T> ---
        Console.WriteLine("\nBooks after 1940, oldest first:");
        var recent = from b in library.Descendants("Book")
                     where int.Parse(b.Attribute("year").Value) > 1940
                     orderby b.Attribute("year").Value
                     select b.Element("Title").Value;
        foreach (var t in recent) Console.WriteLine($"  {t}");

        // --- Transform: reshape the tree in one expression ---
        // Child elements become attributes; no intermediate collections or loops needed.
        var flat = new XElement("FlatLibrary",
            from b in library.Elements("Book")
            select new XElement("Entry",
                new XAttribute("title",  b.Element("Title")?.Value  ?? ""),
                new XAttribute("author", b.Element("Author")?.Value ?? ""),
                new XAttribute("year",   b.Attribute("year")?.Value ?? "")));
        Console.WriteLine("\nReshaped (child elements -> attributes):");
        Console.WriteLine(flat);

        // --- Mutate in place ---
        library.Add(new XElement("Book", new XAttribute("year", "1965"),
            new XElement("Title", "Dune"),
            new XElement("Author", "Frank Herbert"),
            new XElement("Genre", "Science Fiction")));

        library.Elements("Book")
               .First(b => b.Element("Author")?.Value == "George Orwell")
               .SetElementValue("Genre", "Dystopian Classic");

        // .Remove() on an IEnumerable<XElement> removes all of them from their parent
        library.Elements("Book")
               .Where(b => b.Element("Author")?.Value == "Aldous Huxley")
               .Remove();

        Console.WriteLine("\nAfter add Dune, update 1984 genre, remove Brave New World:");
        Console.WriteLine(library);

        // --- Namespaces: the most common real-world LINQ to XML mistake ---
        // A plain "Title" (no namespace) matches NOTHING in a namespaced document.
        // No error, no exception -- just silently empty results.
        XNamespace ns = "http://example.com/library";
        var nsLibrary = new XElement(ns + "Library",
            new XElement(ns + "Book", new XAttribute("year", "1949"),
                new XElement(ns + "Title", "1984")));

        Console.WriteLine("\nNamespaced XML:");
        Console.WriteLine(nsLibrary);

        var withNs    = nsLibrary.Descendants(ns + "Title").Select(t => t.Value).ToList();
        var withoutNs = nsLibrary.Descendants("Title").Select(t => t.Value).ToList();
        Console.WriteLine($"\nWith correct namespace: {withNs.Count} result(s) -- [{string.Join(", ", withNs)}]");
        Console.WriteLine($"Without namespace:      {withoutNs.Count} result(s)  <-- silent empty, no error");

        // --- Save and Load ---
        string path = Path.Combine(Path.GetTempPath(), $"library-{Guid.NewGuid():N}.xml");
        try
        {
            library.Save(path);
            var reloaded = XElement.Load(path);
            Console.WriteLine($"\nSaved and reloaded. Book count: {reloaded.Elements("Book").Count()}");
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
```
