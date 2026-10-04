---
title: "The Array Covariance Trap"
chapter: 4
index: 6
dependencies: []
---

```csharp
using System;

public class Person
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }
    public Person(string firstName, string lastName) { FirstName = firstName; LastName = lastName; }
}

public class Employee : Person
{
    public string Department { get; set; }
    public string JobTitle   { get; set; }
    public Employee(string firstName, string lastName, string department, string jobTitle)
        : base(firstName, lastName) { Department = department; JobTitle = jobTitle; }
}

public class Manager : Employee
{
    public Manager(string firstName, string lastName, string department, string jobTitle)
        : base(firstName, lastName, department, jobTitle) { }
}

internal static class Program
{
    private static void Main()
    {
        Employee[] employees = { new("Joe", "Programmer", "Development", "Software Engineer") };
        Person[] persons = employees;

        Manager[] managers = persons as Manager[];
        Console.WriteLine(managers == null ? "as returned null" : "converted");

        try
        {
            managers = (Manager[])persons;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetType().Name);
        }
    }
}
```
