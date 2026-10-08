---
title: "Static vs. Instance Delegate Binding"
chapter: 6
index: 3
dependencies: []
---

```csharp
using System;

public delegate string GetStringDelegate();

public class Person
{
    public string Name { get; set; }
    public GetStringDelegate InstanceMethod;
    public GetStringDelegate StaticMethod;

    public string GetName() => Name;
    public static string StaticName() => "Static";
}

internal static class Program
{
    private static void Main()
    {
        var alice = new Person { Name = "Alice" };
        var bob   = new Person { Name = "Bob" };

        // Instance delegate carries the TARGET OBJECT, not just the method
        alice.InstanceMethod = alice.GetName;
        alice.StaticMethod   = Person.StaticName;

        // Bob's field points at Alice's method -- so it returns Alice's name
        bob.InstanceMethod   = alice.GetName;
        bob.StaticMethod     = Person.StaticName;

        Console.WriteLine("Alice's InstanceMethod: " + alice.InstanceMethod()); // Alice
        Console.WriteLine("Bob's InstanceMethod:   " + bob.InstanceMethod());   // Alice (!)
        Console.WriteLine("Alice's StaticMethod:   " + alice.StaticMethod());   // Static
        Console.WriteLine("Bob's StaticMethod:     " + bob.StaticMethod());     // Static

        // Consequence: a delegate holding an instance method keeps that object alive.
        // A long-lived subscriber pointing at a short-lived object is the most common
        // managed memory leak in .NET.
    }
}
```
