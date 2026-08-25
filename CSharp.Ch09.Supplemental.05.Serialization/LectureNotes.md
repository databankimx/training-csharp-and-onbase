# Chapter 9 Supplemental 05: Serialization

## What This Is

The "Understanding Serialization" section of Chapter 9: Binary, XML, JSON, and custom serialization, all demonstrated against the same `Book` class so the different formats can be compared directly. Fully self-contained, runs against a temporary working directory created on startup and deleted on exit.

---

## The Security Warning First, Since It Matters Most

`BinaryFormatter` (used for both Binary and Custom Serialization below) has real, well-documented security problems: deserializing binary data from an **untrusted** source with it can let an attacker run arbitrary code, just by handing your program a maliciously crafted byte stream. Microsoft's own guidance is to avoid it in new code. It's covered here because it's part of this chapter's official curriculum and still functions in classic .NET Framework, but a real application should prefer XML or JSON serialization for anything that might ever touch untrusted data, a file from a user, a network payload, anything crossing a genuine trust boundary.

---

## Binary Serialization

```csharp
var formatter = new BinaryFormatter();
using (var stream = new FileStream(filePath, FileMode.Create))
{
    formatter.Serialize(stream, book);
}
```

The most compact, fastest format here, and the least portable, a binary blob is specific to .NET's own object representation, not readable by anything outside .NET, and not human-readable at all.

---

## XML Serialization

```csharp
var serializer = new XmlSerializer(typeof(Book));
using (var stream = new FileStream(filePath, FileMode.Create))
{
    serializer.Serialize(stream, book);
}
```

Worth reading closely: `XmlSerializer` has **no idea what `[Serializable]`, `[NonSerialized]`, or `ISerializable` even mean**. Those are specifically `BinaryFormatter` concepts. `XmlSerializer` only cares about two things: a public parameterless constructor (`Book` has one), and public read/write properties, it serializes every one of those, with no way to opt individual properties out the way `[NonSerialized]` does for binary. Customizing XML output uses an entirely separate mechanism, `IXmlSerializable`, not covered here, worth knowing exists if you ever need it.

---

## JSON Serialization

```csharp
string json = JsonConvert.SerializeObject(book, Formatting.Indented);
var restoredBook = JsonConvert.DeserializeObject<Book>(json);
```

Using Newtonsoft.Json (Json.NET), the long-established JSON library for .NET, including classic .NET Framework (modern .NET also has `System.Text.Json` built in, but Newtonsoft.Json remains extremely common, especially in Framework codebases). Same basic behavior as `XmlSerializer`: works off public properties, `[Serializable]`/`ISerializable` are irrelevant here too. JSON is the most broadly interoperable of the three formats covered, readable by essentially every language and platform, and the de facto standard for web APIs.

---

## Custom Serialization: `ISerializable`, and Why You'd Want It

```csharp
[Serializable]
public class Book : ISerializable
{
    [NonSerialized]
    private string cachedSummary;

    public string Summary => cachedSummary ??= $"{Title} by {Author} ({Year})";

    protected Book(SerializationInfo info, StreamingContext context)
    {
        Title = info.GetString(nameof(Title));
        Author = info.GetString(nameof(Author));
        Year = info.GetInt32(nameof(Year));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Title), Title);
        info.AddValue(nameof(Author), Author);
        info.AddValue(nameof(Year), Year);
    }
}
```

`ISerializable` gives you explicit, hand-written control over exactly what a `BinaryFormatter` writes and reads, beyond what `[Serializable]`/`[NonSerialized]` alone provide. This project's `Book` uses it for a genuinely realistic reason: `cachedSummary` is a computed, cached value, not real state worth persisting, if it were serialized and later restored, it could go stale (imagine `Title` being editable after deserialization, the cached `Summary` wouldn't reflect the change). `GetObjectData()` deliberately omits it entirely; the special deserialization constructor only restores `Title`/`Author`/`Year`, leaving `Summary` to recompute itself, correctly, the next time it's actually read.

`UsingCustomSerialization()` makes this visible directly: the original `Book` has `Summary` already cached (read once, before serializing), but the restored instance never received that cached value at all, its `Summary` is computed fresh, from the actual restored data, not carried over as potentially-stale persisted state.

---

## Worth Comparing All Four Side by Side

Run this project and look at the printed XML and JSON output directly, both are genuinely human-readable text, worth comparing their relative verbosity for the same data. Binary produces no readable output at all (its byte count is printed instead), and Custom Serialization demonstrates that binary's `ISerializable` customization hook has no equivalent reach into the XML or JSON paths, each serialization mechanism in .NET has its own, separate way of being customized, there's no single "customize serialization" switch that applies universally across formats.
