---
title: "Assembly.CreateInstance - Dynamic Object Creation"
chapter: 8
index: 2
dependencies: []
---

```csharp
using System;
using System.Reflection;

// A simple class we'll instantiate via reflection
public class Greeter
{
    public string Name { get; set; }
    public Greeter() { }
    public Greeter(string name) { Name = name; }
    public string Hello() => $"Hello, {Name}!";
}

internal static class Program
{
    private static void Main()
    {
        // Assembly.CreateInstance() takes a fully namespace-qualified class name.
        // A typo or wrong name returns NULL -- it does NOT throw.
        // The ?? throw is not defensive padding -- it is the only thing preventing a
        // NullReferenceException several stack frames later.
        var asm = Assembly.GetExecutingAssembly();
        var greeter = (Greeter)(asm.CreateInstance("Greeter")
            ?? throw new InvalidOperationException("Type not found -- check the fully qualified name!"));

        greeter.Name = "Reflection";
        Console.WriteLine(greeter.Hello());

        // Activator.CreateInstance is often more convenient -- same concept, no Assembly required
        var greeter2 = (Greeter)Activator.CreateInstance(typeof(Greeter), "Activator");
        Console.WriteLine(greeter2.Hello());
        Console.WriteLine();

        // Every reflective lookup that searches by string can return null on a miss.
        // Handle it at the call site every time, not somewhere downstream.
        Console.WriteLine("Rule: every GetXxx() / CreateInstance() / Invoke() can return null.");
        Console.WriteLine("      Handle it where you call it, not three frames later.");
    }
}
```
