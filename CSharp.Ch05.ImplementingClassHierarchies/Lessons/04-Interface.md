---
title: "An Interface"
chapter: 5
index: 4
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

    public string FullName(bool lastFirst = false) =>
        lastFirst ? $"{LastName}, {FirstName}" : $"{FirstName} {LastName}";
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
        foreach (var course in Courses)
            Console.WriteLine($"{course.Name}: {course.LetterGrade} ({course.RawGrade})");
    }
}

internal static class Program
{
    private static void Main()
    {
        var student = new Student
        {
            FirstName = "Alan",
            LastName  = "Turing",
            Courses   =
            [
                new Course { Name = "Computer Science", RawGrade = 98 },
                new Course { Name = "Mathematics",      RawGrade = 95 },
            ]
        };
        Console.WriteLine(student.FullName());
        student.PrintGrades();
    }
}
```
