---
title: "ICloneable - Three Implementations"
chapter: 5
index: 9
dependencies: []
---

```csharp
using System;

public class PersonCloneable : ICloneable
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }
    public PersonCloneable Manager { get; set; }

    public object Clone()
    {
        // Option 1 -- hand-written shallow clone:
        // return new PersonCloneable { FirstName = FirstName, LastName = LastName, Manager = Manager };

        // Option 2 -- MemberwiseClone() shallow clone (same result, less code):
        // return MemberwiseClone();

        // Option 3 -- deep clone (Manager is recursively cloned, not shared):
        return new PersonCloneable
        {
            FirstName = FirstName,
            LastName  = LastName,
            Manager   = (PersonCloneable)Manager?.Clone()
        };
    }
}

internal static class Program
{
    private static void Main()
    {
        var boss = new PersonCloneable { FirstName = "Ada",  LastName = "Lovelace" };
        var bob  = new PersonCloneable { FirstName = "Bob",  LastName = "Smith", Manager = boss };

        // Plain assignment -- no copy at all
        var anne = bob;
        anne.FirstName = "Anne";
        Console.WriteLine(bob.FirstName); // "Anne" -- same object

        // Clone -- genuinely independent copy
        var robert = (PersonCloneable)bob.Clone();
        robert.FirstName = "Robert";
        Console.WriteLine(bob.FirstName); // still "Anne"

        // Deep clone: Manager is also independent
        robert.Manager.FirstName = "Changed";
        Console.WriteLine(bob.Manager.FirstName); // still "Ada"
    }
}
```
