# Chapter 5 Supplemental: Shallow and Deep Cloning

## What This Is

A supplemental lesson on copying objects in C#. It distinguishes simple reference assignment from an actual clone, then compares **shallow cloning** with **deep cloning** using a `Person` that owns both an `Address` object and a `List<string>`.

---

## The Three Cases

### Reference Assignment

```csharp
Person assigned = original;
```

No object is copied. `assigned` and `original` are two variables pointing at the exact same `Person` instance.

```csharp
ReferenceEquals(original, assigned) // true
```

### Shallow Clone

```csharp
Person shallow = original.ShallowClone();
```

`MemberwiseClone()` creates a new outer `Person`, but it copies each field as-is. Value-type fields such as `int` are copied by value. Reference-type fields such as `Address` and `List<string>` have their references copied, so the original and clone still share those child objects.

```csharp
ReferenceEquals(original, shallow)             // false
ReferenceEquals(original.HomeAddress,
                shallow.HomeAddress)            // true
ReferenceEquals(original.Skills, shallow.Skills) // true
```

### Deep Clone

```csharp
Person deep = original.DeepClone();
```

A deep clone creates the outer `Person` and also creates new copies of its mutable child objects.

```csharp
ReferenceEquals(original, deep)                 // false
ReferenceEquals(original.HomeAddress,
                deep.HomeAddress)                // false
ReferenceEquals(original.Skills, deep.Skills)   // false
```

---

## Why Strings Behave Differently

`string` is a reference type, but strings are immutable. A shallow clone initially copies the same string reference, but code such as:

```csharp
shallow.Name = "Shallow Copy";
```

does not modify the existing string object. It assigns a different string reference to the clone's `Name` property. The original person's `Name` therefore remains unchanged.

That is different from:

```csharp
shallow.HomeAddress.City = "Chicago";
```

The `Address` itself is mutable and shared by a shallow clone, so changing one of its properties is visible through both `Person` objects.

---

## The Main Rule

The question is not merely whether a property is a reference type. The important question is:

> **Does the clone share mutable state with the source?**

A shallow clone generally does. A correctly implemented deep clone does not, at least for the portion of the object graph that the application intends to own independently.

---

## About `MemberwiseClone()`

`MemberwiseClone()` is a protected method inherited from `System.Object`. A class can expose it through a method such as:

```csharp
public Person ShallowClone()
{
    return (Person)MemberwiseClone();
}
```

It is specifically a **shallow** copy operation. It does not recursively clone child objects.

---

## About `ICloneable`

.NET also defines `ICloneable`, but its `Clone()` contract does not specify whether the result must be shallow or deep. That ambiguity can make calling code harder to understand. For application code, explicit names such as `ShallowClone()`, `DeepClone()`, or a copy constructor are often clearer.

---

## When to Use Each

Use a shallow clone when shared child objects are intentional, immutable, or otherwise safe to share. Use a deep clone when the copy must be independently mutable without changing the original.

Deep cloning should be deliberate. Not every referenced object should automatically be duplicated: some references may represent shared services, database connections, caches, or other resources that should remain shared rather than copied.
