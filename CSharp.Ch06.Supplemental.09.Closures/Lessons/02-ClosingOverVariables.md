---
title: "Closing Over Variables - State That Outlives Its Scope"
chapter: 6
index: 2
dependencies: []
---

```csharp
using System;

internal static class Program
{
    // A factory that returns a closure.
    // salutation and greetingCount are local to MakeGreeter -- they should
    // evaporate when MakeGreeter returns. They don't, because the returned
    // function closed over them, keeping them alive for as long as it lives.
    private static Func<string, string> MakeGreeter(string salutation)
    {
        int greetingCount = 0;

        return delegate (string name)
        {
            greetingCount++;
            return $"{salutation}, {name}! (greeting #{greetingCount})";
        };
    }

    private static void Main()
    {
        var greet = MakeGreeter("Hello");

        Console.WriteLine(greet("Ada"));   // Hello, Ada! (greeting #1)
        Console.WriteLine(greet("Alan"));  // Hello, Alan! (greeting #2)
        Console.WriteLine(greet("Grace")); // Hello, Grace! (greeting #3)

        // Each call to MakeGreeter produces an INDEPENDENT closure --
        // a fresh salutation and a fresh greetingCount.
        var greetEs = MakeGreeter("Hola");
        Console.WriteLine(greetEs("Ada")); // Hola, Ada! (greeting #1) -- its own counter

        // greet's counter is unaffected
        Console.WriteLine(greet("Linus")); // Hello, Linus! (greeting #4)
    }
}
```
