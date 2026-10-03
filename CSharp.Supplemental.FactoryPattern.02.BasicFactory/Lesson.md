# Supplemental: Factory Pattern 02 -- Basic Factory

## What This Is

The factory method pattern applied to the same song-serialization problem from `01.NoFactory`. Same output, same two formats - the difference is in how the code is organized.

What changes from the previous project: the three concerns that were tangled in one method (format selection, JSON logic, XML logic) are now separated into three distinct things. The caller sees only the interface; the creator decides which product to use; each product does its own work in isolation.

---

## The Three Parts

### The Interface (public entry point)

```csharp
private static string Serialize(Song song, DataFormat dataFormat)
{
    var serializer = GetSerializer(dataFormat);
    return serializer(song);
}
```

The caller only ever sees `Serialize`. It knows nothing about JSON or XML - it just calls through. If a new format is added tomorrow, this method doesn't change.

### The Creator (factory method)

```csharp
private static Func<Song, string> GetSerializer(DataFormat dataFormat) => dataFormat switch
{
    DataFormat.Json => SerializeToJson,
    DataFormat.Xml  => SerializeToXml,
    _               => throw new ArgumentException($"Unknown data format: {dataFormat}")
};
```

`GetSerializer` is the factory: it receives a format identifier and returns the appropriate implementation. When a new format is added, this is one of the two places that changes - one new `case`.

The return type is `Func<Song, string>` - a delegate, not an object. The factory pattern doesn't require abstract classes or interfaces for the products; a method reference works fine when the product is a single operation.

### The Products (implementations)

```csharp
private static string SerializeToJson(Song song)
    => JsonSerializer.Serialize(song, new JsonSerializerOptions { WriteIndented = true });

private static string SerializeToXml(Song song)
{
    var songElement = new XElement("song",
        new XAttribute("id", song.SongId),
        new XElement("title", song.Title),
        new XElement("artist", song.Artist));
    return songElement.ToString();
}
```

Each product is isolated. The JSON developer never reads the XML code. The XML developer never reads the JSON code.

### The Enum

```csharp
internal enum DataFormat { Undefined = 0, Json = 1, Xml = 2 }
```

`DataFormat.Json` instead of `"JSON"` - compile-time validation. A typo is a build error, not a silent `ArgumentException` at runtime.

---

## What Changed From 01

Compare `Program.cs` in both projects side by side. The public interface (`Serialize`) is almost identical - it calls through to something. The difference is that "something" is now separated into a creator and independent products, rather than all three concerns collapsed into one method.

The `01.NoFactory` version's `if (dataFormat == "JSON")` block is now `SerializeToJson` - a named, independently-readable method. Same code, different organization.

---

## What the Next Project Demonstrates

`03.ImprovingPattern` adds a third format (YAML). Specifically: compare how much of this file (`02.BasicFactory/Program.cs`) changes to accommodate YAML versus how much of `01.NoFactory/Program.cs` would have changed. That comparison is the argument for the pattern.
