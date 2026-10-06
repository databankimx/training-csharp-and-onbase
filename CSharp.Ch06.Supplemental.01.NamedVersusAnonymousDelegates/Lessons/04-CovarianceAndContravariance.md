---
title: "Covariance and Contravariance"
chapter: 6
index: 4
dependencies: []
---

```csharp
using System;

public class Person   { public string Name { get; set; } }
public class Employee : Person { }

internal static class Program
{
    private static void Main()
    {
        // COVARIANCE: a method returning Employee satisfies Func<Person>
        // Safe because every Employee IS-A Person -- the caller asked for a Person and got one.
        Func<Person> returnPersonMethod = ReturnEmployee;
        var person = returnPersonMethod();
        Console.WriteLine($"Type: {person.GetType().Name}, Name: {person.Name}");
        // Runtime type is still Employee -- covariance loosened the declared type, not the object.

        // CONTRAVARIANCE: a method taking Person satisfies Action<Employee>
        // Safe because the caller will pass an Employee and Employee IS-A Person.
        Action<Employee> employeeParameterMethod = PersonParameter;
        var employee = new Employee();
        employeeParameterMethod(employee);
        Console.WriteLine($"Type: {employee.GetType().Name}, Name: {employee.Name}");

        // The reverse of either breaks.
        // Action<Person> p = EmployeeOnlyMethod;  // compile error -- would be handed a plain Person
        // Func<Employee> e = ReturnPerson;        // compile error -- caller might need Employee members
    }

    private static Employee ReturnEmployee() => new Employee { Name = "Jane" };

    private static void PersonParameter(Person p) { p.Name = "John Smith"; }
}
```
