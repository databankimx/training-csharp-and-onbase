# Chapter 9 Supplemental 05: Serialization

## What This Is

Serialization is converting an object's state into a storable or transmittable form -- bytes, XML, JSON -- and deserialization is the reverse. Everything here runs against a temporary working directory created on startup and deleted on exit.

A warning belongs at the top of this one:

```
!! WARNING !!
BinaryFormatter has well-documented security problems: deserializing binary data from an
UNTRUSTED source with it can let an attacker execute arbitrary code, simply by handing
your program a maliciously crafted byte stream to deserialize. Microsoft's own guidance
is to avoid it in new code. It's covered here because it's part of this chapter's
official curriculum and still works in classic .NET Framework, but a real application
should prefer XML or JSON serialization for anything crossing a trust boundary.
```

Four serialization approaches, one project, all using the same `Book` class.

---

## How to Write This Program

### Step 1: The Model

Add `Models/Book.cs`. This version is more interesting than the `Book` in the main lesson -- it implements `ISerializable` so it can control exactly what gets written during binary serialization:

```csharp
[Serializable]
public class Book : ISerializable
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int Year { get; set; }

    // NOT written out by GetObjectData() -- recomputed on first access after deserialization.
    [NonSerialized]
    private string cachedSummary;

    public string Summary => cachedSummary ??= $"{Title} by {Author} ({Year})";

    public Book() { }

    public Book(string title, string author, int year)
    {
        Title = title; Author = author; Year = year;
    }

    // Called by BinaryFormatter during deserialization. Reads exactly what GetObjectData()
    // chose to write -- Title, Author, Year -- and leaves cachedSummary unset.
    protected Book(SerializationInfo info, StreamingContext context)
    {
        Title  = info.GetString(nameof(Title));
        Author = info.GetString(nameof(Author));
        Year   = info.GetInt32(nameof(Year));
    }

    // Called by BinaryFormatter during serialization. Chooses exactly what gets written.
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Title),  Title);
        info.AddValue(nameof(Author), Author);
        info.AddValue(nameof(Year),   Year);
        // cachedSummary is deliberately omitted -- it's derived from the other three fields.
    }

    public override string ToString() => Summary;
}
```

### Step 2: The Skeleton

```csharp
private static void Main()
{
    string workingDirectory = Path.Combine(
        Path.GetTempPath(), $"ch09-serialization-demo-{Guid.NewGuid():N}");

    try
    {
        Directory.CreateDirectory(workingDirectory);

        UsingBinarySerialization(workingDirectory);   GenericFunctions.Pause();
        UsingXmlSerialization(workingDirectory);       GenericFunctions.Pause();
        UsingJsonSerialization(workingDirectory);      GenericFunctions.Pause();
        UsingCustomSerialization(workingDirectory);    GenericFunctions.Pause();
    }
    catch (Exception ex)
    {
        new DatabankException("Error Caught!", ex).Log();
        GenericFunctions.Pause();
    }
    finally
    {
        if (Directory.Exists(workingDirectory))
            Directory.Delete(workingDirectory, recursive: true);
        GenericFunctions.Pause(final: true);
    }
}
```

---

### Mini-Program 1: Binary Serialization

Write `UsingBinarySerialization(string workingDirectory)`:

```csharp
private static void UsingBinarySerialization(string workingDirectory)
{
    string filePath = Path.Combine(workingDirectory, "book.bin");
    var book = new Book("1984", "George Orwell", 1949);

    var formatter = new BinaryFormatter();

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        formatter.Serialize(stream, book);
    }
    Console.WriteLine($"BinaryFormatter wrote {new FileInfo(filePath).Length} bytes to {Path.GetFileName(filePath)}");

    Book restoredBook;
    using (var stream = new FileStream(filePath, FileMode.Open))
    {
        restoredBook = (Book)formatter.Deserialize(stream);
    }
    Console.WriteLine($"Deserialized: {restoredBook}");
}
```

Run it. The binary file is small and unreadable in a text editor -- that's the format. It's compact and fast, but the bytes are meaningless without the exact same class definition that produced them.

`[Serializable]` on the class is the opt-in. Everything public and private gets serialized automatically, unless marked `[NonSerialized]`. `BinaryFormatter.Serialize()` writes; `BinaryFormatter.Deserialize()` reads; both take a `Stream`. The cast to `Book` is required because `Deserialize` returns `object`.

The security warning is real. Never use `BinaryFormatter` to deserialize data from a source you don't fully control.

### Mini-Program 2: XML Serialization

Write `UsingXmlSerialization(string workingDirectory)`:

```csharp
private static void UsingXmlSerialization(string workingDirectory)
{
    string filePath = Path.Combine(workingDirectory, "book.xml");
    var book = new Book("Brave New World", "Aldous Huxley", 1932);

    // XmlSerializer requirements: a PUBLIC parameterless constructor, and it only
    // serializes PUBLIC read/write properties. [Serializable] and [NonSerialized]
    // are irrelevant to XmlSerializer -- they're BinaryFormatter concepts.
    var serializer = new XmlSerializer(typeof(Book));

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        serializer.Serialize(stream, book);
    }

    Console.WriteLine("Generated XML:");
    Console.WriteLine(File.ReadAllText(filePath));

    Book restoredBook;
    using (var stream = new FileStream(filePath, FileMode.Open))
    {
        restoredBook = (Book)serializer.Deserialize(stream);
    }
    Console.WriteLine($"Deserialized: {restoredBook}");
}
```

Run it. The XML file is human-readable and you can open it in a text editor. That's the key difference from binary -- it's self-describing and debuggable.

