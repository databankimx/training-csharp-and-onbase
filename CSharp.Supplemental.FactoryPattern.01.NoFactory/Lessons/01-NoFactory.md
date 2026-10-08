---
title: "No Factory - The Problem"
chapter: 0
index: 1
dependencies: []
---

```csharp
using System;
using System.Text.Json;
using System.Xml.Linq;

internal static class Program
{
    private static void Main()
    {
        var song = new Song("1", "Hello", "John Doe");

        Console.WriteLine("JSON");
        Console.WriteLine(Serialize(song, "JSON"));
        Console.WriteLine();
        Console.WriteLine("XML");
        Console.WriteLine(Serialize(song, "XML"));

        Console.WriteLine();
        Console.WriteLine("Problem: every format the caller might ask for, and every step of producing it,");
        Console.WriteLine("is tangled together in one Serialize method. Adding a third format means editing");
        Console.WriteLine("the same method again - there is nowhere else for that logic to go.");
    }

    // Every format and every step of producing it all tangled together.
    // Adding a third format means editing this same method again.
    private static string Serialize(Song song, string format)
    {
        if (format == "JSON")
            return JsonSerializer.Serialize(song, new JsonSerializerOptions { WriteIndented = true });

        if (format == "XML")
        {
            var element = new XElement("song",
                new XAttribute("id", song.SongId),
                new XElement("title", song.Title),
                new XElement("artist", song.Artist));
            return element.ToString();
        }

        throw new ArgumentException($"Unknown format: {format}");
    }
}

internal class Song
{
    public string SongId  { get; }
    public string Title   { get; }
    public string Artist  { get; }
    public Song(string id, string title, string artist) { SongId = id; Title = title; Artist = artist; }
}
```
