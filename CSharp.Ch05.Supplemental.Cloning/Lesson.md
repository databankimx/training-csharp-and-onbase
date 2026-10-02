# Chapter 5 Supplemental: Shallow and Deep Cloning

## What This Is

The main lesson's `ICloneable` section briefly distinguished shallow from deep cloning. This project slows that distinction way down and makes it observable at each step, using `ReferenceEquals` to prove concretely which objects are shared and which are independent.

The `Person` model here has a nested `Address` (a reference type) and a `List<string>` of skills (also a reference type). That's the setup that makes the shallow/deep distinction matter -- a model with only value-type fields would look identical under both approaches.

---

## How to Write This Program

The types live in `Models/`. `Person` provides both a `ShallowClone()` method (using `MemberwiseClone()`) and a `DeepClone()` method (recursively cloning nested reference types). Run the program and read the output alongside the code.

### Step 1: Reference Assignment

```csharp
Person assigned = original;
Console.WriteLine(ReferenceEquals(original, assigned)); // True
```

No clone at all. Both variables point to the same object. This is the baseline -- "copying" a reference type by assignment copies the reference, not the object.

### Step 2: Shallow Clone

```csharp
Person shallow = original.ShallowClone();

Console.WriteLine(ReferenceEquals(original, shallow));                 // False - new Person
Console.WriteLine(ReferenceEquals(original.HomeAddress, shallow.HomeAddress)); // True - shared
Console.WriteLine(ReferenceEquals(original.Skills, shallow.Skills));          // True - shared
```

A new `Person` object was created, so the top-level object is independent. But `HomeAddress` and `Skills` are reference types -- `MemberwiseClone()` copies the references themselves, not the objects they point to. Both the original and the clone now hold references to the same `Address` and the same `List<string>`.

The consequence:

```csharp
shallow.HomeAddress.City = "Chicago";
// original.HomeAddress.City is now "Chicago" too -- same object
```

Assigning a new string to `shallow.Name` doesn't affect `original.Name` because string assignment replaces the clone's reference -- it doesn't mutate the original string (strings are immutable). The nested mutable reference types are where the shared-reference behavior shows up.

### Step 3: Deep Clone

```csharp
Person deep = original.DeepClone();

Console.WriteLine(ReferenceEquals(original, deep));                 // False
Console.WriteLine(ReferenceEquals(original.HomeAddress, deep.HomeAddress)); // False - independent
Console.WriteLine(ReferenceEquals(original.Skills, deep.Skills));          // False - independent
```

Every level of the object graph gets its own copy. Mutating `deep.HomeAddress.City` leaves `original.HomeAddress.City` untouched.

`DeepClone()` achieves this by constructing new instances of every nested reference type, passing through the values rather than the references:

```csharp
public Person DeepClone()
{
    return new Person
    {
        Name = Name,
        Age = Age,
        HomeAddress = new Address { Street = HomeAddress.Street, City = HomeAddress.City, State = HomeAddress.State },
        Skills = new List<string>(Skills) // new list, copied from the original
    };
}
```

---

## Takeaways

- Reference assignment copies the reference, not the object.
- `MemberwiseClone()` is a shallow clone -- new top-level object, shared nested reference-type fields.
- A deep clone must explicitly construct new instances of every nested reference type.
- `ReferenceEquals` is the definitive test: `false` means genuinely independent objects; `true` means shared.
- Strings behave like value types under assignment because they're immutable -- assigning `shallow.Name = "x"` replaces the clone's reference without touching the original's string object.