`XmlSerializer` has its own entirely separate set of requirements from `BinaryFormatter`. It needs a **public parameterless constructor** (which is why `Book` has one). It only serializes **public read/write properties** -- private fields, computed properties, and anything with only a getter are skipped. `[Serializable]` and `[NonSerialized]` mean nothing to `XmlSerializer`.

Customizing XML output (element names, namespaces, attribute vs. element) uses `[XmlElement]`, `[XmlAttribute]`, `[XmlRoot]`, and `IXmlSerializable` -- a completely different set of attributes from the binary formatter's world.

### Mini-Program 3: JSON Serialization

Write `UsingJsonSerialization(string workingDirectory)`:

```csharp
private static void UsingJsonSerialization(string workingDirectory)
{
    string filePath = Path.Combine(workingDirectory, "book.json");
    var book = new Book("Fahrenheit 451", "Ray Bradbury", 1953);

    // Newtonsoft.Json (Json.NET) -- the long-established JSON library for .NET Framework.
    // Serializes public read/write properties by default, similar to XmlSerializer.
    string json = JsonConvert.SerializeObject(book, Formatting.Indented);
    File.WriteAllText(filePath, json);

    Console.WriteLine("Generated JSON:");
    Console.WriteLine(json);

    var restoredBook = JsonConvert.DeserializeObject<Book>(json);
    Console.WriteLine($"Deserialized: {restoredBook}");
}
```

Run it. JSON is more compact than XML, equally human-readable, and the dominant format for REST APIs and configuration files.

`JsonConvert.SerializeObject()` / `JsonConvert.DeserializeObject<T>()` are the simplest Newtonsoft.Json entry points. `Formatting.Indented` produces pretty-printed output; omit it for compact, single-line JSON suitable for transmission.

`DeserializeObject<T>()` is generic -- no cast required. The type information stays in the application rather than being embedded in the JSON itself, which is why JSON serialization is generally considered safer than binary: a malicious JSON payload can populate properties but can't redirect the type system.

Customization uses `[JsonProperty]`, `[JsonIgnore]`, and `JsonConverter` -- again, a completely separate attribute namespace from the other two formatters.

### Mini-Program 4: Custom Serialization (ISerializable)

Write `UsingCustomSerialization(string workingDirectory)`:

```csharp
private static void UsingCustomSerialization(string workingDirectory)
{
    string filePath = Path.Combine(workingDirectory, "custom-book.bin");
    var book = new Book("Dune", "Frank Herbert", 1965);

    // Read Summary now to cache it on THIS instance before serializing.
    // The point of this demo is that the cached value will NOT be carried through.
    Console.WriteLine($"Original Summary (cached): {book.Summary}");

    var formatter = new BinaryFormatter();
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        formatter.Serialize(stream, book);
    }

    Book restoredBook;
    using (var stream = new FileStream(filePath, FileMode.Open))
    {
        restoredBook = (Book)formatter.Deserialize(stream);
    }

    // GetObjectData() never wrote cachedSummary out. Summary recomputes it lazily
    // the first time it's read on the restored instance.
    Console.WriteLine($"Restored Title/Author/Year: {restoredBook.Title}, {restoredBook.Author}, {restoredBook.Year}");
    Console.WriteLine($"Restored Summary (freshly recomputed): {restoredBook.Summary}");
}
```

Run it. The cached value from before serialization is gone; the restored instance recomputes it from `Title`, `Author`, and `Year`.

`ISerializable` gives you complete control over what `BinaryFormatter` writes and reads. Two pieces are required: `GetObjectData()` (writes) and the protected deserialization constructor `Book(SerializationInfo info, StreamingContext context)` (reads). The formatter calls them at the appropriate times.

`[NonSerialized]` on `cachedSummary` would skip it during automatic serialization -- but here `GetObjectData()` takes over entirely and `cachedSummary` is never mentioned. The result is the same: derived state doesn't get persisted. Any time you have a cached value, a computed property, or a field that might go stale, this is the pattern that keeps serialized data clean.

The deserialization constructor must read back **in the same order** and with **the same keys** as `GetObjectData()` wrote. Using `nameof(Title)` instead of the string `"Title"` means a rename refactoring catches the mismatch at compile time rather than at runtime.

---

## Three Formatters, Three Different Rules

| | `BinaryFormatter` | `XmlSerializer` | `Newtonsoft.Json` |
|---|---|---|---|
| Output | Compact binary | Human-readable XML | Human-readable JSON |
| Opt-in | `[Serializable]` | Public parameterless ctor | Public read/write props |
| Serializes | Public + private fields | Public read/write props only | Public read/write props |
| Skip a field | `[NonSerialized]` | No direct equivalent | `[JsonIgnore]` |
| Custom control | `ISerializable` | `IXmlSerializable` | `JsonConverter` |
| Trust boundary | **Never use with untrusted data** | Safe | Safe |

Each formatter has its own attribute namespace, its own interface for customization, and its own set of requirements. A class that works perfectly with one may fail silently or throw with another.

---

## Takeaways

- `[Serializable]` opts a class into `BinaryFormatter` serialization. `[NonSerialized]` skips a field.
- Never deserialize untrusted binary data with `BinaryFormatter`. It's a remote code execution vector.
- `XmlSerializer` needs a public parameterless constructor and only touches public read/write properties. `[Serializable]` is irrelevant to it.
- `JsonConvert.DeserializeObject<T>()` is generic -- no cast, no embedded type information in the JSON.
- `ISerializable` gives complete control over what `BinaryFormatter` writes and reads. Implement `GetObjectData()` and the protected deserialization constructor.
- Use `nameof()` for serialization keys so renames are caught at compile time.
- Derived or cached values should be recomputed after deserialization, not persisted as potentially stale state.
