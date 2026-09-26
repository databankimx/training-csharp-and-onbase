# Factory Pattern: 2 of 3 - Basic Factory

## Definition

The **Factory Method** is a creational design pattern that provides an interface for creating objects, without the calling code needing to know which concrete type actually gets created. It separates *what* gets built from *how* it's decided what to build.

## The Three Pieces

Same task as `01.NoFactory` - serialize a `Song` to JSON or XML - restructured into three distinct roles:

- **Interface** (`Serialize`) - what the caller actually calls. Doesn't know or care how many formats exist, or how any of them work.
- **Creator** (`GetSerializer`) - decides which serialization logic applies, based on the requested `DataFormat`, and hands back the matching function.
- **Products** (`SerializeToJson`, `SerializeToXml`) - the actual serialization logic for one specific format each, with no awareness of each other or of how they got selected.

```csharp
private static string Serialize(Song song, DataFormat dataFormat)      // Interface
{
    var serializer = GetSerializer(dataFormat);
    return serializer(song);
}

private static Func<Song, string> GetSerializer(DataFormat dataFormat) => dataFormat switch  // Creator
{
    DataFormat.Json => SerializeToJson,
    DataFormat.Xml => SerializeToXml,
    _ => throw new ArgumentException($"Unknown data format: {dataFormat}")
};

private static string SerializeToJson(Song song) => ...                // Product
private static string SerializeToXml(Song song) => ...                 // Product
```

## Why Bother

This doesn't look dramatically different from `01.NoFactory` yet - same two formats, same output. The payoff isn't visible until something changes. See `03.ImprovingPattern` for what "something changes" actually looks like in practice.

## Key Advantages

- **Single Responsibility** - the Creator's only job is picking a Product; each Product's only job is one format.
- **Decoupling** - the Interface never touches format-specific logic at all.
- **Maintainability** - understanding or fixing the XML logic means reading `SerializeToXml`, nothing else.
