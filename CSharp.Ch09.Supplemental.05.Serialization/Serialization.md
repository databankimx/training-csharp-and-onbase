# Serialization

## Introduction

Serialization turns an object into a form you can save or send somewhere, bytes, XML text, or JSON text, and deserialization turns it back into an object. This lesson covers all three, plus how to hand-control what actually gets serialized, all using the same simple `Book` class so you can compare the formats directly.

**A security note first**: one of the techniques below, `BinaryFormatter`, has real, documented security risks when used on data from an untrusted source. It's covered because it's part of this lesson's material and still works, but prefer XML or JSON for anything that isn't fully under your own control.

---

## Binary Serialization

```csharp
var formatter = new BinaryFormatter();
formatter.Serialize(stream, book);
```

Compact and fast, but the result is a binary blob, not readable text, and not portable outside .NET.

---

## XML Serialization

```csharp
var serializer = new XmlSerializer(typeof(Book));
serializer.Serialize(stream, book);
```

Produces real, human-readable XML text. Worth knowing: `XmlSerializer` only needs a public parameterless constructor and public properties, it doesn't know or care about anything specific to binary serialization.

---

## JSON Serialization

```csharp
string json = JsonConvert.SerializeObject(book, Formatting.Indented);
var book = JsonConvert.DeserializeObject<Book>(json);
```

JSON is probably the format you'll use most in real work, it's compact, human-readable, and understood by essentially every language and platform, the standard choice for web APIs.

---

## Custom Serialization: Taking Control

```csharp
public class Book : ISerializable
{
    [NonSerialized]
    private string cachedSummary;

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Title), Title);
        info.AddValue(nameof(Author), Author);
        info.AddValue(nameof(Year), Year);
        // cachedSummary is deliberately left out
    }
}
```

Sometimes you don't want *everything* on an object serialized, in this example, `Book` has a cached `Summary` string that's computed from the other properties. Persisting that cached value would be pointless (and risky, if the object could ever change after being restored, the cached summary would be wrong). `ISerializable` lets you explicitly choose what gets written and how it gets read back, here, `Summary` is deliberately excluded and recomputed fresh every time, rather than carried along as stale, persisted state.

Worth knowing: this specific technique only applies to `BinaryFormatter`. `XmlSerializer` and JSON serializers have their own, completely separate ways of being customized, there's no single mechanism that controls all three formats at once.

---

## Try It Yourself

Run the project and compare the printed XML and JSON output for the same `Book` object side by side, notice how much more verbose XML is for the identical data. Then look closely at the Custom Serialization section's output: the original object's `Summary` was already computed before serializing, but the restored object's `Summary` is calculated fresh, proof that the cached value genuinely wasn't carried through.
