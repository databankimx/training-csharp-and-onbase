# Factory Pattern: 1 of 3 - No Factory

## The Task

A client has a library of songs and wants to convert them to a more convenient format - JSON or XML. This first version solves that the straightforward way: one `Serialize` method, one `if` per format.

```csharp
private static string Serialize(Song song, string dataFormat)
{
    if (dataFormat == "JSON")
    {
        return JsonSerializer.Serialize(song, new JsonSerializerOptions { WriteIndented = true });
    }

    if (dataFormat == "XML")
    {
        var songElement = new XElement("song", ...);
        return songElement.ToString();
    }

    throw new ArgumentException($"Unknown data format: {dataFormat}");
}
```

## Where This Starts to Hurt

This works fine for two formats. It's still fine for three. By the time there's a fifth or sixth format, `Serialize` is a long chain of `if` statements where the JSON logic, the XML logic, and everything else all live in the same method, and every new format means editing that same method again - reading past every format that already works just to add the one that doesn't yet.

There's also no way to test "does XML serialization work" without going through the entire `Serialize` method and its string-based format switch. The format-detection logic and the actual serialization logic are welded together.

## What's Next

`02.BasicFactory` restructures this exact same task using the **Factory Method** pattern - separating "which serializer do I need" from "what does that serializer actually do."
