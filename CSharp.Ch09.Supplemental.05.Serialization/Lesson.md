# Chapter 9 Supplemental 05: Serialization

## What This Is

File I/O (`Supplemental.04`) worked at the byte and text level - you controlled exactly what bytes went where. Serialization is the layer above that: converting an entire object's state into a storable or transmittable form - XML, JSON, binary - and deserializing it back. The goal is to persist or transmit the object, not just its data.

What's being abstracted here is the mapping between objects and their stored representation. Instead of manually writing each property to a stream and reading them back, the serializer handles that translation automatically. The tradeoff at each level: binary is compact and fast but opaque and fragile; XML and JSON are human-readable, self-describing, and interoperable, at the cost of verbosity.

Everything here runs against a temporary working directory created on startup and deleted on exit.

---

## A Note on BinaryFormatter

The original curriculum included `BinaryFormatter` here, and the project source still contains it. It has been removed from the LessonRunner steps for two reasons that reinforce each other.

**Security.** `BinaryFormatter` has well-documented vulnerabilities: deserializing binary data from an untrusted source can let an attacker execute arbitrary code simply by handing your program a maliciously crafted byte stream. Microsoft's own guidance, since .NET 5, is to treat it as permanently deprecated for anything crossing a trust boundary.

**Runtime availability.** The LessonRunner compiles against .NET Framework 4.8 reference assemblies but executes on the .NET 10 runtime, where `BinaryFormatter` has been removed entirely. Steps that call it throw `PlatformNotSupportedException` before doing anything useful.

The concepts it illustrated - controlling what gets written during serialization, omitting derived or cached state, using `[NonSerialized]` and `ISerializable` - are covered in the steps below using XML and JSON instead, which teach the same ideas on a foundation that actually runs.

If you want to see `BinaryFormatter` and `ISerializable` in action, run the project source directly from Visual Studio against a .NET Framework target. The source is intact; only the LessonRunner steps were updated.

---

## A Note on Newtonsoft.Json vs. System.Text.Json

The project source uses `Newtonsoft.Json` (`JsonConvert.SerializeObject` / `DeserializeObject<T>()`), which is the long-established third-party library and the standard in .NET Framework codebases. The LessonRunner steps use `System.Text.Json` (`JsonSerializer.Serialize` / `Deserialize<T>()`) instead, because `Newtonsoft.Json` is not available in the runner's compilation environment.

The two libraries are structurally similar:

| | `Newtonsoft.Json` | `System.Text.Json` |
|---|---|---|
| Serialize | `JsonConvert.SerializeObject(obj, Formatting.Indented)` | `JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true })` |
| Deserialize | `JsonConvert.DeserializeObject<T>(json)` | `JsonSerializer.Deserialize<T>(json)` |
| Skip a property | `[JsonIgnore]` | `[JsonIgnore]` (same name, different namespace) |
| Rename a property | `[JsonProperty("name")]` | `[JsonPropertyName("name")]` |
| Custom converter | `JsonConverter` | `JsonConverter<T>` |

`System.Text.Json` ships in the .NET BCL from .NET Core 3.0 onward - no NuGet package required. For new projects targeting .NET 5+, it is the default choice. `Newtonsoft.Json` remains widely used in existing .NET Framework projects and wherever its richer feature set (more lenient parsing, broader type support) is needed.

---

## How to Write This Program

### The Model

```csharp
public class Book
{
    public string Title  { get; set; }
    public string Author { get; set; }
    public int    Year   { get; set; }

    public Book() { }  // required by XmlSerializer

    public Book(string title, string author, int year)
    {
        Title = title; Author = author; Year = year;
    }

    public override string ToString() => $"{Title} by {Author} ({Year})";
}
```

---

### Mini-Program 1: XML Serialization

```csharp
var serializer = new XmlSerializer(typeof(Book));
var book = new Book("Brave New World", "Aldous Huxley", 1932);

using (var stream = new FileStream(filePath, FileMode.Create))
    serializer.Serialize(stream, book);

Console.WriteLine(File.ReadAllText(filePath));

Book restored;
using (var stream = new FileStream(filePath, FileMode.Open))
    restored = (Book)serializer.Deserialize(stream);
```

