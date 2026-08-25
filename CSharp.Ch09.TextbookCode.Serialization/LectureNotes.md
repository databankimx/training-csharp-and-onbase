# Ch09 Textbook Code: Serialization

## What This Is

A genuinely runnable console demo (unlike `Chapter9`'s dead-code collection), the same three serialization formats covered in `CSharp.Ch09.Supplemental.05.Serialization` (Binary, XML, JSON), applied to a `Person` class that also implements `ISerializable` for custom control over the binary form. Writes and reads back `Person.bin`, `Person.xml`, and `Person.json` in the working directory. Three real bugs found and fixed, all variations on the same theme: a stream left open when it should have been closed.

---

## Bug 1: The XML Write Was Never Closed Before the Read Tried to Open the Same File

```csharp
XmlSerializer xmlSerializer = new XmlSerializer(typeof(Person));
StreamWriter streamWriter = new StreamWriter("Person.xml");
xmlSerializer.Serialize(streamWriter, person);
// streamWriter was never closed here

XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(Person));
FileStream fs = new FileStream("Person.xml", FileMode.Open);   // tries to open the SAME file
```

This is the most serious of the three: `StreamWriter` buffers its output internally and holds an exclusive lock on the underlying file until it's closed. With `streamWriter` still open, the very next lines try to open that same `Person.xml` file again, for reading, this collision is exactly the kind of thing that throws `IOException: The process cannot access the file 'Person.xml' because it is being used by another process.` Even setting the crash aside, any buffered-but-unflushed content wouldn't have made it to disk yet either way, so the file being read could have been incomplete. **Fixed** by adding `streamWriter.Close();` immediately after the `Serialize()` call, before `fs` is ever opened.

---

## Bug 2: Closing the Wrong Variable

```csharp
Stream stream3 = new FileStream("Person.json", FileMode.Open);
DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(Person));
person = (Person)ser2.ReadObject(stream3);
stream.Close();   // should be stream3.Close()
```

`stream` here refers to the binary-serialization stream from earlier in `Main()`, already closed at that point, so this line was a harmless no-op. The real problem is what it *didn't* do: `stream3`, the JSON read stream opened right above, was never closed at all, a genuine resource leak. This reads like a copy-paste mistake, every other stream in this file is correctly closed by its own matching variable name, this was the one exception. **Fixed** by changing it to `stream3.Close();`.

---

## Bug 3: The XML Read Stream, Also Never Closed

```csharp
FileStream fs = new FileStream("Person.xml", FileMode.Open);
Person person4 = (Person)xmlSerializer2.Deserialize(fs);
// fs was never closed here either
```

Same class of issue as Bug 2, `fs` is opened and used but never closed. Less severe than Bug 1 (nothing else in this file tries to re-open `Person.xml` afterward, so it never caused an observable crash), but still a genuine leak. **Fixed** by adding `fs.Close();` for consistency with every other stream in the file.

---

## Worth Noticing: The Real Lesson Behind All Three

Every one of these three bugs is the same underlying mistake in a different spot: a stream or writer opened without a matching, guaranteed close. `using` statements (or, for `IAsyncDisposable` types, `await using`) exist specifically to make this class of bug structurally difficult to write, the compiler enforces that a `using`-wrapped resource gets disposed no matter how the block exits, including via an exception partway through, which manual `.Close()` calls (as this whole file relies on) simply don't guarantee. Compare this file's style against `CSharp.Ch09.Supplemental.05.Serialization`'s `using` blocks throughout, three real bugs' worth of evidence for why that stylistic difference matters in practice, not just in theory.

---

## Worth Noticing: `_id` Is Set but Never Actually Observable

```csharp
private int _id;
public void SetId(int id) { _id = id; }
```

`_id` has a setter method (`SetId()`) but no getter anywhere, and `GetObjectData()` doesn't include it either:

```csharp
public void GetObjectData(SerializationInfo info, StreamingContext context)
{
    info.AddValue("custom field 1", FirstName);
    info.AddValue("custom field 2", LastName);
}
```

`Main()` calls `person.SetId(1)` faithfully every time a `Person` is constructed, but nothing in this file, or in `Person` itself, ever reads `_id` back. It's set, deliberately excluded from binary serialization (matching the same `ISerializable`-driven selective-persistence pattern demonstrated more explicitly in `CSharp.Ch09.Supplemental.05.Serialization`'s `Book.Summary`), and then simply... never observed again. Not a bug exactly, `_id` genuinely isn't wired to anything that would need it in this particular demo, but worth noticing as a loose end if you're reading this file closely.
