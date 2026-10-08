---
title: "First-Class Functions and Free Variables"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        // Three spellings, one idea: a function sitting in a variable.
        static string GreetLocal(string name) => $"Hello, {name}!";

        Func<string, string> greetDelegate = delegate (string name)
        {
            return $"Hello, {name}!";
        };

        Func<string, string> greetLambda = name => $"Hello, {name}!";

        Console.WriteLine(GreetLocal("Ada"));
        Console.WriteLine(greetDelegate("Ada"));
        Console.WriteLine(greetLambda("Ada"));

        // Now add a FREE VARIABLE -- one not defined inside the function,
        // borrowed from the outer scope.
        string salutation = "Hello";

        Func<string, string> greet = delegate (string name)
        {
            return $"{salutation}, {name}!";
        };

        Console.WriteLine(greet("Ada"));    // Hello, Ada!

        // Change the outer variable AFTER the function was created.
        // A closure holds the VARIABLE itself, not a snapshot of its value.
        salutation = "Howdy";

        Console.WriteLine(greet("Alan"));   // Howdy, Alan!  <-- reflects the change
    }
}
```