`XmlSerializer` has its own entirely separate set of requirements. It needs a **public parameterless constructor** - omit it and deserialization throws at runtime with no compile-time warning. It only serializes **public read/write properties** - private fields, computed properties, and anything with only a getter are silently skipped. Output is human-readable and self-describing.

Customization uses `[XmlElement]`, `[XmlAttribute]`, `[XmlRoot]`, and `IXmlSerializable`.

### Mini-Program 2: JSON Serialization

```csharp
var options = new JsonSerializerOptions { WriteIndented = true };
string json = JsonSerializer.Serialize(book, options);
File.WriteAllText(filePath, json);

var restored = JsonSerializer.Deserialize<Book>(json);
```

`Deserialize<T>()` is generic - no cast required. JSON is more compact than XML, equally human-readable, and the dominant format for REST APIs and configuration files. A malicious JSON payload can populate properties but cannot redirect the type system, which is why it's considered safe at trust boundaries in a way `BinaryFormatter` never was.

Customization in `System.Text.Json` uses `[JsonPropertyName]`, `[JsonIgnore]`, and `JsonConverter<T>`.

### Mini-Program 3: Controlling What Survives Serialization

Add a cached derived property to `Book`:

```csharp
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
```

Force-compute `Summary` before serializing to cache it on the original instance. After a JSON or XML round-trip, confirm `CachedSummary` is `null` on the restored instance and `Summary` recomputes it fresh.

`[JsonIgnore]` tells `System.Text.Json` to skip the property entirely during both serialization and deserialization. `[XmlIgnore]` does the same for `XmlSerializer`. The underlying principle - derived or cached values should be recomputed after deserialization, not persisted as potentially stale state - is what `BinaryFormatter`'s `ISerializable` and `[NonSerialized]` were also expressing, just through a different mechanism.

The `BinaryFormatter` version of this used `ISerializable`:

- `GetObjectData(SerializationInfo info, StreamingContext ctx)` decided what got written - `Title`, `Author`, `Year` but not `cachedSummary`.
- A protected deserialization constructor `Book(SerializationInfo info, StreamingContext ctx)` read them back in the same order with the same keys.
- `nameof(Title)` instead of the string `"Title"` meant a rename was caught at compile time.

The mechanism differs; the goal is the same.

---

## Summary: Two Formatters, Same Principle

| | `XmlSerializer` | `System.Text.Json` |
|---|---|---|
| Output | Human-readable XML | Human-readable JSON |
| Opt-in | Public parameterless ctor required | Public read/write props |
| Serializes | Public read/write props only | Public read/write props |
| Skip a property | `[XmlIgnore]` | `[JsonIgnore]` |
| Custom control | `IXmlSerializable` | `JsonConverter<T>` |
| Trust boundary | Safe | Safe |

Each formatter has its own attribute namespace, its own interface for customization, and its own requirements. A class that works perfectly with one may fail silently or throw with another.

`BinaryFormatter` belonged in a third row of this table. It has been omitted from the steps because it is no longer available on the runtime the LessonRunner executes on, and because it was already the wrong choice for new code before that. The project source retains the original implementation for reference when run directly from Visual Studio.

---

## Takeaways

- `XmlSerializer` needs a public parameterless constructor and only touches public read/write properties.
- `JsonSerializer.Deserialize<T>()` is generic - no cast, no type information embedded in the JSON.
- Each serializer has its own attribute namespace. `[XmlIgnore]` and `[JsonIgnore]` are not interchangeable even though they share a name.
- `System.Text.Json` is the BCL default from .NET Core 3.0+; `Newtonsoft.Json` is the established choice for .NET Framework projects. The two APIs are structurally similar.
- Derived or cached values should be recomputed after deserialization, not persisted as potentially stale state.
- `[JsonIgnore]` / `[XmlIgnore]` exclude a property from both serialization and deserialization.
- Use `nameof()` for any string-based serialization keys so renames are caught at compile time rather than at runtime.
- `BinaryFormatter` was removed from .NET 5+ and is a known remote code execution vector. Do not use it in new code. Prefer XML or JSON for anything crossing a trust boundary.
