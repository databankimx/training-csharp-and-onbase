# Chapter 10 Supplemental 02: LINQ to XML Deep Dive

## What This Is

The main lesson's `BooksToXml()` showed the basics of *building* XML from a LINQ query. This Supplemental covers the rest of `System.Xml.Linq`: parsing existing XML, querying and navigating it, transforming its shape, modifying it in place, handling namespaces, and saving/loading a real file.

---

## Parsing and Navigating

```csharp
var library = XElement.Parse(xmlText);

foreach (var book in library.Elements("Book"))
{
    string title = book.Element("Title")?.Value;
    string year = book.Attribute("year")?.Value;
}
```

`Elements("Book")` returns only the *immediate* child `<Book>` elements. `Element("Title")` (singular) returns the *first* matching child, or `null` if none exists, worth the `?.` null-conditional operator, since a malformed or differently-shaped document could genuinely be missing an expected element. `.Attribute("year")` reads an XML attribute directly.

---

## `Elements()` vs. `Descendants()`

```csharp
var titlesAfter1940 = from book in library.Descendants("Book")
                       where int.Parse(book.Attribute("year").Value) > 1940
                       select book.Element("Title").Value;
```

`Descendants()` digs through *every* level of nesting, not just immediate children, worth reaching for whenever the XML's structure isn't a fixed, known depth (or when you genuinely don't care where in the tree a match occurs). `Elements()` is more precise when you know exactly which level you're working at, and won't accidentally match something nested more deeply than expected.

---

## Transforming Shape

```csharp
var flattened = new XElement("FlatLibrary",
    from book in library.Elements("Book")
    select new XElement("Entry",
        new XAttribute("title", book.Element("Title")?.Value ?? ""),
        new XAttribute("author", book.Element("Author")?.Value ?? "")));
```

Worth noticing this is the exact same pattern as any other LINQ projection, `select new XElement(...)` instead of `select new { ... }`. Querying XML and reshaping it into a *different* XML structure entirely (nested elements collapsed into attributes here) is genuinely just LINQ, nothing XML-specific about the technique itself.

---

## Modifying a Loaded Tree

```csharp
library.Add(new XElement("Book", ...));                                          // add
orwellBook.SetElementValue("Genre", "Dystopian Classic");                        // update
library.Elements("Book").Where(b => ...).Remove();                               // delete
```

`XElement` trees are fully mutable once loaded into memory. `.Add()` appends a new child, `.SetElementValue()` updates (or creates, if missing) a named child element's text, and `.Remove()` (called on a *filtered sequence* of elements, not the parent) removes every element in that sequence from its parent. Worth noticing `.Remove()` here is an extension method that removes each matched element from wherever it currently lives, a compact way to delete "every element matching this condition" without a separate loop.

---

## Namespaces

```csharp
XNamespace ns = "http://example.com/library";
var library = new XElement(ns + "Library", new XElement(ns + "Book", ...));

var titles = library.Descendants(ns + "Title").Select(t => t.Value);
```

`XNamespace` represents a namespace URI; combining it with a local name via `+` produces a fully-qualified `XName`. The gotcha worth internalizing: **querying namespaced XML requires using that exact same namespace when naming what you're searching for**. `library.Descendants("Title")` (no namespace) against namespaced XML finds nothing at all, silently, no error, just an empty result, since `"Title"` and `{http://example.com/library}Title` are different names as far as XML is concerned. This is a genuinely common, confusing gotcha the first time you work with real-world XML (which very often uses namespaces) after only having practiced against plain, namespace-free XML.

---

## Saving and Loading

```csharp
library.Save(filePath);
var reloaded = XElement.Load(filePath);
```

Straightforward round-tripping to and from a real file, `Save()`/`Load()` handle the file I/O and XML serialization together, no separate `StreamWriter`/`StreamReader` needed the way raw text or binary I/O required back in Chapter 9.
