---
title: "XML and JSON Serialization"
chapter: 9
index: 1
dependencies: []
---

```csharp
using System;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

// XmlSerializer requirements:
//   - Public parameterless constructor (mandatory)
//   - Only PUBLIC read/write properties are serialized
//   - Output is human-readable, self-describing XML
// Customization uses [XmlElement], [XmlAttribute], [XmlRoot] --
// a completely separate attribute namespace from JSON concepts.
public class Book
{
    public string Title  { get; set; }
    public string Author { get; set; }
    public int    Year   { get; set; }

    public Book() { }
    public Book(string title, string author, int year)
    { Title = title; Author = author; Year = year; }

    public override string ToString() => $"{Title} by {Author} ({Year})";
}

internal static class Program
{
    private static void Main()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"ch09-serial-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            // --- XmlSerializer ---
            string xmlPath    = Path.Combine(dir, "book.xml");
            var xmlBook       = new Book("Brave New World", "Aldous Huxley", 1932);
            var xmlSerializer = new XmlSerializer(typeof(Book));

            using (var s = new FileStream(xmlPath, FileMode.Create))
                xmlSerializer.Serialize(s, xmlBook);

            Console.WriteLine("Generated XML:");
            Console.WriteLine(File.ReadAllText(xmlPath));

            Book xmlRestored;
            using (var s = new FileStream(xmlPath, FileMode.Open))
                xmlRestored = (Book)xmlSerializer.Deserialize(s);
            Console.WriteLine($"Deserialized: {xmlRestored}\n");

            // --- System.Text.Json ---
            // JsonSerializer.Serialize/Deserialize<T>() -- part of the .NET BCL since .NET Core 3.0,
            // no external NuGet package required.
            // DeserializeObject<T>() is generic -- no cast required.
            // More compact than XML, equally human-readable, dominant for REST APIs and config.
            // Considered safe at trust boundaries -- a malicious JSON payload can populate
            // properties but cannot redirect the type system.
            // Customization: [JsonPropertyName], [JsonIgnore], JsonConverter<T>.
            string jsonPath = Path.Combine(dir, "book.json");
            var jsonBook    = new Book("Fahrenheit 451", "Ray Bradbury", 1953);
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string json     = JsonSerializer.Serialize(jsonBook, jsonOptions);
            File.WriteAllText(jsonPath, json);

            Console.WriteLine("Generated JSON:");
            Console.WriteLine(json);

            var jsonRestored = JsonSerializer.Deserialize<Book>(json);
            Console.WriteLine($"Deserialized: {jsonRestored}\n");

            // --- Side-by-side comparison ---
            Console.WriteLine($"XML file size:  {new FileInfo(xmlPath).Length} bytes");
            Console.WriteLine($"JSON file size: {new FileInfo(jsonPath).Length} bytes");
            Console.WriteLine("XML is more verbose; JSON is more compact.");
            Console.WriteLine("Both are human-readable and self-describing.");
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }
}
```
