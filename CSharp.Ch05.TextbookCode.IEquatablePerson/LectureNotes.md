# Ch05 Textbook Code: IEquatable Person

## What This Is

A deliberately narrow interactive lab: two text boxes (First Name, Last Name) and an Add button. Type a name, click Add. Type the exact same name again, click Add again, and a message box tells you the list already contains that person, instead of silently adding a duplicate.

No bugs found in the sense of "this demo behaves incorrectly." Worth knowing about, not fixing (see below).

---

## Worth Actually Clicking Through

There's no visible list control here, no way to see everyone you've added. That's on purpose, the entire lesson is contained in one comparison: `List<Person>.Contains(person)`. Add "Jane Doe," it goes in silently. Add "Jane Doe" again, message box. Add "Jane Smith," goes in silently, different person. The absence of a visible list is what keeps the demo focused entirely on the one behavior being taught.

```csharp
if (People.Contains(person))
{
    MessageBox.Show("The list already contains this person.");
}
else
{
    People.Add(person);
    ...
}
```

`Contains()` is only able to recognize "Jane Doe" as a duplicate because `Person` implements `IEquatable<Person>`. Without it, `Contains()` would fall back to reference equality, and every freshly-constructed `Person` (even with identical First/Last Name) would count as a brand new, distinct entry.

---

## Worth Knowing (Not a Bug in This Demo)

```csharp
public bool Equals(Person other)
{
    return ((FirstName == other.FirstName) &&
            (LastName == other.LastName));
}
```

`Equals()` never checks whether `other` is `null` before reading `other.FirstName`. Passed a `null`, this throws `NullReferenceException` instead of correctly returning `false` (the conventional behavior for `Equals(null)`). It doesn't come up in this specific demo, `person` is always a freshly-constructed, non-null object every time `Contains()` is called, so the unsafe path is never actually reached here. Worth knowing as a general pattern to watch for if this code (or something shaped like it) ever gets reused somewhere `Equals()` might legitimately be called with `null`.
