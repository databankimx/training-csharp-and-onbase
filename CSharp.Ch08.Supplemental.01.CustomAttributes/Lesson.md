# Chapter 8 Supplemental 01: Custom Attributes Deep Dive

## What This Is

The main lesson's custom attribute coverage (`CourseCatalogAttribute`) was deliberately minimal — one attribute, one target, constructor-only properties, applied once. That's enough to understand the concept, but it leaves out nearly everything you hit the moment you use attributes for real work.

This project covers four of those things:

1. **`AllowMultiple`** — applying the same attribute more than once
2. **Named initializer syntax** and enum-typed properties
3. **`IsDefined()`** vs. `GetCustomAttribute<T>()`
4. **`Inherited`** — whether subclasses report the attribute too

---

## `AllowMultiple`: Stacking the Same Attribute

By default, an attribute can be applied to a given target exactly once. Setting `AllowMultiple = true` changes that:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DataMappingAttribute : Attribute
{
	public string ColumnName { get; }
	public string PropertyName { get; }
	...
}
```

Which enables this:

```csharp
[DataMapping("cust_id", "Id")]
[DataMapping("cust_name", "Name")]
[DataMapping("cust_email", "Email")]
public class CustomerRecord { ... }
```

Without `AllowMultiple = true`, stacking three `[DataMapping(...)]` attributes on the same class **wouldn't even compile**. With it, `CustomerRecord` carries its entire external-column-to-property mapping table as declarative data.

This is a realistic pattern. The class now describes its own relationship to an external schema — a database table, a CSV header row, a fixed-width file layout — without a separate mapping file or a hand-written translation method. A generic reader can consume any class annotated this way without knowing anything else about it.

### The Plural Form Is Mandatory

`UsingAllowMultiple()`:

```csharp
// GetCustomAttribute<T>() (singular) would throw here, since there's more than
//   one DataMappingAttribute on this type. GetCustomAttributes<T>() (plural)
//   returns all of them.
var mappings = recordType.GetCustomAttributes<DataMappingAttribute>().ToList();

Console.WriteLine($"{recordType.Name} carries {mappings.Count} DataMappingAttribute instance(s):");
foreach (var mapping in mappings)
{
	Console.WriteLine($" - Column '{mapping.ColumnName}' maps to property '{mapping.PropertyName}'");
}
```

This is the practical trap, and it's worth stating plainly: **the moment `AllowMultiple` is `true`, every piece of code that reads that attribute must use `GetCustomAttributes<T>()` (plural).** The singular version throws `AmbiguousMatchException` when it finds more than one match.

Note the failure mode. If someone applies a second `[DataMapping]` to a class that previously had one, existing code calling the singular form starts throwing — and the change that broke it was made in an entirely different file, with no compiler warning. When you set `AllowMultiple = true`, audit every read site.

The plural form also has a pleasant property the singular one lacks: it returns an **empty collection**, never `null`, when nothing matches. No null check needed.

---

## Named Initializers and Enum-Typed Properties

`AuditableAttribute` is built differently from anything in the main lesson:

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class AuditableAttribute : Attribute
{
	public bool Enabled { get; set; }
	public AuditLevel Level { get; set; }
}
```

Applied as:

```csharp
[Auditable(Enabled = true, Level = AuditLevel.Full)]
public class BaseRecord { ... }
```

Three differences from `CourseCatalogAttribute` worth noticing:

**No constructor at all.** `CourseCatalogAttribute` required its values through a constructor and exposed get-only properties. `AuditableAttribute` has neither — just settable properties and the implicit parameterless constructor.

**Named initializer syntax.** The `Enabled = true, Level = AuditLevel.Full` form sets properties by name after construction. This is worth recognizing as **its own C# feature**, not something attribute-specific — the same `PropertyName = value` pattern is ordinary object initializer syntax, which you'd use for any class.

**An enum-typed property.** `Level` is an `AuditLevel`, demonstrating that attribute properties aren't limited to strings and primitives.

### Positional vs. Named: Which to Use

The two forms can be combined — positional constructor arguments must come first, named ones after:

```csharp
[SomeAttribute("required value", OptionalFlag = true)]
```

The convention that follows from this:

- **Constructor parameters** for values that are *required* — the attribute is meaningless without them. `CourseCatalogAttribute` can't do its job without a department.
- **Settable properties** for values that are *optional* — sensible defaults exist. An unset `bool` property is simply `false`.

Note the tradeoff `AuditableAttribute` accepts by having no constructor: `[Auditable]` with no arguments at all is legal, producing `Enabled = false`. Whether that's a reasonable default or a silent mistake depends on the design — but it's a real consequence of choosing properties over constructor parameters.

### What Types Are Allowed

Attribute values are baked into assembly metadata at compile time, so they're restricted to compile-time constants:

- Simple types (`bool`, `int`, `double`, etc.)
- `string`
- `enum` types (as `AuditLevel` shows)
- `System.Type` (e.g. `typeof(Foo)`)
- One-dimensional arrays of the above

