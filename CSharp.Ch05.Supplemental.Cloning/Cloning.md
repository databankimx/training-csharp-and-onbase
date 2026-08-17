# Cloning Objects in C#

## Introduction

In C#, a variable holding a class instance normally contains a **reference** to an object. Assigning that variable to another variable copies the reference, not the object.

```csharp
Person second = first;
```

After this assignment, `first` and `second` refer to the same `Person`. That is not cloning.

Cloning creates another object. The two most common conceptual forms are **shallow cloning** and **deep cloning**.

---

## The Example Object Graph

This project uses a `Person` with both simple values and nested mutable reference types:

```csharp
internal sealed class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Address HomeAddress { get; set; }
    public List<string> Skills { get; set; }
}
```

Conceptually:

```text
Person
  |-- Name       -> string
  |-- Age        -> int
  |-- HomeAddress -> Address
  `-- Skills      -> List<string>
```

This matters because cloning rules act differently on values and references.

---

## Reference Assignment Is Not a Clone

```csharp
Person assigned = original;
```

Memory can be pictured like this:

```text
original ----+
             +----> Person
assigned ----+
```

There is only one `Person` object.

```csharp
Console.WriteLine(ReferenceEquals(original, assigned)); // True
```

Any mutation made through either variable is a mutation of the same object.

---

## Shallow Clone

The project exposes `MemberwiseClone()` through `ShallowClone()`:

```csharp
public Person ShallowClone()
{
    return (Person)MemberwiseClone();
}
```

`MemberwiseClone()` creates a new outer object and copies the fields from the source object into it.

For value types, the value itself is copied:

```text
original.Age = 36
shallow.Age  = 36
```

The two `Age` fields are independent values.

For reference types, the reference is copied:

```text
original ----> Person A ----> Address X

shallow  ----> Person B ----> Address X
```

The two `Person` instances are different, but they both point to the same `Address`.

The same problem appears with mutable collections:

```text
original ----> Person A ----> List X
shallow  ----> Person B ----> List X
```

Therefore:

```csharp
shallow.HomeAddress.City = "Chicago";
shallow.Skills.Add("New Skill");
```

also changes what is observed through `original.HomeAddress` and `original.Skills`.

---

## Why `string` Does Not Usually Cause the Same Surprise

`string` is a reference type, so technically a shallow clone copies the string reference too. But strings are immutable.

```csharp
shallow.Name = "Different Name";
```

This does not edit the string shared with the original. It replaces the clone's property with a reference to a different string.

As a result:

```csharp
original.Name // still the original value
shallow.Name  // the new value
```

This is why the practical cloning concern is usually **shared mutable reference state**.

---

## Deep Clone

A deep clone creates independent copies of the mutable objects that belong to the source.

This project's `Person.DeepClone()` is intentionally explicit:

```csharp
public Person DeepClone()
{
    return new Person
    {
        Name = Name,
        Age = Age,
        HomeAddress = HomeAddress?.DeepClone(),
        Skills = Skills == null
            ? null
            : new List<string>(Skills)
    };
}
```

And `Address` creates its own copy:

```csharp
public Address DeepClone()
{
    return new Address
    {
        Street = Street,
        City = City,
        State = State
    };
}
```

Now the object graph looks like this:

```text
original ----> Person A ----> Address X
                         `---> List X

deep     ----> Person B ----> Address Y
                         `---> List Y
```

Changing `Address Y` or `List Y` cannot change `Address X` or `List X`.

---

## `ReferenceEquals()` Is Useful for Proving What Happened

`ReferenceEquals()` answers whether two variables refer to the exact same object.

```csharp
ReferenceEquals(original, shallow)
```

is `false`, because `MemberwiseClone()` created another `Person`.

But:

```csharp
ReferenceEquals(original.HomeAddress, shallow.HomeAddress)
```

is `true`, because a shallow copy shares that nested reference.

For the deep clone, both tests are `false`.

---

## Shallow vs. Deep at a Glance

| Question | Shallow Clone | Deep Clone |
|---|---|---|
| New outer object? | Yes | Yes |
| Value-type fields copied? | Yes | Yes |
| Nested references copied? | Yes | No, when those children are explicitly cloned |
| Mutable child objects shared? | Usually yes | No, if implemented correctly |
| Faster/simpler? | Usually | Usually not |
| Safe for independent mutation? | Not necessarily | Yes, for cloned portions of the graph |

---

## Deep Clone Does Not Mean "Blindly Duplicate Everything"

A useful deep-clone implementation follows **ownership semantics**.

Suppose a class contains:

```csharp
public ILogger Logger { get; set; }
public SqlConnection Connection { get; set; }
public Address HomeAddress { get; set; }
```

An independent copy of `HomeAddress` may make sense. Duplicating a logger or live database connection probably does not.

A deep clone is therefore best understood as:

> Create independent copies of the mutable state that the object logically owns.

It is not necessarily "recursively copy every reference no matter what it represents."

---

## Why This Project Uses Explicit Clone Methods

You may encounter `ICloneable`:

```csharp
public interface ICloneable
{
    object Clone();
}
```

The interface does **not** say whether `Clone()` must make a shallow or deep copy. A caller has to know the implementation-specific behavior.

Explicit methods communicate intent better:

```csharp
person.ShallowClone();
person.DeepClone();
```

Copy constructors are another clear option:

```csharp
public Person(Person source)
{
    Name = source.Name;
    Age = source.Age;
    HomeAddress = source.HomeAddress?.DeepClone();
    Skills = source.Skills == null
        ? null
        : new List<string>(source.Skills);
}
```

---

## Serialization-Based Cloning

Another approach sometimes shown in examples is serializing an object and deserializing it into a new instance. This can create independent object graphs, but it has tradeoffs:

- every required type must be supported by the serializer;
- serialization can be significantly more expensive than explicit copying;
- constructors, private state, polymorphism, and unsupported resource types can complicate the result;
- it hides which references should really be shared versus independently copied.

For a small, known domain model, explicit copy logic is usually easier to read and reason about.

---

## Exercise 1: Predict the Output

Before running the project, predict the results of these expressions:

```csharp
ReferenceEquals(original, shallow)
ReferenceEquals(original.HomeAddress, shallow.HomeAddress)
ReferenceEquals(original.Skills, shallow.Skills)

ReferenceEquals(original, deep)
ReferenceEquals(original.HomeAddress, deep.HomeAddress)
ReferenceEquals(original.Skills, deep.Skills)
```

Then run the program and compare your answer.

---

## Exercise 2: Add Another Mutable Child

Add this model:

```csharp
internal sealed class EmergencyContact
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
}
```

Add an `EmergencyContact` property to `Person`.

First, do **not** update `DeepClone()`. Run the program and prove that the supposedly deep clone still shares the contact.

Then update `DeepClone()` so `EmergencyContact` is independently copied.

This demonstrates an important maintenance risk: when a model gains new mutable reference properties, its deep-copy logic may also need to change.

---

## Key Takeaways

1. Assignment of a class variable copies a reference; it does not clone the object.
2. `MemberwiseClone()` creates a shallow copy.
3. A shallow clone gets a new outer object but normally shares nested mutable reference objects.
4. A deep clone explicitly creates independent copies of the mutable state that should not be shared.
5. Strings are reference types, but their immutability makes them safe to share.
6. `ReferenceEquals()` is a simple way to prove whether two variables share the same object instance.
7. Deep-copy behavior should reflect object ownership, not blindly duplicate every reference.
8. Explicit `ShallowClone()` and `DeepClone()` methods make intent clearer than an ambiguous `ICloneable.Clone()` implementation.
