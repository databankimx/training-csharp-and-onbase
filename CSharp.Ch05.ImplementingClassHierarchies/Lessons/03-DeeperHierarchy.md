---
title: "A Deeper Hierarchy"
chapter: 5
index: 3
dependencies: []
---

```csharp
using System;

public class Person
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }

    public Person() { }
    public Person(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName  = lastName;
    }

    public string FullName(bool lastFirst = false) =>
        lastFirst ? $"{LastName}, {FirstName}" : $"{FirstName} {LastName}";
}

public class Employee : Person
{
    public string Department { get; set; }

    public Employee() { }
    public Employee(string firstName, string lastName) : base(firstName, lastName) { }
    public Employee(string firstName, string lastName, string department)
        : base(firstName, lastName) { Department = department; }
}

public enum Degree { Associate, Bachelor, Master, Doctorate }

public class Faculty : Employee
{
    public Degree Degree { get; set; }

    public Faculty() { }
    public Faculty(string firstName, string lastName, Degree degree)
        : base(firstName, lastName) { Degree = degree; }
}

internal static class Program
{
    private static void Main()
    {
        var faculty = new Faculty("Charles", "Babbage", Degree.Doctorate);
        Console.WriteLine($"{faculty.FullName()} -- {faculty.Department ?? "no department"}, {faculty.Degree}");
    }
}
```
