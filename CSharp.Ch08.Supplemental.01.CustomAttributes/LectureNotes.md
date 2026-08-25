# Chapter 8 Supplemental 01: Custom Attributes Deep Dive

## What This Is

The main lesson's custom attribute coverage (`CourseCatalogAttribute`) was deliberately minimal, one attribute, one target, constructor-only properties, applied once. This project covers four things that come up as soon as you actually start using attributes for real work: `AllowMultiple`, named initializer syntax with enum-typed properties, `IsDefined()` vs. `GetCustomAttribute<T>()`, and `Inherited`.

---

## `AllowMultiple`: Stacking the Same Attribute

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DataMappingAttribute : Attribute
{
    public string ColumnName { get; }
    public string PropertyName { get; }
    ...
}
```

```csharp
[DataMapping("cust_id", "Id")]
[DataMapping("cust_name", "Name")]
[DataMapping("cust_email", "Email")]
public class CustomerRecord { ... }
```

Without `AllowMultiple = true`, stacking three `[DataMapping(...)]` attributes on the same class wouldn't even compile. With it, `CustomerRecord` carries its entire external-column-to-property mapping table as data, readable at runtime:

```csharp
var mappings = recordType.GetCustomAttributes<DataMappingAttribute>().ToList();
```

Note the plural `GetCustomAttributes<T>()` here, not the singular `GetCustomAttribute<T>()` the main lesson used. Calling the singular version on a type with more than one matching attribute throws `AmbiguousMatchException`, worth remembering: the moment `AllowMultiple` is `true`, code reading that attribute back needs to use the plural form.

---

## Named Initializers and Enum-Typed Properties

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class AuditableAttribute : Attribute
{
    public bool Enabled { get; set; }
    public AuditLevel Level { get; set; }
}
```

```csharp
[Auditable(Enabled = true, Level = AuditLevel.Full)]
public class BaseRecord { ... }
```

Unlike `CourseCatalogAttribute` (which required its values through the constructor), `AuditableAttribute` has no constructor at all, its properties are settable, and the `[Auditable(Enabled = true, Level = AuditLevel.Full)]` syntax sets them directly by name. This is worth recognizing as its own C# feature (object/attribute initializer syntax), not something specific to attributes, the same `PropertyName = value` pattern works in ordinary object initializers too. `Level`'s type, `AuditLevel` (a plain enum), demonstrates that attribute properties aren't limited to primitives and strings.

---

## `IsDefined()`: A Cheaper Yes/No Check

```csharp
bool hasAuditable = Attribute.IsDefined(recordType, typeof(AuditableAttribute));
```

`IsDefined()` answers "is this attribute present at all" without ever constructing an instance of it. If all you need is a boolean, this avoids the (admittedly small, but real) cost of allocating and populating an attribute object just to check whether it's `null`.

---

## `Inherited`: Does a Subclass Count?

```csharp
[Auditable(Enabled = true, Level = AuditLevel.Full)]   // Inherited = true
[ClassSpecific]                                         // Inherited = false
public class BaseRecord { ... }

public class DerivedRecord : BaseRecord { ... }         // declares neither attribute itself
```

```csharp
derivedType.GetCustomAttribute<AuditableAttribute>();     // found (inherited from BaseRecord)
derivedType.GetCustomAttribute<ClassSpecificAttribute>(); // not found
```

Both attributes live only on `BaseRecord`, `DerivedRecord` declares neither directly. Whether reflection reports them as present on `DerivedRecord` anyway depends entirely on each attribute's own `[AttributeUsage(..., Inherited = ...)]` setting, not on anything `DerivedRecord` itself does. `AuditableAttribute` says `Inherited = true`, so its data flows down to every subclass automatically, useful for something like "every entity under this base type should be audited, unless a subclass explicitly overrides it." `ClassSpecificAttribute` says `Inherited = false`, so it applies to exactly the class it's written on and no further, useful for a marker that's genuinely meant to be class-specific.
