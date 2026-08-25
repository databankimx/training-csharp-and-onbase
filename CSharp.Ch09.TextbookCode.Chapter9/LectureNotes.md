# Ch09 Textbook Code: Chapter 9

## What This Is

The textbook's own combined Chapter 9 sample project, covering collections (arrays, `ArrayList`, `List<T>`, `Dictionary<TKey,TValue>`, `Hashtable`, `Queue`, `Stack`, `SortedList`, and a custom `CollectionBase`-derived collection), file I/O, and ADO.NET, one topic per file. Matches the exact same structural pattern already seen in Chapters 7 and 8: `Program.cs`'s `Main()` (the actual `StartupObject`) is entirely empty, and every other file carries its own separate, unreachable `Main()`, meant to be read and individually explored rather than run end to end.

---

## `Main()` Is Genuinely Empty, By Design

Ten other files in this project (`ArrayListSamples.cs`, `CustomCollectionSamples.cs`, `DictionarySamples.cs`, `HashTableSamples.cs`, `IOSamples.cs`, `ListSamples.cs`, `QueueSamples.cs`, `SortedListSamples.cs`, `StackSamples.cs`, `ADONETSamples.cs`) each declare their own `static void Main(string[] args)`. None of them are the actual entry point, `<StartupObject>Chapter9.Program</StartupObject>` in the `.csproj` pins that to `Program.cs`'s empty one specifically. Running this project via `LessonRunner` shows a blank console that starts and exits instantly, exactly as designed, the value is in reading each file directly. `ArraySamples.cs` doesn't even have a `Main()`, its two methods (`Sample1()`/`Sample2()`) are private instance methods on a class nobody instantiates, the most purely "read this, don't run it" file in the set.

---

## `IOSamples.cs`: A Genuine Bug, In Unreachable Code

```csharp
FileStream fileStream = new FileStream(@"c:\Chapter9Samples\Numbers.txt", FileMode.Truncate, FileAccess.Write, FileShare.None);
```

Worth knowing even though this code never runs automatically: `FileMode.Truncate` requires the target file to **already exist**, it opens and empties an existing file rather than creating one. If `c:\Chapter9Samples\Numbers.txt` doesn't already exist (and nothing in this file creates it first), this throws `FileNotFoundException` the moment it's reached. `FileMode.Create` (used later in the same file, for `BinaryWriter.txt`) is the one that creates a new file or overwrites an existing one, that's almost certainly what was intended here too. Left exactly as downloaded rather than fixed, since it's genuinely unreachable, but worth recognizing `Truncate` vs. `Create`'s different preconditions if you ever reach for this API yourself.

Every path in this file is also hardcoded to `c:\Chapter9Samples\...`, a folder that doesn't exist in this environment. If you ever want to actually run this file's contents (by temporarily changing `StartupObject`, or copy-pasting into a runnable method), create that folder first, and be aware `Numbers.txt`/`HelloWorld.txt` would need to already exist too given the `Truncate` bug above.

---

## `ADONETSamples.cs`: The Same `Reflection` Database as Chapter 8

```csharp
cn.ConnectionString = "Server = (local); Database = Reflection; Trusted_Connection = True;";
```

Every method in this file targets a `Reflection` database with a `Person` table, the exact same target already established as unreachable dead code in `CSharp.Ch08.TextbookCode.Chapter8`'s `Person.GetPerson()`. Nothing here is any more reachable than that was, `ADONETSamples.Main()` is empty, and none of its other methods (`OpenConnection()`, `CommandExecuteReader()`, `DataSetInsert()`, etc.) are ever called. Worth reading through regardless: it's a genuinely thorough tour of raw ADO.NET, `ExecuteNonQuery()` (both plain text and stored procedure forms), `ExecuteReader()`, `ExecuteScalar()`, `ExecuteXmlReader()`, and `SqlDataAdapter`-driven `DataSet` population, insert, update, and delete, worth comparing against the more curated, actually-runnable version of the same ground covered in `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework`.

---

## The Custom Collection: `PersonCollection : CollectionBase`

```csharp
class PersonCollection : CollectionBase
{
    public void Add(Person person) => List.Add(person);
    public Person this[int index] { get => (Person)List[index]; set => List[index] = value; }
    ...
}
```

Worth comparing directly against `BoundedCollection<T>` from the main Chapter 9 lesson (`CSharp.Ch09.WorkingWithDataCollections`). `CollectionBase` is the older, pre-generics way of building a custom, strongly-typed collection, it wraps a plain (non-generic) internal list and requires hand-writing every strongly-typed member (`Add(Person)`, the `Person`-typed indexer) yourself, with casts (`(Person)List[index]`) sprinkled throughout. `BoundedCollection<T>` achieves the same goal, a custom collection with its own rules, far more concisely by implementing the generic `ICollection<T>` instead, no casting required anywhere, and works with any element type rather than being hand-written for `Person` specifically. `CollectionBase` still shows up in older code, worth recognizing it, but there's no reason to reach for it in anything new.
