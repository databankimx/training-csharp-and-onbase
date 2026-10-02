# Chapter 5 Supplemental: Shallow and Deep Cloning

## What This Is

The main lesson's `ICloneable` section briefly distinguished shallow from deep cloning. This project slows that distinction way down and makes it observable at each step, using `ReferenceEquals` to prove concretely which objects are shared and which are independent.

The `Person` model here has a nested `Address` (a reference type) and a `List<string>` of skills (also a reference type). That's the setup that makes the shallow/deep distinction matter -- a model with only value-type fields would look identical under both approaches.

---

## How to Write This Program

The types live in `Models/`. `Person` provides both a `ShallowClone()` method (using `MemberwiseClone()`) and a `DeepClone()` method (recursively cloning nested reference types). `Address` provides its own `DeepClone()`. Before running, work through Exercise 1 at the bottom of this file and predict each `ReferenceEquals` result. Then run and compare.

### Mini-Program 1: Reference Assignment

Create a `Person`, assign it to a second variable, and prove with `ReferenceEquals` that no clone occurred:

```csharp
Person original = new Person
{
    Name = "Ada Lovelace",
    Age = 36,
    HomeAddress = new Address { Street = "123 Example Street", City = "London", State = "England" },
    Skills = new List<string> { "Mathematics", "Programming" }
};

Person assigned = original;
Console.WriteLine(ReferenceEquals(original, assigned)); // True -- same object, no clone
```

**Run it.** Both variables point at the same `Person`. Any mutation through either variable affects the same object.

### Mini-Program 2: Shallow Clone

Call `ShallowClone()` and use `ReferenceEquals` to reveal which parts of the graph are shared:

```csharp
Person shallow = original.ShallowClone();

Console.WriteLine(ReferenceEquals(original, shallow));                         // False -- new Person
Console.WriteLine(ReferenceEquals(original.HomeAddress, shallow.HomeAddress)); // True  -- shared
Console.WriteLine(ReferenceEquals(original.Skills, shallow.Skills));           // True  -- shared
```

Now demonstrate the consequence by mutating the clone's nested state:

```csharp
shallow.Name = "Shallow Copy";            // replaces the reference -- original.Name unchanged
shallow.HomeAddress.City = "Chicago";     // mutates the shared Address -- original sees it
shallow.Skills.Add("Shared-list surprise"); // mutates the shared List -- original sees it

Console.WriteLine(original.Name);              // Ada Lovelace  -- unaffected
Console.WriteLine(original.HomeAddress.City);  // Chicago       -- changed!
Console.WriteLine(original.Skills.Count);      // 3             -- changed!
```

**Run it.** `Name` was unaffected because assigning a string replaces the clone's reference rather than mutating the original string. `City` and `Skills` changed because the nested `Address` and `List<string>` are shared objects.

### Mini-Program 3: Deep Clone

Call `DeepClone()` and prove every level of the graph is independent:

```csharp
Person deep = original.DeepClone();

Console.WriteLine(ReferenceEquals(original, deep));                         // False
Console.WriteLine(ReferenceEquals(original.HomeAddress, deep.HomeAddress)); // False -- independent
Console.WriteLine(ReferenceEquals(original.Skills, deep.Skills));           // False -- independent
```

Mutate the deep clone and confirm the original is untouched:

```csharp
deep.HomeAddress.City = "Chicago";
deep.Skills.Add("Independent list");

Console.WriteLine(original.HomeAddress.City); // London -- unaffected
Console.WriteLine(original.Skills.Count);     // 2      -- unaffected
Console.WriteLine(deep.HomeAddress.City);     // Chicago
Console.WriteLine(deep.Skills.Count);         // 3
```

**Run it.** The deep clone owns independent copies of every mutable child object. No changes to the clone affect the original.

---

## Reference Assignment Is Not a Clone

```csharp
Person assigned = original;
```

There is only one `Person` object. Both variables point to it:

```
original ----+
             +----> Person
assigned ----+
```

```csharp
Console.WriteLine(ReferenceEquals(original, assigned)); // True
```

Any mutation made through either variable mutates the same object. This is the baseline.

---

## Shallow Clone

`ShallowClone()` exposes `MemberwiseClone()`:

```csharp
public Person ShallowClone()
{
    return (Person)MemberwiseClone();
}
```

`MemberwiseClone()` creates a new outer object and copies each field from the source into it. For value-type fields (`Age`, `Name`), the value itself is copied -- independent. For reference-type fields (`HomeAddress`, `Skills`), the reference is copied -- not the object it points to:

```
original ----> Person A ----> Address X
                         `---> List X

shallow  ----> Person B ----> Address X   <- same Address
                         `---> List X     <- same List
```

```csharp
Person shallow = original.ShallowClone();

Console.WriteLine(ReferenceEquals(original, shallow));                          // False -- new Person
Console.WriteLine(ReferenceEquals(original.HomeAddress, shallow.HomeAddress));  // True  -- shared
Console.WriteLine(ReferenceEquals(original.Skills, shallow.Skills));            // True  -- shared
```

The consequence:

```csharp
shallow.HomeAddress.City = "Chicago";
// original.HomeAddress.City is now "Chicago" too -- same object

shallow.Skills.Add("New Skill");
// original.Skills now contains "New Skill" too -- same list
```

### Why strings don't cause the same surprise

