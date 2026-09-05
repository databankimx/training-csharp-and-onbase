# Chapter 5 Supplemental: Shallow and Deep Cloning

## What This Is

A supplemental lesson on copying objects in C#. It distinguishes simple reference assignment from an actual clone, then compares **shallow cloning** with **deep cloning** using a `Person` that owns both an `Address` object and a `List<string>`.

The main Chapter 5 lesson touches cloning through `ICloneable`. This project exists to slow the topic down and make each case observable, because "I copied it and the original changed anyway" is a bug people hit repeatedly before the distinction clicks.

---

## The Test Subject

```csharp
internal sealed class Person
{
	public string Name { get; set; }              // reference type, immutable
	public int Age { get; set; }                  // value type
	public Address HomeAddress { get; set; }      // reference type, mutable
	public List<string> Skills { get; set; }      // reference type, mutable
}
```

Four properties chosen deliberately to cover every behavior category: a value type, an immutable reference type, a mutable reference type, and a mutable collection. Each one behaves differently under a shallow clone, which is the entire point of the exercise.

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

This isn't cloning at all — it's the Chapter 3 reference-copy behavior. It's included first because it's the baseline people accidentally use when they meant to copy.

### Shallow Clone

```csharp
public Person ShallowClone()
{
	return (Person)MemberwiseClone();
}
```

`MemberwiseClone()` creates a new outer `Person`, but it copies each field as-is. Value-type fields such as `int` are copied by value. Reference-type fields such as `Address` and `List<string>` have their *references* copied, so the original and clone still share those child objects.

```csharp
ReferenceEquals(original, shallow)                 // false
ReferenceEquals(original.HomeAddress,
				shallow.HomeAddress)               // true
ReferenceEquals(original.Skills, shallow.Skills)   // true
```

New wrapper, same contents. That middle ground is exactly where the confusion lives — the object genuinely is a new object, so it *looks* copied, right up until someone mutates a child.

### Deep Clone

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
```

A deep clone creates the outer `Person` and also creates new copies of its mutable child objects.

```csharp
ReferenceEquals(original, deep)                    // false
ReferenceEquals(original.HomeAddress,
				deep.HomeAddress)                  // false
ReferenceEquals(original.Skills, deep.Skills)      // false
```

Three implementation details worth noticing:

- **`HomeAddress?.DeepClone()`** — the null-conditional operator. A `null` address stays `null` instead of throwing, and the recursion means `Address` is responsible for cloning itself. Deep cloning is inherently recursive; each type in the graph handles its own layer.
- **`new List<string>(Skills)`** — the copy constructor produces a genuinely new list. Note that this is itself a *shallow* copy of the list: it's safe here only because `string` is immutable. A `List<Address>` would need each element cloned individually.
- **`Skills == null ? null : ...`** — preserving `null` rather than silently substituting an empty list. A clone should reproduce the source's state, including the parts that are absent.

---

## Why Strings Behave Differently

`string` is a reference type, but strings are immutable. A shallow clone initially copies the same string reference, but code such as:

```csharp
shallow.Name = "Shallow Copy";
```

does not modify the existing string object. It assigns a *different* string reference to the clone's `Name` property. The original person's `Name` therefore remains unchanged.

That is different from:

```csharp
shallow.HomeAddress.City = "Chicago";
```

The `Address` itself is mutable and shared by a shallow clone, so changing one of its properties is visible through **both** `Person` objects.

Watch the distinction carefully, because it's the crux of the whole lesson. Assigning to `shallow.Name` replaces a reference held by the clone. Assigning to `shallow.HomeAddress.City` reaches *through* a shared reference and mutates the object on the far end. The first is invisible to the original; the second is not.

The same logic explains `Age`. It's an `int`, copied by value, so the clone owns its own copy and there's nothing to share.

---

## The Main Rule

The question is not merely whether a property is a reference type. The important question is:

> **Does the clone share mutable state with the source?**

A shallow clone generally does. A correctly implemented deep clone does not, at least for the portion of the object graph that the application intends to own independently.

This also explains why immutability is such a valuable property in a type. An immutable object is always safe to share, which means shallow cloning is always sufficient for it, which means the whole shallow-versus-deep question stops applying. Making `Address` immutable would have been an alternative solution to the same problem.

---

## About `MemberwiseClone()`

`MemberwiseClone()` is a `protected` method inherited from `System.Object`. Because it's protected, external callers can't invoke it — a class has to expose it deliberately:

```csharp
public Person ShallowClone()
{
	return (Person)MemberwiseClone();
}
```

It is specifically a **shallow** copy operation. It does not recursively clone child objects, and there is no option to make it do so.

It has one genuine advantage: it copies every field automatically, including private ones, and it keeps working when someone adds a new field later. A hand-written deep clone silently misses newly added properties — which is a real maintenance hazard worth knowing about, since nothing in the compiler will warn you that `DeepClone()` has fallen out of sync with the class definition.

---

## About `ICloneable`

.NET also defines `ICloneable`, but its `Clone()` contract does not specify whether the result must be shallow or deep. That ambiguity makes calling code harder to reason about — you cannot tell from the call site which behavior you're getting, and the return type is `object`, so every call needs a cast.

For application code, explicit names such as `ShallowClone()`, `DeepClone()`, or a copy constructor are clearer. That's exactly why this project uses two distinctly named methods instead of implementing the interface.

---

## When to Use Each

Use a **shallow clone** when shared child objects are intentional, immutable, or otherwise safe to share.

Use a **deep clone** when the copy must be independently mutable without changing the original.

Deep cloning should be deliberate. Not every referenced object should automatically be duplicated — some references represent shared services, database connections, caches, loggers, or other resources that *should* remain shared rather than copied. A "clone everything" implementation that duplicates a database connection is a worse bug than the shared-state one it was trying to fix.

A note on the serialization shortcut: you'll see deep cloning implemented by serializing an object and immediately deserializing it. It's concise and it handles arbitrary graphs, but it's slow, it requires every type in the graph to be serializable, and it gives you no control over what stays shared. Fine for a quick tool; not a default.

---

## Takeaways

- Assignment copies a reference. It is not a copy of anything.
- `MemberwiseClone()` is always shallow — new wrapper, shared contents.
- Value types and immutable types (like `string`) are safe under a shallow clone; mutable reference types are not.
- Deep cloning is recursive, and each type should clone its own layer.
- The real question is never "is it a reference type," it's "is mutable state shared."
- Prefer explicitly named `ShallowClone()`/`DeepClone()` methods over `ICloneable`.
- Don't deep clone reflexively. Some references are meant to stay shared.
