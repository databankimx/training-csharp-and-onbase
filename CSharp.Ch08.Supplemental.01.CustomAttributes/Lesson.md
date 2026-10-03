# Chapter 8 Supplemental 01: Custom Attributes

## What This Is

The main lesson covered the basics of custom attributes: define one, apply it once, read it back with `GetCustomAttribute<T>()`. That's enough to understand what attributes are. This project covers the four things that come up as soon as you actually start using them for real work.

What's being extended here is the attribute declaration model. The main lesson's `CourseCatalogAttribute` had `AllowMultiple = false`, constructor-only properties, and no inheritance consideration. Each mini-program in this project relaxes or changes one of those constraints and shows the practical consequence. There's no performance story here - attributes are read lazily and infrequently. The improvement is expressiveness: the right attribute configuration makes misuse a compile error rather than a runtime surprise.

---

## How to Write This Program

The models are already in the project. Read them before starting - they're short and the lesson references them directly.

**Attributes:**
- `DataMappingAttribute` - `AllowMultiple = true`, maps external column names to property names
- `AuditableAttribute` - `Inherited = true`, uses named initializer syntax, has an enum-typed property
- `ClassSpecificAttribute` - `Inherited = false`, a minimal marker attribute

**Objects:**
- `CustomerRecord` - has three stacked `[DataMapping(...)]` attributes
- `BaseRecord` - carries `[Auditable(Enabled = true, Level = AuditLevel.Full)]` and `[ClassSpecific]`
- `DerivedRecord : BaseRecord` - declares no attributes of its own

---

### Mini-Program 1: AllowMultiple

The main lesson's `CourseCatalogAttribute` had `AllowMultiple = false` - one instance per class. `DataMappingAttribute` turns it on, letting a class carry its entire column mapping table as stacked attributes. This is a pattern you'll see in ORMs and serialization libraries.

Clear `Main()` and write:

```csharp
var recordType = typeof(CustomerRecord);

// GetCustomAttribute<T>() (singular) would throw here - there's more than one.
// GetCustomAttributes<T>() (plural) returns all of them as a collection.
var mappings = recordType.GetCustomAttributes<DataMappingAttribute>().ToList();

Console.WriteLine($"{recordType.Name} carries {mappings.Count} DataMappingAttribute instance(s):");
foreach (var mapping in mappings)
    Console.WriteLine($" - Column '{mapping.ColumnName}' maps to property '{mapping.PropertyName}'");

GenericFunctions.Pause();
```

Run it. Three mappings are returned, one per `[DataMapping(...)]` applied to `CustomerRecord`.

The singular `GetCustomAttribute<T>()` would throw an `AmbiguousMatchException` here because there's more than one instance. Use the plural form whenever `AllowMultiple = true`. Conversely, when `AllowMultiple = false` you can use either form - the plural just returns a single-element collection.

### Mini-Program 2: Named Initializer Syntax and Enum Properties

`AuditableAttribute` has no constructor arguments. Its properties are set with named initializer syntax at the usage site: `[Auditable(Enabled = true, Level = AuditLevel.Full)]`. This is the second way to supply attribute values, alongside constructor arguments. Both end up baked into assembly metadata at compile time as constant data.

Enum-typed properties are also common in real attributes (`[JsonProperty(NamingPolicy = NamingPolicy.CamelCase)]`). They work the same way - the enum value is a compile-time constant.

Clear `Main()` and write:

```csharp
var recordType = typeof(BaseRecord);
var auditable = recordType.GetCustomAttribute<AuditableAttribute>();

if (auditable != null)
    Console.WriteLine($"{recordType.Name} is auditable: {auditable.Enabled}, level: {auditable.Level}");

GenericFunctions.Pause();
```

Run it. `Enabled` is `true`, `Level` is `Full`.

Note `AuditableAttribute` has no constructor - all properties are get/set, not get-only. Named initializer syntax can set any public settable property. Constructor arguments are required; named initializers are optional. In practice, required values go in the constructor, optional or defaultable values go as named initializers.

### Mini-Program 3: IsDefined() vs GetCustomAttribute\<T\>()

`GetCustomAttribute<T>()` allocates an attribute instance every time you call it - the constructor arguments stored in metadata are used to construct a real object. If all you need is a yes/no answer, that allocation is wasted.

