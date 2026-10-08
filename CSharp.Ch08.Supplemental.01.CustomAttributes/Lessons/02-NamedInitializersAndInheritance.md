---
title: "Named Initializers, Enums, IsDefined, and Inheritance"
chapter: 8
index: 2
dependencies: []
---

```csharp
using System;
using System.Reflection;

public enum AuditLevel { None, Basic, Full }

// Named initializer syntax: properties are set at the usage site without going through the constructor.
// Required values go in the constructor; optional/defaultable values go as named initializers.
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class AuditableAttribute : Attribute
{
    public bool       Enabled { get; set; }
    public AuditLevel Level   { get; set; }
}

// Inherited = false: subclasses of a [ClassSpecific]-decorated type do NOT report having it.
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ClassSpecificAttribute : Attribute { }

// BaseRecord carries both attributes.
// Auditable uses named initializer syntax and an enum-typed property.
[Auditable(Enabled = true, Level = AuditLevel.Full)]
[ClassSpecific]
public class BaseRecord { }

// DerivedRecord declares no attributes of its own.
public class DerivedRecord : BaseRecord { }

internal static class Program
{
    private static void Main()
    {
        // --- Named initializers and enum properties ---
        var auditable = typeof(BaseRecord).GetCustomAttribute<AuditableAttribute>();
        if (auditable != null)
            Console.WriteLine($"BaseRecord: Enabled={auditable.Enabled}, Level={auditable.Level}");

        // --- IsDefined: presence check, never allocates an attribute instance ---
        // Use this when you only need yes/no. GetCustomAttribute<T>() allocates; IsDefined doesn't.
        bool hasCatalog = Attribute.IsDefined(typeof(BaseRecord), typeof(AuditableAttribute));
        Console.WriteLine($"\nIsDefined<AuditableAttribute> on BaseRecord: {hasCatalog}");

        // --- Attribute inheritance ---
        // DerivedRecord declares no attributes. What does reflection find?
        var derivedType = typeof(DerivedRecord);

        // Inherited = true: DerivedRecord reports having BaseRecord's AuditableAttribute.
        var inheritedAuditable = derivedType.GetCustomAttribute<AuditableAttribute>();
        Console.WriteLine($"\nDerivedRecord + AuditableAttribute (Inherited=true):  " +
            (inheritedAuditable != null ? $"found (Level={inheritedAuditable.Level})" : "not found"));

        // Inherited = false: DerivedRecord does NOT report having BaseRecord's ClassSpecificAttribute.
        var notInherited = derivedType.GetCustomAttribute<ClassSpecificAttribute>();
        Console.WriteLine($"DerivedRecord + ClassSpecificAttribute (Inherited=false): " +
            (notInherited != null ? "found" : "not found"));

        Console.WriteLine();
        Console.WriteLine("Use Inherited=true for contracts the whole hierarchy must honor.");
        Console.WriteLine("Use Inherited=false for metadata specific to exactly one type.");
        Console.WriteLine("The default for [AttributeUsage] is Inherited=true.");
    }
}
```
