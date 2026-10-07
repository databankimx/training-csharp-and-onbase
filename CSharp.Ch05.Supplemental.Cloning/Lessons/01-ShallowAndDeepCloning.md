---
title: "Reference Assignment vs. Shallow vs. Deep Clone"
chapter: 5
index: 1
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

// A reference-type child object -- the key to understanding what "shallow" means.
public class Address
{
    public string Street { get; set; }
    public string City   { get; set; }
    public string State  { get; set; }

    public Address DeepClone() =>
        new Address { Street = Street, City = City, State = State };

    public override string ToString() => $"{Street}, {City}, {State}";
}

public class Person
{
    public string       Name        { get; set; }
    public int          Age         { get; set; }
    public Address      HomeAddress { get; set; }
    public List<string> Skills      { get; set; } = new List<string>();

    // MemberwiseClone copies the field values -- reference fields point to
    // the same child objects as the source.
    public Person ShallowClone() => (Person)MemberwiseClone();

    // Deep clone: every mutable child object is independently copied.
    public Person DeepClone() => new Person
    {
        Name        = Name,
        Age         = Age,
        HomeAddress = HomeAddress?.DeepClone(),
        Skills      = Skills == null ? null : new List<string>(Skills)
    };
}

internal static class Program
{
    private static void Main()
    {
        var original = new Person
        {
            Name = "Ada Lovelace",
            Age  = 36,
            HomeAddress = new Address { Street = "123 Example Street", City = "London", State = "England" },
            Skills = new List<string> { "Mathematics", "Programming" }
        };

        Console.WriteLine("1. REFERENCE ASSIGNMENT");
        Console.WriteLine("-----------------------");
        var assigned = original;
        Console.WriteLine($"ReferenceEquals(original, assigned): {ReferenceEquals(original, assigned)}");
        Console.WriteLine("No new Person was created -- both variables point to the exact same object.\n");

        Console.WriteLine("2. SHALLOW CLONE");
        Console.WriteLine("----------------");
        var shallow = original.ShallowClone();
        Console.WriteLine($"Same Person object:  {ReferenceEquals(original, shallow)}");
        Console.WriteLine($"Same Address object: {ReferenceEquals(original.HomeAddress, shallow.HomeAddress)}");
        Console.WriteLine($"Same Skills list:    {ReferenceEquals(original.Skills, shallow.Skills)}");
        Console.WriteLine();

        shallow.Name = "Shallow Copy";
        shallow.HomeAddress.City = "Chicago";
        shallow.Skills.Add("Shared-list surprise");

        Console.WriteLine("After changing the shallow clone:");
        Console.WriteLine($"  original.Name:             {original.Name}");
        Console.WriteLine($"  original.HomeAddress.City: {original.HomeAddress.City}");
        Console.WriteLine($"  original.Skills.Count:     {original.Skills.Count}");
        Console.WriteLine();
        Console.WriteLine("Name did not change -- string assignment replaces the clone's property value.");
        Console.WriteLine("City and Skills DID change -- Address and List are shared references.\n");

        // Restore before deep clone demo
        original.HomeAddress.City = "London";
        original.Skills.Remove("Shared-list surprise");

        Console.WriteLine("3. DEEP CLONE");
        Console.WriteLine("-------------");
        var deep = original.DeepClone();
        Console.WriteLine($"Same Person object:  {ReferenceEquals(original, deep)}");
        Console.WriteLine($"Same Address object: {ReferenceEquals(original.HomeAddress, deep.HomeAddress)}");
        Console.WriteLine($"Same Skills list:    {ReferenceEquals(original.Skills, deep.Skills)}");
        Console.WriteLine();

        deep.Name = "Deep Copy";
        deep.HomeAddress.City = "Chicago";
        deep.Skills.Add("Independent list");

        Console.WriteLine("After changing the deep clone:");
        Console.WriteLine($"  original.HomeAddress.City: {original.HomeAddress.City}");
        Console.WriteLine($"  original.Skills.Count:     {original.Skills.Count}");
        Console.WriteLine($"  deep.HomeAddress.City:     {deep.HomeAddress.City}");
        Console.WriteLine($"  deep.Skills.Count:         {deep.Skills.Count}");
        Console.WriteLine();
        Console.WriteLine("The deep clone owns independent copies -- changes do not affect the original.");
    }
}
```