Clear `Main()` and write:

```csharp
var recordType = typeof(BaseRecord);

// IsDefined(): a pure presence check, returns bool, never allocates an attribute instance.
bool hasAuditable = Attribute.IsDefined(recordType, typeof(AuditableAttribute));
Console.WriteLine($"IsDefined<AuditableAttribute>() on {recordType.Name}: {hasAuditable}");

// GetCustomAttribute<T>(): allocates and returns the instance. Use this when you need the data.
var auditable = recordType.GetCustomAttribute<AuditableAttribute>();
Console.WriteLine($"GetCustomAttribute<AuditableAttribute>().Level: {auditable?.Level}");

GenericFunctions.Pause();
```

Run it. `IsDefined` returns `true`. `GetCustomAttribute` returns the attribute with its data.

In a tight loop over many types - scanning an assembly's types at startup, for example - `IsDefined` is noticeably cheaper when you're filtering before deciding whether to read the data. In a single call, the difference is irrelevant.

### Mini-Program 4: Attribute Inheritance

`[AttributeUsage(Inherited = true/false)]` on the attribute definition controls whether subclasses of an attributed class are considered to carry that attribute too, via reflection.

- `AuditableAttribute` has `Inherited = true` - a subclass of an `[Auditable]` class is reported as auditable.
- `ClassSpecificAttribute` has `Inherited = false` - a subclass of a `[ClassSpecific]` class is not.

`DerivedRecord` inherits from `BaseRecord`, which carries both. `DerivedRecord` declares no attributes of its own.

Clear `Main()` and write:

```csharp
var derivedType = typeof(DerivedRecord);

// AuditableAttribute: Inherited = true - BaseRecord's attribute is found on DerivedRecord.
var inheritedAuditable = derivedType.GetCustomAttribute<AuditableAttribute>();
Console.WriteLine($"{derivedType.Name} + AuditableAttribute (Inherited=true): " +
    (inheritedAuditable != null ? $"found (Level = {inheritedAuditable.Level})" : "not found"));

// ClassSpecificAttribute: Inherited = false - BaseRecord's attribute is NOT found on DerivedRecord.
var notInherited = derivedType.GetCustomAttribute<ClassSpecificAttribute>();
Console.WriteLine($"{derivedType.Name} + ClassSpecificAttribute (Inherited=false): " +
    (notInherited != null ? "found" : "not found"));

GenericFunctions.Pause();
```

Run it. `AuditableAttribute` is found; `ClassSpecificAttribute` is not - even though `BaseRecord` carries both.

The practical consequence: use `Inherited = true` for attributes that represent a contract the whole hierarchy must honor. Use `Inherited = false` for attributes that are specific to one type's own metadata. The default for `[AttributeUsage]` is `Inherited = true`, so you only need to set it explicitly when you want `false`.

---

## Try It Yourself

Add a fourth `[DataMapping(...)]` to `CustomerRecord` and confirm the reflection output picks it up automatically - no code changes needed beyond the attribute itself. Then change `AuditableAttribute`'s `Inherited` setting to `false` and predict what Mini-Program 4's output will change to before running it again.

---

## Summary

| Feature | Default | Effect of changing it |
|---|---|---|
| `AllowMultiple` | `false` (one instance per target) | `true` allows stacking; read with plural `GetCustomAttributes<T>()` |
| Property style | Constructor args (required) | Named initializers for optional/defaultable values |
| `IsDefined()` vs `GetCustomAttribute<T>()` | N/A | `IsDefined` is cheaper when you only need presence |
| `Inherited` | `true` (flows to subclasses) | `false` locks the attribute to exactly the class it's applied to |

---

## Takeaways

- `AllowMultiple = true` lets the same attribute type be stacked multiple times on one target.
- Use `GetCustomAttributes<T>()` (plural) for `AllowMultiple` attribute types; the singular form throws on multiple instances.
- Named initializer syntax (`[Attr(Property = value)]`) sets public settable properties; constructor arguments are for required values.
- Enum-typed attribute properties work the same way as any other compile-time constant property.
- `IsDefined()` is a cheap presence check that never allocates an attribute instance.
- `Inherited = true` (the default): subclasses of an attributed class report having the attribute.
- `Inherited = false`: subclasses do not report having the attribute, even if the base class carries it.
