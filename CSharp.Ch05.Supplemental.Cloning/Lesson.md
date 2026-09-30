# Chapter 5 Supplemental: Shallow and Deep Cloning

## What This Is About

The main Chapter 5 lesson mentions cloning in passing. This project slows it down and makes each case observable, because "I copied it and the original changed anyway" is a bug people hit repeatedly before the distinction actually clicks.

---

## How to Write This Program

Three mini-programs, three fundamentally different outcomes, all from the same `Person`. Build each one, run it, make sure you understand the output before moving on.

### Step 1: Build the Supporting Types

These need to exist before any of the demos can run. Put them alongside `Main()`:

```csharp
internal class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }

    public Address DeepClone() => new Address { Street = Street, City = City, State = State };

    public override string ToString() => $"{Street}, {City}, {State}";
}

internal class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Address HomeAddress { get; set; }
    public List<string> Skills { get; set; } = new List<string>();

    public Person ShallowClone() => (Person)MemberwiseClone();

    public Person DeepClone() => new Person
    {
        Name = Name,
        Age = Age,
        HomeAddress = HomeAddress?.DeepClone(),
        Skills = Skills == null ? null : new List<string>(Skills)
    };
}
```

Four properties, chosen deliberately to cover every behavior category: a value type (`int`), an immutable reference type (`string`), a mutable reference type (`Address`), and a mutable collection (`List<string>`). Each one behaves differently under a shallow clone, which is the entire point of the exercise.

### Mini-Program 1: Reference Assignment (Not a Clone at All)

```csharp
var original = new Person
{
    Name = "Ada Lovelace",
    Age = 36,
    HomeAddress = new Address { Street = "123 Example Street", City = "London", State = "England" },
    Skills = new List<string> { "Mathematics", "Programming" }
};

Person assigned = original;

Console.WriteLine(ReferenceEquals(original, assigned)); // True
assigned.Name = "Changed";
Console.WriteLine(original.Name); // "Changed" too -- same object
```

Run it. `True`, then `Changed`.

`assigned = original` doesn't copy anything. Both variables point at the exact same `Person` object. Changing anything through `assigned` changes what you see through `original`, because there's only one object. This isn't cloning at all -- it's the Chapter 3 reference-type behavior. It's included here as the baseline, because it's exactly what people accidentally write when they mean to copy.

### Mini-Program 2: Shallow Clone

```csharp
var original = new Person
{
    Name = "Ada Lovelace",
    Age = 36,
    HomeAddress = new Address { Street = "123 Example Street", City = "London", State = "England" },
    Skills = new List<string> { "Mathematics", "Programming" }
};

Person shallow = original.ShallowClone();

Console.WriteLine(ReferenceEquals(original, shallow));                 // False - new outer object
Console.WriteLine(ReferenceEquals(original.HomeAddress, shallow.HomeAddress)); // True - shared!
Console.WriteLine(ReferenceEquals(original.Skills, shallow.Skills));   // True - shared!

shallow.Name = "Shallow Copy";
shallow.HomeAddress.City = "Chicago";
shallow.Skills.Add("Shared-list surprise");

Console.WriteLine(original.Name);                 // Ada Lovelace - unchanged
Console.WriteLine(original.HomeAddress.City);     // Chicago - changed!
Console.WriteLine(original.Skills.Count);         // 3 - changed!
```

Run it. `False`/`True`/`True`, then `Ada Lovelace`, then `Chicago`, then `3`.

`MemberwiseClone()` creates a new outer `Person`, but copies each field as-is. `int` (`Age`) is copied by value -- the clone owns its own number. `string` (`Name`) is a reference type but immutable -- assigning `shallow.Name = "Shallow Copy"` replaces the property value in the clone, it doesn't modify the original string object, so `original.Name` is unaffected. `Address` and `List<string>` are mutable reference types -- the clone holds the same reference the original does, so mutating through either one is visible from both.

This is the crux of the whole lesson. "New wrapper, same contents" looks copied until someone mutates a child. That's the middle ground where the confusion lives.

### Mini-Program 3: Deep Clone

```csharp
var original = new Person
{
    Name = "Ada Lovelace",
    Age = 36,
    HomeAddress = new Address { Street = "123 Example Street", City = "London", State = "England" },
    Skills = new List<string> { "Mathematics", "Programming" }
};

Person deep = original.DeepClone();

Console.WriteLine(ReferenceEquals(original, deep));                    // False
Console.WriteLine(ReferenceEquals(original.HomeAddress, deep.HomeAddress)); // False - independent!
Console.WriteLine(ReferenceEquals(original.Skills, deep.Skills));      // False - independent!

deep.Name = "Deep Copy";
deep.HomeAddress.City = "Chicago";
deep.Skills.Add("Independent list");

Console.WriteLine(original.Name);                 // Ada Lovelace - unchanged
Console.WriteLine(original.HomeAddress.City);     // London - unchanged
Console.WriteLine(original.Skills.Count);         // 2 - unchanged
Console.WriteLine(deep.HomeAddress.City);         // Chicago - only on the clone
Console.WriteLine(deep.Skills.Count);             // 3 - only on the clone
```

Run it. Three `False` values, then five independent outputs showing original and clone have gone their separate ways.

```csharp
public Person DeepClone() => new Person
{
    Name = Name,
    Age = Age,
    HomeAddress = HomeAddress?.DeepClone(), // Address clones itself
    Skills = Skills == null ? null : new List<string>(Skills)
};
```

Three things worth noticing in `DeepClone()`:

- **`HomeAddress?.DeepClone()`** -- null-conditional, so a null address stays null instead of throwing. And `Address` is responsible for cloning its own fields -- deep cloning is inherently recursive; each type in the graph handles its own layer.
- **`new List<string>(Skills)`** -- the copy constructor produces a genuinely new list. This is a *shallow* copy of the list, which is safe here only because `string` is immutable. A `List<Address>` would need each element cloned individually.
- **`Skills == null ? null : ...`** -- preserving null rather than silently substituting an empty list. A clone should reproduce the source's state, including the parts that are absent.

---

## The Main Rule

The question isn't "is this property a reference type." The question is: **does the clone share mutable state with the source?**

Immutable types (`string`, all value types) are always safe under a shallow clone. Mutable reference types (`Address`, `List<string>`) are not, unless sharing them is actually intentional.

That's also why immutability is so valuable: if `Address` were immutable, shallow cloning would be sufficient for it, and the whole shallow-versus-deep question would stop applying to it. Making types immutable where practical removes an entire category of bug.

## Takeaways

- Assignment copies a reference. It's not a copy of anything.
- `MemberwiseClone()` is always shallow -- new wrapper, shared contents.
- Value types and immutable types are safe under a shallow clone; mutable reference types are not.
- Deep cloning is recursive -- each type clones its own layer.
- Don't deep clone reflexively. Some references are meant to stay shared.
