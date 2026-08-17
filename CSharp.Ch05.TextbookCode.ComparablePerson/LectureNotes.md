# Ch05 Textbook Code: Comparable Person

## What This Is

A small, genuinely interactive WinForms lab: a `Person` class implementing `IComparable<Person>` by full name (`ToString().CompareTo(other.ToString())`), with an unsorted list box next to a sorted one so the effect of `Array.Sort()` is visible side by side.

No bugs found, `Load` is correctly wired this time. Six fictional names (Fred Franklin, Cindy Carter, Dan Dent, Ben Baker, Eva Eager, Ann Able), no personal data.

---

## Worth Noticing

`CompareTo()` here delegates entirely to `ToString()` rather than comparing `FirstName`/`LastName` directly:

```csharp
public override string ToString()
{
    return FirstName + " " + LastName;
}

public int CompareTo(Person other)
{
    return ToString().CompareTo(other.ToString());
}
```

That's a small but reusable pattern, once a type has a sensible `ToString()`, `IComparable` can often just piggyback on it rather than duplicating comparison logic across two properties. The tradeoff is that it sorts by the *combined* string ("Ann Able" before "Ben Baker"), not independently by last name then first name the way many real address-book-style sorts would want. Worth noticing before copying this pattern into something that actually needs last-name-first sorting.
