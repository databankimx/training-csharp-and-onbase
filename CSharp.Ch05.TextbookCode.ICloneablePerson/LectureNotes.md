# Ch05 Textbook Code: ICloneable Person

## What This Is

An interactive WinForms lab demonstrating the shallow-clone gotcha directly, no debugger required. `Person.Clone()` copies the `Manager` reference as-is rather than cloning it, the deep-clone alternative is right there in the code, commented out:

```csharp
public object Clone()
{
    Person person = new Person();
    person.FirstName = FirstName;
    person.LastName = LastName;
    person.Manager = Manager;
    // Uncomment the following for deep clones.
    //if (Manager != null)
    //    person.Manager = (Person)Manager.Clone();
    return person;
}
```

No bugs found.

---

## Watch This One Run

```csharp
Person ann = new Person() { FirstName = "Ann", LastName = "Archer", Manager = null };
Person bob = new Person() { FirstName = "Bob", LastName = "Baker", Manager = ann };
Person bob2 = (Person)bob.Clone();
Person cindy = new Person() { FirstName = "Cindy", LastName = "Cane", Manager = bob };

// Change Bob's manager's name.
bob.Manager.FirstName = "Dan";
bob.Manager.LastName = "Dent";
```

`bob2` is a shallow clone of `bob`, so `bob2.Manager` is the exact same `ann` object `bob.Manager` points at, not a copy of it. When `bob.Manager.FirstName`/`LastName` get changed, that mutates the one shared `Person` object every reference to it points at, `bob.Manager`, `bob2.Manager`, and the `ann` variable itself, since they're all the same object in memory.

The listbox ends up showing **"Dan Dent"**, not "Ann Archer", for the entry added via the `ann` variable. That's the entire lesson in one visible surprise: a shallow clone sharing a mutable child object doesn't just risk unexpected mutation through the clone, it means changes made anywhere in the object graph show up everywhere that graph is referenced, including places that look, at a glance, like they shouldn't be affected at all.

---

## Pairs With CSharp.Ch05.Supplemental.Cloning

That supplemental lesson covers the same shallow-vs-deep distinction in more depth, with `ReferenceEquals()` used explicitly to prove what's shared and what isn't, plus a `List<string>` example showing the same problem isn't unique to nested objects, mutable collections have it too. Worth running both back to back, this one shows the surprise happening, that one shows how to reason about and verify it.
