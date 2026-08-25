# Custom Attributes Deep Dive

## Introduction

The main Chapter 8 lesson introduced custom attributes with one simple example. This lesson covers what comes up as soon as you actually start using attributes for real work: applying the same attribute more than once, setting attribute properties by name instead of through a constructor, checking whether an attribute is present without allocating one, and whether a subclass inherits a base class's attributes.

---

## Applying the Same Attribute More Than Once

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DataMappingAttribute : Attribute
{
    public string ColumnName { get; }
    public string PropertyName { get; }

    public DataMappingAttribute(string columnName, string propertyName)
    {
        ColumnName = columnName;
        PropertyName = propertyName;
    }
}
```

```csharp
[DataMapping("cust_id", "Id")]
[DataMapping("cust_name", "Name")]
[DataMapping("cust_email", "Email")]
public class CustomerRecord
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

By default, an attribute can only be applied once per target. Setting `AllowMultiple = true` in `[AttributeUsage(...)]` lifts that restriction, here, letting `CustomerRecord` carry its whole column-mapping table as three stacked attributes.

Reading them back requires the **plural** method:

```csharp
var mappings = recordType.GetCustomAttributes<DataMappingAttribute>();
```

The singular `GetCustomAttribute<T>()` throws an exception if more than one matching attribute is present, use the plural form whenever `AllowMultiple` is `true`.

---

## Setting Properties by Name

```csharp
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

Instead of requiring every value through a constructor, `AuditableAttribute` exposes plain settable properties, and `Enabled = true, Level = AuditLevel.Full` sets them directly by name when the attribute is applied. This works because `Level` and `Enabled` have public setters, this is the same named-initializer syntax C# supports for ordinary object initialization too, not something unique to attributes. `AuditLevel` here is a regular enum, showing that attribute properties can hold more than just strings and numbers.

---

## Checking Presence Without Allocating

```csharp
bool hasAuditable = Attribute.IsDefined(recordType, typeof(AuditableAttribute));
```

If you only need a yes/no answer, `IsDefined()` is cheaper than `GetCustomAttribute<T>()`, it never actually constructs an instance of the attribute, it just checks whether one is present.

---

## Inheritance: Does a Subclass Get the Attribute Too?

```csharp
[Auditable(Enabled = true, Level = AuditLevel.Full)]
public class BaseRecord { ... }

public class DerivedRecord : BaseRecord { ... }   // no attributes of its own
```

```csharp
derivedType.GetCustomAttribute<AuditableAttribute>();   // found!
```

Even though `DerivedRecord` never declares `[Auditable(...)]` itself, reflection still reports finding one, because `AuditableAttribute` was defined with `[AttributeUsage(..., Inherited = true)]`. Whether an attribute flows down to subclasses is entirely controlled by that setting on the attribute's own definition, not by anything the subclass does. Set `Inherited = false` instead, and a subclass won't be reported as carrying a base class's attribute at all, useful for markers that are genuinely meant to apply to one specific class only.

---

## Try It Yourself

Add a fourth `[DataMapping(...)]` to `CustomerRecord` and confirm the reflection output picks it up automatically, no code changes needed beyond the attribute itself. Then try changing `AuditableAttribute`'s `Inherited` setting to `false` and predict what `UsingAttributeInheritance()`'s output will change to before running it again.
