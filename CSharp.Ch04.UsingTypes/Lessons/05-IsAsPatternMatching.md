---
title: "is, as, and Pattern Matching"
chapter: 4
index: 5
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
}

public class Employee : Person
{
    public string Department { get; set; }
    public string JobTitle   { get; set; }

    public Employee(string firstName, string lastName, string department, string jobTitle)
        : base(firstName, lastName)
    {
        Department = department;
        JobTitle   = jobTitle;
    }
}

internal static class Program
{
    private static void Main()
    {
        var employee = new Employee("Joe", "Programmer", "Development", "Software Engineer");
        Person person = employee;

        Console.WriteLine(person is Employee ? "yes" : "no");

        if (person is Employee emp)
        {
            Console.WriteLine($"{emp.FirstName} is a {emp.JobTitle}");
        }
    }
}
```
