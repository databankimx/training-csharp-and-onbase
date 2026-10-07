---
title: "AllowMultiple - Stacking the Same Attribute Type"
chapter: 8
index: 1
dependencies: []
---

```csharp
using System;
using System.Linq;
using System.Reflection;

// AllowMultiple = true lets this attribute be stacked multiple times on one target.
// This is how ORMs and serializers attach full column-mapping tables as metadata.
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DataMappingAttribute : Attribute
{
    public string ColumnName   { get; }
    public string PropertyName { get; }
    public DataMappingAttribute(string columnName, string propertyName)
    {
        ColumnName   = columnName;
        PropertyName = propertyName;
    }
}

// Three stacked DataMapping attributes on one class.
[DataMapping("cust_id",    "Id")]
[DataMapping("cust_name",  "Name")]
[DataMapping("cust_email", "Email")]
public class CustomerRecord
{
    public int    Id    { get; set; }
    public string Name  { get; set; }
    public string Email { get; set; }
}

internal static class Program
{
    private static void Main()
    {
        var recordType = typeof(CustomerRecord);

        // GetCustomAttribute<T>() (singular) would throw AmbiguousMatchException here --
        // there are three DataMappingAttributes, not one.
        // GetCustomAttributes<T>() (plural) returns all of them.
        var mappings = recordType.GetCustomAttributes<DataMappingAttribute>().ToList();

        Console.WriteLine($"{recordType.Name} carries {mappings.Count} DataMappingAttribute instance(s):");
        foreach (var m in mappings)
            Console.WriteLine($"  Column '{m.ColumnName}' -> property '{m.PropertyName}'");

        Console.WriteLine();
        Console.WriteLine("Rule: use the PLURAL GetCustomAttributes<T>() for AllowMultiple attributes.");
        Console.WriteLine("      The singular form throws AmbiguousMatchException when there is more than one.");
    }
}
```
