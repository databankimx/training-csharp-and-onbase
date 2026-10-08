---
title: "IEquatable"
chapter: 5
index: 8
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

public class PersonWithEquality : IEquatable<PersonWithEquality>
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }

    public PersonWithEquality(string first, string last)
    {
        FirstName = first;
        LastName  = last;
    }

    public bool Equals(PersonWithEquality other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(FirstName, other.FirstName, StringComparison.CurrentCultureIgnoreCase)
            && string.Equals(LastName,  other.LastName,  StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(object obj) => Equals(obj as PersonWithEquality);

    public override int GetHashCode() =>
        HashCode.Combine(
            FirstName?.ToUpperInvariant(),
            LastName?.ToUpperInvariant());
}

internal static class Program
{
    private static void Main()
    {
        var abe    = new PersonWithEquality("Abraham", "Lincoln");
        var lincoln = new PersonWithEquality("Abraham", "Lincoln");

        Console.WriteLine(abe == lincoln);         // False -- reference equality
        Console.WriteLine(abe.Equals(lincoln));    // True  -- value equality via IEquatable

        var people = new List<PersonWithEquality> { abe };
        Console.WriteLine(people.Contains(lincoln)); // True -- Contains uses Equals()
    }
}
```
