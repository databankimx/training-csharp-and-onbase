---
title: "Controlling What Survives Serialization"
chapter: 9
index: 2
dependencies: []
---

```csharp
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

// [JsonIgnore] tells System.Text.Json to skip the property during both
// serialization and deserialization.
// [XmlIgnore] does the same for XmlSerializer.
// Each serializer has its own attribute namespace -- they are not interchangeable.
public class Book
{
    public string Title  { get; set; }
    public string Author { get; set; }
    public int    Year   { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public string CachedSummary { get; private set; }

    public string Summary
    {
        get
        {
            CachedSummary ??= $"{Title} by {Author} ({Year})";
            return CachedSummary;
        }
    }

    public Book() { }
    public Book(string title, string author, int year)
    { Title = title; Author = author; Year = year; }

    public override string ToString() => Summary;
}

internal static class Program
{
    private static void Main()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"ch09-serial2-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var original = new Book("Dune", "Frank Herbert", 1965);

            // Force-compute the summary so it's cached on the original instance.
            _ = original.Summary;
            Console.WriteLine($"Original CachedSummary (cached): \"{original.CachedSummary}\"");
            Console.WriteLine();

            // --- JSON round-trip ---
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(original, options);
            Console.WriteLine("Serialized JSON (CachedSummary absent -- [JsonIgnore]):");
            Console.WriteLine(json);
            Console.WriteLine();

            var jsonRestored = JsonSerializer.Deserialize<Book>(json);
            Console.WriteLine($"Restored Title/Author/Year: {jsonRestored.Title}, {jsonRestored.Author}, {jsonRestored.Year}");
            Console.WriteLine($"Restored CachedSummary before access: \"{jsonRestored.CachedSummary}\"  <-- null");
            Console.WriteLine($"Restored Summary (freshly recomputed): \"{jsonRestored.Summary}\"");
            Console.WriteLine();

            // --- XML round-trip (same principle) ---
            string xmlPath    = Path.Combine(dir, "book.xml");
            var xmlSerializer = new XmlSerializer(typeof(Book));

            using (var s = new FileStream(xmlPath, FileMode.Create))
                xmlSerializer.Serialize(s, original);

            Console.WriteLine("Serialized XML (CachedSummary absent -- [XmlIgnore]):");
            Console.WriteLine(File.ReadAllText(xmlPath));

            Book xmlRestored;
            using (var s = new FileStream(xmlPath, FileMode.Open))
                xmlRestored = (Book)xmlSerializer.Deserialize(s);
            Console.WriteLine($"Restored CachedSummary before access: \"{xmlRestored.CachedSummary}\"  <-- null");
            Console.WriteLine($"Restored Summary (freshly recomputed): \"{xmlRestored.Summary}\"");
            Console.WriteLine();

            Console.WriteLine("Key rules:");
            Console.WriteLine("  [JsonIgnore] / [XmlIgnore]: exclude a property from the serialized form.");
            Console.WriteLine("  Each serializer has its OWN attribute namespace -- not interchangeable.");
            Console.WriteLine("  Derived/cached values should be recomputed, not persisted as stale state.");
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }
}
```
