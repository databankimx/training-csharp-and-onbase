# Ch10 Textbook Code: LINQ Samples

## What This Is

The textbook's own combined Chapter 10 sample project: every topic from the chapter outline (query expressions, method-based queries, joins, grouping, aggregates, `Concat`/`Skip`/`Take`/`Distinct`, and LINQ to XML) as individually-explorable methods, one per concept. Matches the same structural pattern already established for `CSharp.Ch08.TextbookCode.Chapter8` and `CSharp.Ch09.TextbookCode.Chapter9`: `Main()` is entirely empty, `<StartupObject>LINQSamples.Program</StartupObject>` pins it as the real entry point, and every other method in the file is meant to be read directly rather than run.

Worth comparing directly against this chapter's main lesson and Supplementals, which cover the same ground (query vs. method syntax, joins, grouping, deferred execution) in a more curated, actually-runnable form.

---

## A Genuine Bug: `State.Equals()`

```csharp
public bool Equals(State other)
{
    if (Object.ReferenceEquals(this, other)) { return true; }
    else
    {
        if (StateId == other.StateId && StateName == StateName)   // bug
        { return true; }
        else { return false; }
    }
}
```

Two real problems here, worth reading closely:

1. **`StateName == StateName` compares the field to itself.** Not `StateName == other.StateName`, which was clearly intended. `StateName == StateName` is a tautology, always true, so this `Equals()` implementation only ever actually checks `StateId`; the `StateName` half of the condition does nothing at all. Two `State` objects with the same `StateId` but different `StateName` would be reported equal.
2. **No null check on `other`.** If `other` is `null`, `ReferenceEquals(this, other)` correctly returns `false` (since `this` is never null), but the `else` branch then dereferences `other.StateId` directly, throwing `NullReferenceException` rather than returning `false` the way a well-behaved `Equals()` override should.

Neither bug happens to surface in `DistinctCodeLab()`'s specific sample data (every `State` has a distinct `StateId`, and none of the list entries are `null`), which is presumably why this shipped unnoticed. Left exactly as downloaded, since this is unreachable, read-only reference content, but worth recognizing both mistakes if you're implementing `IEquatable<T>` yourself: always compare against `other`'s fields specifically (not the current instance's own field, twice), and always null-check `other` before dereferencing it.

---

## Another Genuine Bug: `LINQToXMLV2()`'s Invalid Cast

```csharp
IEnumerable<XElement> xmlEmployees = (IEnumerable<XElement>)(from e in employees
                                     select e);
```

This selects the `Employee` objects themselves (`select e`, not `select new XElement(...)` the way `LINQToXML()` right above it does), then casts the resulting `IEnumerable<Employee>` directly to `IEnumerable<XElement>`. `Employee` and `XElement` are unrelated types, there's no inheritance relationship and no user-defined conversion between them, so this cast would throw `InvalidCastException` the instant it actually ran. Reads like an incomplete or abandoned edit, most likely meant to build real `XElement` objects the way `LINQToXML()` does just above it, but never finished. Left exactly as downloaded.

---

## Worth Reading: The `Employee` Class Has a Commented-Out Earlier Version

```csharp
//class Employee
//{
//    public string FirstName { get; set; }
//    public string LastName { get; set; }
//    public int StateId { get; set; }     
//}

class Employee
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int StateId { get; set; }
    public string City { get; set; }
    public string State { get; set; }
}
```

Not a bug, just visible leftover history: the active `Employee` class adds `City`/`State` (needed for `CompositeKey()`/`MethodBasedCompositeKey()`'s composite-key join examples) on top of an earlier, simpler version that only had `StateId`. The commented-out original was never cleaned up.
