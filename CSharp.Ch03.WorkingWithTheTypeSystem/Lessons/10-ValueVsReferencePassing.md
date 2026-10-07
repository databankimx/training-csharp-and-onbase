---
title: "Value Types and Reference Types Passed to a Method"
chapter: 3
index: 10
dependencies: []
---

```csharp
using System;

public class Student
{
    public string FirstName;
    public string LastName;
    public string Grade;
}

internal static class Program
{
    private static int Sum(int value1, int value2)
    {
        return value1 + value2;
    }

    private static void ChangeValues(int value1, int value2)
    {
        value1--;
        value2 += 5;
        Console.WriteLine("value1 is now " + value1);
        Console.WriteLine("value2 is now " + value2);
    }

    private static void ChangeName(Student refValue)
    {
        refValue.FirstName = "George";
    }

    private static void Main()
    {
        int num1 = 2;
        int num2 = 3;

        int result = Sum(value2: num2, value1: num1);
        Console.WriteLine($"Sum is: {result}");

        ChangeValues(num1, num2);
        Console.WriteLine(num1); // still 2 -- value type was copied
        Console.WriteLine(num2); // still 3 -- value type was copied

        var firstStudent = new Student { FirstName = "John", LastName = "Smith", Grade = "six" };
        ChangeName(firstStudent);
        Console.WriteLine(firstStudent.FirstName); // "George" -- reference type was mutated
    }
}
```
