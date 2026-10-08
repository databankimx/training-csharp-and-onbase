---
title: "A Class and Its Static Field"
chapter: 3
index: 8
dependencies: []
---

```csharp
using System;

public class Student
{
    public static int StudentCount;
    public string FirstName;
    public string LastName;
    public string Grade;
}

internal static class Program
{
    private static void Main()
    {
        Student firstStudent = new();
        Student.StudentCount++;
        Student secondStudent = new();
        Student.StudentCount++;

        firstStudent.FirstName = "John";
        firstStudent.LastName = "Smith";
        firstStudent.Grade = "six";

        secondStudent.FirstName = "Tom";
        secondStudent.LastName = "Thumb";
        secondStudent.Grade = "two";

        Console.WriteLine(firstStudent.FirstName);
        Console.WriteLine(secondStudent.FirstName);
        Console.WriteLine(Student.StudentCount);
    }
}
```
