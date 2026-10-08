---
title: "Switch Statements"
chapter: 2
index: 10
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        string condition = "Hello";
        Console.WriteLine($"condition = {condition}");

        switch (condition)
        {
            case "Good Morning":
                Console.WriteLine("Good morning to you!");
                break;
            case "Hello":
                Console.WriteLine("Hello to you too.");
                break;
            case "Good Evening":
                Console.WriteLine("Have a wonderful evening!");
                break;
            default:
                Console.WriteLine("Good bye...");
                break;
        }

        var r = new Random();
        int number = r.Next(0, 9);
        switch (number)
        {
            case 0:
            case 1:
                Console.WriteLine($"Number [{number}] could be binary, octal, or decimal.");
                break;
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
                Console.WriteLine($"Number [{number}] could be octal or decimal.");
                break;
            default:
                Console.WriteLine($"Number [{number}] must be decimal.");
                break;
        }
    }
}
```
