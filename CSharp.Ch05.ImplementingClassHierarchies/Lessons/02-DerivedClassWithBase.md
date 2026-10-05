---
title: "A Derived Class With base()"
chapter: 5
index: 2
dependencies: []
---

```csharp
using System;

public class Person
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }

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

public class Employee : Person
{
    public string Department { get; set; }

    public Employee() { }
    public Employee(string firstName) : base(firstName) { }
    public Employee(string firstName, string lastName) : base(firstName, lastName) { }

    public Employee(string firstName, string lastName, string department) : base(firstName, lastName)
    {
        if (string.IsNullOrEmpty(department))
            throw new ArgumentOutOfRangeException(nameof(department), "Department must not be null or blank!");
        Department = department;
    }
}

internal static class Program
{
    private static void Main()
    {
        var employee = new Employee("Grace", "Hopper", "Engineering");
        Console.WriteLine($"{employee.FullName()} - {employee.Department}");

        Person person = employee;
        Console.WriteLine(person.GetType().Name);
    }
}
```