`string` is a reference type, so technically a shallow clone copies the string reference too. But strings are immutable. Assigning `shallow.Name = "x"` replaces the clone's reference with a reference to a different string -- it doesn't mutate the original string object. The original's `Name` is untouched. The practical cloning concern is always **shared mutable reference state**, and strings aren't mutable.

---

## Deep Clone

A deep clone explicitly constructs independent copies of every nested mutable reference type:

```csharp
public Person DeepClone()
{
    return new Person
    {
        Name = Name,
        Age = Age,
        HomeAddress = HomeAddress?.DeepClone(),
        Skills = Skills == null ? null : new List<string>(Skills)
    };
}

// Address.DeepClone():
public Address DeepClone()
{
    return new Address { Street = Street, City = City, State = State };
}
```

Now the graph is fully independent:

```
original ----> Person A ----> Address X
                         `---> List X

deep     ----> Person B ----> Address Y   <- independent copy
                         `---> List Y     <- independent copy
```

```csharp
Person deep = original.DeepClone();

Console.WriteLine(ReferenceEquals(original, deep));                          // False
Console.WriteLine(ReferenceEquals(original.HomeAddress, deep.HomeAddress));  // False -- independent
Console.WriteLine(ReferenceEquals(original.Skills, deep.Skills));            // False -- independent
```

Mutating `deep.HomeAddress.City` leaves `original.HomeAddress.City` untouched.

---

## Shallow vs. Deep at a Glance

| Question | Shallow Clone | Deep Clone |
|---|---|---|
| New outer object? | Yes | Yes |
| Value-type fields independent? | Yes | Yes |
| Nested mutable references shared? | Yes | No (if implemented correctly) |
| Safe for independent mutation? | Not necessarily | Yes |
| Faster/simpler to implement? | Usually | Usually not |

---

## Deep Clone Does Not Mean "Blindly Duplicate Everything"

Suppose a class contains:

```csharp
public ILogger Logger { get; set; }
public SqlConnection Connection { get; set; }
public Address HomeAddress { get; set; }
```

An independent copy of `HomeAddress` makes sense. Duplicating a logger or a live database connection almost certainly doesn't -- the clone would be holding a second reference to the same infrastructure resource, not its own independent one.

A deep clone is best understood as:

> Create independent copies of the mutable state that the object logically **owns**.

It is not "recursively copy every reference regardless of what it represents."

---

## The `ICloneable` Problem

You may encounter `ICloneable`:

```csharp
public interface ICloneable
{
    object Clone();
}
```

The interface does **not** specify whether `Clone()` must be shallow or deep. A caller has to know the implementation's specific behavior, which defeats much of the purpose of an interface. Explicit methods communicate intent clearly:

```csharp
person.ShallowClone();
person.DeepClone();
```

A copy constructor is another clear option:

```csharp
public Person(Person source)
{
    Name = source.Name;
    Age = source.Age;
    HomeAddress = source.HomeAddress?.DeepClone();
    Skills = source.Skills == null ? null : new List<string>(source.Skills);
}
```

---

## Serialization-Based Cloning

Another approach sometimes shown in examples: serialize the object and deserialize it into a new instance. This can produce independent object graphs, but carries real tradeoffs:

- Every type in the graph must be supported by the serializer.
- Serialization is often significantly more expensive than explicit copying.
- Constructors, private state, polymorphism, and resource types (file handles, connections) complicate the result unpredictably.
- It hides which references should actually be shared versus independently copied.

For a small, well-known domain model, explicit copy logic is easier to read and reason about.

---

## A Maintenance Risk Worth Naming

When a model gains new mutable reference-type properties, its deep-copy logic may also need to change. Nothing in the compiler or the interface enforces this -- `DeepClone()` will still compile and run after you add `EmergencyContact` to `Person`, it just won't clone the new field. This is a silent correctness failure.

**Exercise:** add an `EmergencyContact` property to `Person` without updating `DeepClone()`. Run the program and prove via `ReferenceEquals` that the "deep" clone still shares the contact. Then update `DeepClone()` to fix it.

---

## Exercises

**Exercise 1 -- Predict before running.** Before running the program, predict the result of each of these:

```csharp
ReferenceEquals(original, shallow)
ReferenceEquals(original.HomeAddress, shallow.HomeAddress)
ReferenceEquals(original.Skills, shallow.Skills)

ReferenceEquals(original, deep)
ReferenceEquals(original.HomeAddress, deep.HomeAddress)
ReferenceEquals(original.Skills, deep.Skills)
```

Then run and compare.

**Exercise 2 -- Add a mutable child.** Add `EmergencyContact` (name + phone number) to `Person`. First update only the model, not `DeepClone()`. Prove the deep clone still shares the contact. Then fix it.

---

## Takeaways

- Reference assignment copies the reference, not the object. Both variables point at the same instance.
- `MemberwiseClone()` creates a shallow clone: new outer object, shared nested mutable reference-type fields.
- A deep clone must explicitly construct new instances of every nested reference type the object owns.
- Strings are reference types, but their immutability makes shared references safe under assignment.
- `ReferenceEquals` is the definitive test: `false` means genuinely independent objects; `true` means shared.
- Deep clone reflects ownership semantics, not "recursively copy everything."
- `ICloneable.Clone()` is ambiguous about depth -- explicit method names or a copy constructor communicate intent more clearly.
- Serialization-based cloning works but hides ownership decisions and carries real performance and correctness tradeoffs.
- When a model gains new mutable properties, its deep-copy logic may silently need updating. Nothing enforces this.
