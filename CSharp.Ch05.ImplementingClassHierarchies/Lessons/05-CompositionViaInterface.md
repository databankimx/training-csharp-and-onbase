---
title: "Composition via Interface - TeachingAssistant"
chapter: 5
index: 5
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

public class Person
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }
    public Person() { }
    public Person(string firstName, string lastName) { FirstName = firstName; LastName = lastName; }
    public string FullName() => $"{FirstName} {LastName}";
}

public class Employee : Person
{
    public string Department { get; set; }
    public Employee() { }
    public Employee(string firstName, string lastName) : base(firstName, lastName) { }
}

public enum Degree { Associate, Bachelor, Master, Doctorate }

public class Faculty : Employee
{
    public Degree Degree { get; set; }
    public Faculty() { }
    public Faculty(string firstName, string lastName, Degree degree)
        : base(firstName, lastName) { Degree = degree; }
}

public class Course
{
    public string Name     { get; set; }
    public int    RawGrade { get; set; }
    public string LetterGrade => RawGrade switch
    {
        >= 90 => "A", >= 80 => "B", >= 70 => "C", >= 60 => "D", _ => "F"
    };
}

public interface IStudent
{
    List<Course> Courses { get; set; }
    void PrintGrades();
}

public class Student : Person, IStudent
{
    public List<Course> Courses { get; set; }
    public void PrintGrades()
    {
        foreach (var c in Courses)
            Console.WriteLine($"{c.Name}: {c.LetterGrade} ({c.RawGrade})");
    }
}

public class TeachingAssistant : Faculty, IStudent
{
    private readonly Student _student = new();

    public string Credentials() =>
        $"TA {FirstName} {LastName} has a {Degree} degree.";

    public List<Course> Courses
    {
        get => _student.Courses;
        set => _student.Courses = value;
    }

    public void PrintGrades() => _student.PrintGrades();
}

internal static class Program
{
    private static void Main()
    {
        var ta = new TeachingAssistant
        {
            FirstName = "Linus",
            LastName  = "Torvalds",
            Degree    = Degree.Master,
            Courses   = [new Course { Name = "Operating Systems", RawGrade = 99 }]
        };
        Console.WriteLine(ta.Credentials());
        ta.PrintGrades();
    }
}
```
