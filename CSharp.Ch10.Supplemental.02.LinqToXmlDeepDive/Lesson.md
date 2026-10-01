# Chapter 10 Supplemental 02: LINQ to XML Deep Dive

## What This Is

The main lesson's `BooksToXml()` showed one direction: building an `XElement` tree out of a LINQ query. This project covers the rest: parsing existing XML, querying it with LINQ, reshaping it, modifying it in place, handling namespaces, and saving and loading from a real file.

LINQ to XML (`XElement`, `XAttribute`, `XDocument` -- all in `System.Xml.Linq`) is worth knowing specifically as the modern replacement for the older `XmlDocument`/`XmlNode` DOM API. Everything here reads and writes noticeably more naturally.

---

## How to Write This Program

Add a shared data helper to `Program.cs` -- every mini-program calls this:

```csharp
private static XElement GetLibraryXml()
{
    return XElement.Parse(@"
        <Library>
            <Book year=""1949"">
                <Title>1984</Title>
                <Author>George Orwell</Author>
                <Genre>Dystopian</Genre>
            </Book>
            <Book year=""1953"">
                <Title>Fahrenheit 451</Title>
                <Author>Ray Bradbury</Author>
                <Genre>Dystopian</Genre>
            </Book>
            <Book year=""1932"">
                <Title>Brave New World</Title>
                <Author>Aldous Huxley</Author>
                <Genre>Dystopian</Genre>
            </Book>
        </Library>");
}
```

Also create a temp file path for the save/load mini-program:

```csharp
string tempFilePath = Path.Combine(Path.GetTempPath(), $"ch10-linqtoxml-demo-{Guid.NewGuid():N}.xml");
```

Delete it in `finally`.

---

### Mini-Program 1: Parsing and Navigating

Clear `Main()` and write:

```csharp
var library = GetLibraryXml();

Console.WriteLine($"Root element name: {library.Name}");

Console.WriteLine("\nAll books (Elements(\"Book\")):");
foreach (var book in library.Elements("Book"))
{
    string title = book.Element("Title")?.Value;
    string year  = book.Attribute("year")?.Value;
    Console.WriteLine($" - {title} ({year})");
}

GenericFunctions.Pause();
```

Run it. Three books, each with its year from the attribute.

`Elements("Book")` returns only the **immediate** child elements named `Book`. `.Element("Title")` returns the first matching child element; `.Value` is its text content. `.Attribute("year")` reads an attribute directly off the element.

The `?.` on both calls is the same null-safety pattern used throughout the codebase -- if the element or attribute doesn't exist, the result is `null` rather than an exception.

### Mini-Program 2: Querying With LINQ

Clear `Main()` and write:

```csharp
var library = GetLibraryXml();

// Descendants() searches through every nesting level, not just immediate children.
// Use it over Elements() whenever the XML's nesting depth isn't fixed or known.
var titlesAfter1940 = from book in library.Descendants("Book")
                      where int.Parse(book.Attribute("year").Value) > 1940
                      orderby book.Attribute("year").Value
                      select book.Element("Title").Value;

Console.WriteLine("Books published after 1940, oldest first:");
foreach (string title in titlesAfter1940)
    Console.WriteLine($" - {title}");

GenericFunctions.Pause();
```

Run it. `1984` (1949) and `Fahrenheit 451` (1953) -- `Brave New World` (1932) is filtered out.

LINQ operators work on `XElement` sequences exactly the same way they work on any other `IEnumerable<T>`. Filtering, ordering, projection, grouping -- all of it applies. `Descendants("Book")` produces an `IEnumerable<XElement>`, and the query runs from there.

### Mini-Program 3: Reshaping XML

Clear `Main()` and write:

```csharp
var library = GetLibraryXml();

// Flatten <Book><Title>...</Title><Author>...</Author></Book> into
// <Entry title="..." author="..." year="..." /> (attributes instead of child elements).
var flattened = new XElement("FlatLibrary",
    from book in library.Elements("Book")
    select new XElement("Entry",
        new XAttribute("title",  book.Element("Title")?.Value  ?? ""),
        new XAttribute("author", book.Element("Author")?.Value ?? ""),
        new XAttribute("year",   book.Attribute("year")?.Value ?? "")));

Console.WriteLine("Reshaped XML:");
Console.WriteLine(flattened);
GenericFunctions.Pause();
```

