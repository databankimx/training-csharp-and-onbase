---
title: "A Method on a Class"
chapter: 3
index: 9
dependencies: ["08-ClassWithStaticField.md"]
---

```csharp
using System;

public class Student
{
    public static int StudentCount;
    public string FirstName;
    public string LastName;
    public string Grade;

    public string ConcatenateName()
    {
        string fullName = FirstName + " " + LastName;
        return fullName;
    }

    public void DisplayName()
    {
        string name = ConcatenateName();
        Console.WriteLine(name);
    }
}

internal static class Program
{
    private static void Main()
    {
        Student firstStudent = new() { FirstName = "John", LastName = "Smith", Grade = "six" };
        firstStudent.DisplayName();
    }
}
```
