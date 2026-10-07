---
title: "Your First Struct"
chapter: 3
index: 4
dependencies: []
---

```csharp
using System;

public struct Person
{
    public string FirstName;
    public string LastName;
    public byte Age;

    public Person(string firstName, string lastName, byte age)
    {
        FirstName = firstName;
        LastName = lastName;
        Age = age;
    }

    public string Greet()
    {
        return $"Hello. My name is {FirstName} {LastName}. I am {Age} years old.";
    }
}

internal static class Program
{
    private static void Main()
    {
        var birth = new DateTime(1985, 6, 15);
        int age = DateTime.Today.Year - birth.Year;
        if (DateTime.Today.DayOfYear < birth.DayOfYear) age--;

        var me = new Person("Alex", "Turner", (byte)age);
        Console.WriteLine(me.Greet());
    }
}
```