Run it. Same data, completely different structure -- nested child elements flattened into attributes on a single element.

This is LINQ to XML's strength: transformation. A LINQ query projecting one `XElement` per source element, passed directly to another `XElement` constructor, produces a new tree in one expression. No intermediate collections, no separate loops.

### Mini-Program 4: Modifying in Place

Clear `Main()` and write:

```csharp
var library = GetLibraryXml();

// Add a new book.
library.Add(new XElement("Book",
    new XAttribute("year", "1965"),
    new XElement("Title", "Dune"),
    new XElement("Author", "Frank Herbert"),
    new XElement("Genre", "Science Fiction")));

// Update an existing element.
var orwellBook = library.Elements("Book")
    .First(b => b.Element("Author")?.Value == "George Orwell");
orwellBook.SetElementValue("Genre", "Dystopian Classic");

// Remove elements matching a condition.
library.Elements("Book")
    .Where(b => b.Element("Author")?.Value == "Aldous Huxley")
    .Remove();

Console.WriteLine("Library after adding Dune, updating 1984's Genre, removing Brave New World:");
Console.WriteLine(library);
GenericFunctions.Pause();
```

Run it. Dune added, 1984's genre updated, Brave New World gone.

`SetElementValue("Genre", "Dystopian Classic")` replaces the element's text content in place. `.Remove()` on an `IEnumerable<XElement>` removes all of them from their parent -- no separate loop or index tracking needed. These mutations happen directly on the in-memory tree.

### Mini-Program 5: Namespaces

Clear `Main()` and write:

```csharp
XNamespace ns = "http://example.com/library";

var library = new XElement(ns + "Library",
    new XElement(ns + "Book", new XAttribute("year", "1949"),
        new XElement(ns + "Title", "1984")));

Console.WriteLine("XML with an explicit namespace:");
Console.WriteLine(library);

// Querying namespaced XML requires the SAME XNamespace.
// A plain "Title" (no namespace) matches nothing.
var titles = library.Descendants(ns + "Title").Select(t => t.Value);

Console.WriteLine("\nTitles found (with correct namespace):");
foreach (string title in titles)
    Console.WriteLine($" - {title}");

GenericFunctions.Pause();
```

Run it. The `xmlns` declaration appears in the output automatically.

`XNamespace ns = "..."` combined with `ns + "Book"` produces a fully-qualified element name. Every element and attribute in a namespaced document must be addressed with its namespace in LINQ queries. A plain string `"Title"` without the namespace prefix matches nothing in a namespaced document -- the single most common LINQ to XML mistake when first working with real-world XML.

### Mini-Program 6: Save and Load

Clear `Main()` and write:

```csharp
var library = GetLibraryXml();

library.Save(tempFilePath);
Console.WriteLine($"Saved to {tempFilePath}");

var reloaded = XElement.Load(tempFilePath);
Console.WriteLine($"\nReloaded from disk, book count: {reloaded.Elements("Book").Count()}");
GenericFunctions.Pause();
```

Run it. Saved, reloaded, same count.

`XElement.Save(path)` writes the tree to a file. `XElement.Load(path)` reads it back. Both handle encoding and the XML declaration automatically. `XDocument` wraps an `XElement` root and adds the XML declaration (`<?xml version="1.0" encoding="utf-8"?>`) to the output -- use `XDocument` when you need the declaration, `XElement` when you're working with fragments or don't need it.

---

## Takeaways

- `Elements("Name")` returns immediate children only. `Descendants("Name")` searches all levels.
- `.Element("Name")` returns the first matching child; `.Attribute("name")` returns an attribute.
- `.Value` is the element's text content.
- LINQ operators work on `IEnumerable<XElement>` exactly like any other sequence.
- `.Remove()` on an `IEnumerable<XElement>` removes all matching elements from their parent.
- `SetElementValue()` replaces an element's text content in place.
- Namespaced XML must be queried with the correct `XNamespace`. A plain string matches nothing in a namespaced document.
- `XElement.Save()` / `XElement.Load()` handle file I/O and encoding automatically.
- Use `XDocument` when you need the XML declaration; `XElement` for fragments.
