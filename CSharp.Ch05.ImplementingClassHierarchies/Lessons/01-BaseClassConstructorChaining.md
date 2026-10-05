---
title: "A Base Class With Constructor Chaining"
chapter: 5
index: 1
dependencies: []
---

```csharp
using System;

public class Person
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }
    public Person Manager   { get; set; }

    public Person() { }

    public Person(string firstName)
    {
        if (string.IsNullOrEmpty(firstName))
            throw new ArgumentOutOfRangeException(nameof(firstName), "FirstName must not be null or blank!");
        FirstName = firstName;
    }

    public Person(string firstName, string lastName) : this(firstName)
    {
        if (string.IsNullOrEmpty(lastName))
            throw new ArgumentOutOfRangeException(nameof(lastName), "LastName must not be null or blank!");
        LastName = lastName;
    }

    public string FullName(bool lastFirst = false) =>
        lastFirst ? $"{LastName}, {FirstName}" : $"{FirstName} {LastName}";
}

internal static class Program
{
    private static void Main()
    {
        var person = new Person("Ada", "Lovelace");
        Console.WriteLine(person.FullName());
        Console.WriteLine(person.FullName(lastFirst: true));

        try
        {
            var bad = new Person("");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
```
