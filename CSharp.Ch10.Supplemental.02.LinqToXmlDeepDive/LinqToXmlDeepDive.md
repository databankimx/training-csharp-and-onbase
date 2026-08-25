# LINQ to XML Deep Dive

## Introduction

The main LINQ lesson showed building simple XML from a query. This lesson goes further: reading existing XML, querying it, changing its shape, editing it, working with namespaces, and saving to a real file.

---

## Reading XML

```csharp
var library = XElement.Parse(xmlText);

foreach (var book in library.Elements("Book"))
{
    string title = book.Element("Title")?.Value;
    string year = book.Attribute("year")?.Value;
}
```

`Elements("Book")` gets direct children named `Book`. `Element("Title")` gets the first child named `Title`. `Attribute("year")` reads an attribute. The `?.` handles the case where something you expected isn't actually there.

---

## Searching Deeper: `Descendants()`

```csharp
var allTitles = library.Descendants("Title").Select(t => t.Value);
```

`Descendants()` searches at *every* level of nesting, not just the top. Use it when you're not sure how deep something is nested, or don't care.

---

## Querying With LINQ, Just Like Anything Else

```csharp
var recentBooks = from book in library.Descendants("Book")
                   where int.Parse(book.Attribute("year").Value) > 1940
                   select book.Element("Title").Value;
```

Once you have an `XElement`, it's just LINQ from there, `where`, `orderby`, `select`, all work exactly the same as querying a list.

---

## Editing XML

```csharp
library.Add(new XElement("Book", ...));                    // add a new element
someBook.SetElementValue("Genre", "New Genre");             // change a value
library.Elements("Book").Where(b => ...).Remove();          // remove matching elements
```

Once loaded, an XML document is fully editable in memory.

---

## Namespaces: A Common Gotcha

```csharp
XNamespace ns = "http://example.com/library";
var library = new XElement(ns + "Library", ...);

// Searching WITHOUT the namespace finds nothing, silently:
library.Descendants("Title");          // empty!
library.Descendants(ns + "Title");     // correct
```

If XML uses a namespace, you have to search using that same namespace, or your query just quietly returns nothing. This trips people up constantly the first time they work with real XML from an external source (which very often has namespaces).

---

## Saving and Loading

```csharp
library.Save(filePath);
var reloaded = XElement.Load(filePath);
```

Simple, one-line round-tripping to a real file.

---

## Try It Yourself

Run `WorkingWithNamespaces()` and notice the demo deliberately uses the correct namespace when searching, then try changing that search to leave the namespace off, you'll get back nothing at all, with no error telling you why.
