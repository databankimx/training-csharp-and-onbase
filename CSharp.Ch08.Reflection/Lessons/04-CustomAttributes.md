---
title: "Custom Attributes - Define, Apply, Read"
chapter: 8
index: 4
dependencies: []
---

```csharp
using System;
using System.Reflection;

// The class must inherit from Attribute -- that's what makes it an attribute.
// The "Attribute" suffix is dropped at the usage site: applied as [CourseCatalog(...)].
// [AttributeUsage] constrains where it can be applied and how many times.
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CourseCatalogAttribute : Attribute
{
    public string Department  { get; }
    public int    CreditHours { get; }

    // Properties are get-only: attribute values are compile-time constants baked into
    // assembly metadata. You can't pass a computed value or a "new" object as an argument.
    public CourseCatalogAttribute(string department, int creditHours)
    {
        Department  = department;
        CreditHours = creditHours;
    }
}

// Applying it: the "Attribute" suffix is optional at the usage site.
// AttributeTargets.Class means applying this to a method would be a compile ERROR, not a runtime surprise.
[CourseCatalog("Computer Science", 3)]
public class Course
{
    public string Name { get; set; }
}

public class CourseWithout
{
    public string Name { get; set; }
}

internal static class Program
{
    private static void Main()
    {
        // Reading an attribute: GetCustomAttribute<T>() instantiates it from metadata.
        // Returns null if the attribute isn't present -- does not throw.
        var courseType = typeof(Course);
        var attr = courseType.GetCustomAttribute<CourseCatalogAttribute>();

        if (attr != null)
            Console.WriteLine($"{courseType.Name}: department={attr.Department}, credits={attr.CreditHours}");
        else
            Console.WriteLine($"{courseType.Name} has no CourseCatalogAttribute.");

        // A type without the attribute returns null
        var withoutAttr = typeof(CourseWithout).GetCustomAttribute<CourseCatalogAttribute>();
        Console.WriteLine($"CourseWithout: {(withoutAttr != null ? "found" : "not found")}");
        Console.WriteLine();

        // IsDefined(): presence-only check, never allocates an attribute instance.
        // Use this when you only need yes/no -- it's cheaper than GetCustomAttribute.
        bool hasCatalog = Attribute.IsDefined(typeof(Course), typeof(CourseCatalogAttribute));
        Console.WriteLine($"IsDefined<CourseCatalogAttribute> on Course: {hasCatalog}");
        Console.WriteLine();

        // Attributes are the foundation of declarative programming in .NET:
        // [Serializable], [Obsolete], [TestMethod], [Required], [JsonProperty], [HttpGet]
        // all work by this same mechanism -- metadata attached to code, read back via reflection.
        Console.WriteLine("Attributes are data embedded in assembly metadata, not documentation.");
        Console.WriteLine("GetCustomAttribute<T>() instantiates the attribute lazily, on read.");
    }
}
```
