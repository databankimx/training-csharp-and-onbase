---
title: "Basic Factory - Introducing the Pattern"
chapter: 0
index: 2
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
        Console.WriteLine(Serialize(song, DataFormat.Json));
        Console.WriteLine();
        Console.WriteLine("XML");
        Console.WriteLine(Serialize(song, DataFormat.Xml));

        Console.WriteLine();
        Console.WriteLine("The public interface (Serialize) is now completely unaware of how many");
        Console.WriteLine("formats exist or how each one works. The Creator (GetSerializer) decides");
        Console.WriteLine("which Product (function) to hand back. Adding a format means one new enum");
        Console.WriteLine("value, one new switch arm, and one new method - nothing existing changes.");
    }

    // Interface - callers only ever touch this
    private static string Serialize(Song song, DataFormat format)
        => GetSerializer(format)(song);

    // Creator - decides which product to return
    private static Func<Song, string> GetSerializer(DataFormat format) => format switch
    {
        DataFormat.Json => SerializeToJson,
        DataFormat.Xml  => SerializeToXml,
        _               => throw new ArgumentException($"Unknown format: {format}"),
    };

    // Products - one method per format
    private static string SerializeToJson(Song song)
        => JsonSerializer.Serialize(song, new JsonSerializerOptions { WriteIndented = true });

    private static string SerializeToXml(Song song)
    {
        var element = new XElement("song",
            new XAttribute("id", song.SongId),
            new XElement("title", song.Title),
            new XElement("artist", song.Artist));
        return element.ToString();
    }
}

internal class Song
{
    public string SongId  { get; }
    public string Title   { get; }
    public string Artist  { get; }
    public Song(string id, string title, string artist) { SongId = id; Title = title; Artist = artist; }
}

internal enum DataFormat { Undefined = 0, Json = 1, Xml = 2 }
```
