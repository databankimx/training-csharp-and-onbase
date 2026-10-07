---
title: "Improving the Pattern - Adding a Third Format"
chapter: 0
index: 3
dependencies: []
---

```csharp
using System;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

// Note: the real project uses YamlDotNet for YAML serialization, which is not available
// in the runner. This step uses a manual YAML formatter to demonstrate the same structural
// point - open the full project in Visual Studio to see YamlDotNet in action.

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
        Console.WriteLine("YAML");
        Console.WriteLine(Serialize(song, DataFormat.Yaml));

        Console.WriteLine();
        Console.WriteLine("Compare this file to 02.BasicFactory: the Interface (Serialize) didn't change.");
        Console.WriteLine("Adding YAML meant one enum value, one switch arm, one new method.");
        Console.WriteLine("Nothing that already worked had to be touched. That's the pattern's promise.");
    }

    private static string Serialize(Song song, DataFormat format)
        => GetSerializer(format)(song);

    private static Func<Song, string> GetSerializer(DataFormat format) => format switch
    {
        DataFormat.Json => SerializeToJson,
        DataFormat.Xml  => SerializeToXml,
        DataFormat.Yaml => SerializeToYaml,
        _               => throw new ArgumentException($"Unknown format: {format}"),
    };

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

    // Manual YAML - substitutes for YamlDotNet in the runner context only.
    private static string SerializeToYaml(Song song)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"songId: {song.SongId}");
        sb.AppendLine($"title: {song.Title}");
        sb.AppendLine($"artist: {song.Artist}");
        return sb.ToString();
    }
}

internal class Song
{
    public string SongId  { get; }
    public string Title   { get; }
    public string Artist  { get; }
    public Song(string id, string title, string artist) { SongId = id; Title = title; Artist = artist; }
}

internal enum DataFormat { Undefined = 0, Json = 1, Xml = 2, Yaml = 3 }
```
