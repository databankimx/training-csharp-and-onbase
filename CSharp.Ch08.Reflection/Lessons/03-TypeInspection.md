---
title: "Type - Constructors, Fields, Properties, Methods"
chapter: 8
index: 3
dependencies: []
---

```csharp
using System;
using System.Linq;
using System.Reflection;

public enum Degree { Associates, Bachelors, Masters, Doctorate }

public class Person
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }
    private int _internalId;

    public Person() { }
    public Person(string firstName) { FirstName = firstName; }
    public Person(string firstName, string lastName) { FirstName = firstName; LastName = lastName; }

    public string FullName() => $"{FirstName} {LastName}";
}

public class Employee : Person
{
    public string Department { get; set; }
}

internal static class Program
{
    private static void Main()
    {
        // --- Constructors ---
        var personType = typeof(Person);
        Console.WriteLine($"Constructors on {personType.Name}:");
        foreach (var ctor in personType.GetConstructors())
        {
            string ps = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            Console.WriteLine($"  {personType.Name}({ps})");
        }
        // ParameterInfo preserves parameter NAMES -- that's what DI containers and model binding use.

        // --- Fields: BindingFlags governs what you get ---
        Console.WriteLine($"\nPublic fields on {personType.Name}: {personType.GetFields().Length}");
        // Passing any BindingFlags REPLACES the defaults entirely.
        // You must specify both a visibility flag (Public/NonPublic) AND a scope flag (Instance/Static).
        var allFields = personType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Console.WriteLine($"All instance fields (public + non-public): {allFields.Length}");
        foreach (var f in allFields) Console.WriteLine($"  {f.FieldType.Name} {f.Name}");

        // --- Properties (inherited by default) ---
        var employeeType = typeof(Employee);
        Console.WriteLine($"\nProperties on {employeeType.Name} (including inherited):");
        foreach (var p in employeeType.GetProperties())
            Console.WriteLine($"  {p.PropertyType.Name} {p.Name}  (declared on {p.DeclaringType?.Name})");

        // --- Methods (DeclaredOnly to skip everything from object/base) ---
        Console.WriteLine($"\nMethods declared directly on {personType.Name}:");
        foreach (var m in personType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            Console.WriteLine($"  {m.ReturnType.Name} {m.Name}()");

        // --- Invoke a method via reflection ---
        var person = new Person("Ada", "Lovelace");
        var fullNameMethod = personType.GetMethod("FullName");
        var result = fullNameMethod?.Invoke(person, null);
        Console.WriteLine($"\nInvoked FullName() via reflection: {result}");

        // --- Enum ---
        var degreeType = typeof(Degree);
        Console.WriteLine($"\nDegree enum values:");
        foreach (var v in degreeType.GetEnumValues())
            Console.WriteLine($"  {(int)v}: {v}");
    }
}
```
