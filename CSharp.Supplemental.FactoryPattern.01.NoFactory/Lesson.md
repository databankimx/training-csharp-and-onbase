# Supplemental: Factory Pattern 01 -- No Factory

## What This Is

A starting point for the three-project Factory Pattern series. This project does the job -- serializes a `Song` to JSON or XML -- but without any separation between the caller, the format-selection logic, and the serialization code. All three are tangled together in one method.

Read this project first. The next two projects (`02.BasicFactory` and `03.ImprovingPattern`) apply the pattern progressively; understanding what problem they're solving requires seeing the problem first.

---

## The Code

```csharp
private static string Serialize(Song song, string dataFormat)
{
    if (dataFormat == "JSON")
    {
        return JsonSerializer.Serialize(song, new JsonSerializerOptions { WriteIndented = true });
    }

    if (dataFormat == "XML")
    {
        var songElement = new XElement("song",
            new XAttribute("id", song.SongId),
            new XElement("title", song.Title),
            new XElement("artist", song.Artist));
        return songElement.ToString();
    }

    throw new ArgumentException($"Unknown data format: {dataFormat}");
}
```

It works. Run it and it produces correct JSON and XML.

---

## The Problem

Every concern is in the same place:

- **Format selection** (`if (dataFormat == "JSON")`) -- deciding which branch to take.
- **JSON serialization** -- the actual JSON-specific work.
- **XML serialization** -- the actual XML-specific work.

Adding a third format means editing this same method. A developer who only understands JSON and has no interest in the XML path has to read (and risk breaking) the XML code to add their format. A developer who only wants to change the JSON output has to navigate past the XML logic to find the right place.

The format string is also a plain `string`, not an enum. `"json"` and `"Json"` both silently fall through to the `ArgumentException`. There's no compile-time validation of which values are accepted.

This is the shape of code that accumulates a sprawling `switch` or `if` chain as more formats are added -- held together only by the original method and every maintainer's awareness of where that method lives.

---

## What Changes in the Next Project

`02.BasicFactory` separates the three concerns into three distinct things: a public interface (what callers call), a creator (what decides which implementation to use), and products (the implementations themselves). Adding a new format in that version touches exactly those three things, and only those three things.

Compare the two `Program.cs` files side by side after running both.