You cannot pass a `new` object, a computed expression, or anything resolved at runtime.

---

## `IsDefined()`: A Cheaper Yes/No Check

`UsingIsDefined()`:

```csharp
bool hasAuditable = Attribute.IsDefined(recordType, typeof(AuditableAttribute));
Console.WriteLine($"IsDefined<AuditableAttribute>() on {recordType.Name}: {hasAuditable}");

// IsDefined() answers "is it there at all", without ever constructing an
//   AuditableAttribute instance behind the scenes, cheaper when you don't
//   actually need the attribute's data, just whether it's present.
```

The distinction rests on something established in the main lesson: **`GetCustomAttribute<T>()` actually instantiates the attribute object.** It reads the constructor arguments and property values out of metadata and builds a real object from them.

`IsDefined()` skips all of that. It checks metadata for the attribute's presence and returns a `bool` — no allocation, no property population.

The saving on a single call is negligible. It becomes meaningful in the scenario where attribute-scanning actually happens: sweeping every type in an assembly at startup to find the handful that are marked. Filtering a few thousand types with `IsDefined()` and only materializing attributes for the matches is a genuinely different amount of work than constructing an attribute object for every candidate just to compare it against `null`.

The rule is straightforward: **if you need the attribute's data, use `GetCustomAttribute<T>()`. If you only need to know whether it's there, use `IsDefined()`.** Don't call `IsDefined()` and *then* `GetCustomAttribute<T>()` — that's two metadata lookups to do one job; just null-check the latter.

Note also that `IsDefined()` is used here as the static `Attribute.IsDefined(type, attributeType)`. There's an equivalent instance method, `type.IsDefined(typeof(T))`, on `Type` itself.

---

## `Inherited`: Does a Subclass Count?

The setup deliberately puts two attributes that *disagree* on the same base class:

```csharp
[Auditable(Enabled = true, Level = AuditLevel.Full)]   // Inherited = true
[ClassSpecific]                                         // Inherited = false
public class BaseRecord { ... }

public class DerivedRecord : BaseRecord { ... }         // declares neither attribute itself
```

And then queries the *derived* type for both — `UsingAttributeInheritance()`:

```csharp
// AuditableAttribute is marked Inherited = true: DerivedRecord declares no
//   [Auditable] attribute of its own, but reflection still finds BaseRecord's.
var inheritedAuditable = derivedType.GetCustomAttribute<AuditableAttribute>();

// ClassSpecificAttribute is marked Inherited = false: even though BaseRecord has
//   one, DerivedRecord does NOT report having it.
var notInherited = derivedType.GetCustomAttribute<ClassSpecificAttribute>();
```

Output: `AuditableAttribute` is **found** (with `Level = Full`), `ClassSpecificAttribute` is **not found**.

### Why This Design Is Worth Understanding

The critical point: **both attributes live only on `BaseRecord`. `DerivedRecord` declares neither.** The difference in behavior comes entirely from each attribute's own `[AttributeUsage(..., Inherited = ...)]` setting — nothing about `DerivedRecord` differs between the two cases.

This means **the attribute's author decides the inheritance semantics, not the consumer.** When you define an attribute, you're making a policy decision about every future subclass of every class it's applied to:

- **`Inherited = true`** suits policy that should cascade. "Every entity under this base type should be audited, unless a subclass explicitly overrides it." A subclass gets the behavior automatically, which is usually what you want for cross-cutting concerns.
- **`Inherited = false`** suits markers genuinely meant for one specific class. Note that `DataMappingAttribute` uses this — and correctly so, since a subclass almost certainly maps to a *different* set of columns. Inheriting the parent's mapping table would be actively wrong.

`Inherited` defaults to `true`, which is easy to forget. If your attribute describes something class-specific, say so explicitly.

### Two Wrinkles

**`Inherited` applies to class inheritance, not interface implementation.** An attribute on an interface is never inherited by implementing types, regardless of the `Inherited` setting.

**Some APIs let the caller override it.** The lower-level `GetCustomAttributes(Type, bool inherit)` overload takes an explicit `inherit` flag. And `Attribute.IsDefined()` has an overload that accepts one too. The generic extension methods used here default to honoring the attribute's own declaration, which is the sane behavior — but if you see a stray `false` in a legacy call, that's what it's doing.

---

## What to Take Away

**`AllowMultiple = true` changes the contract for every reader.** The singular `GetCustomAttribute<T>()` throws `AmbiguousMatchException` once a second instance exists. Use the plural form — it also conveniently returns an empty collection rather than `null`.

**Constructor parameters mean required; settable properties mean optional.** Attribute values must be compile-time constants either way, which rules out anything computed at runtime.

**`IsDefined()` doesn't allocate.** Reach for it when scanning many types for a marker and you don't need the attribute's data.

**`Inherited` is the attribute author's decision, not the consumer's.** It defaults to `true`. Set it to `false` deliberately for anything class-specific — `DataMappingAttribute` is the model case, since inheriting a parent's column mapping would be a bug rather than a convenience.
