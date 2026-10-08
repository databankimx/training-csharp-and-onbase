---
title: "The foreach Loop and Average Grades"
chapter: 2
index: 12
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // Basic foreach loop
        int[] numbers = [5, 10, 15, 20];
        foreach (int number in numbers)
        {
            Console.WriteLine($"number / 5 = {number / 5}");
        }

        // Accumulator pattern -- average grades
        int[] arrGrades = [78, 89, 90, 76, 98, 65];
        int total = 0;
        int gradeCount = 0;
        double average;

        foreach (int grade in arrGrades)
        {
            total += grade;
            gradeCount++;
        }

        if (gradeCount == 0) total = gradeCount = 1;

        average = (double)total / gradeCount;
        Console.WriteLine($"Average grade = {average}");
    }
}
```
