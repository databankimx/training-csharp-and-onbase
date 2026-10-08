---
title: "Generics - a Queue and a Stack"
chapter: 3
index: 11
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

public abstract class BaseStackOrQueue<T>
{
    protected readonly List<T> Values = [];

    public void Add(T obj) => Values.Add(obj);
    public abstract T Next();
    public bool Waiting() => Values.Count > 0;
}

public class GenericQueue<T> : BaseStackOrQueue<T>
{
    public override T Next()
    {
        if (Values.Count <= 0) throw new IndexOutOfRangeException("GenericQueue is empty!");
        var ret = Values[0];
        Values.RemoveAt(0);
        return ret;
    }
}

public class GenericStack<T> : BaseStackOrQueue<T>
{
    public override T Next()
    {
        if (Values.Count <= 0) throw new IndexOutOfRangeException("GenericStack is empty!");
        var ret = Values[Values.Count - 1];
        Values.RemoveAt(Values.Count - 1);
        return ret;
    }
}

internal static class Program
{
    private static void Main()
    {
        var queue = new GenericQueue<string>();
        queue.Add("Alex");
        queue.Add("Andy");
        queue.Add("Alan");

        Console.WriteLine("Queue (first in, first out):");
        while (queue.Waiting())
            Console.WriteLine($"Now serving {queue.Next()}");

        var stack = new GenericStack<string>();
        stack.Add("Alex");
        stack.Add("Andy");
        stack.Add("Alan");

        Console.WriteLine("Stack (last in, first out):");
        while (stack.Waiting())
            Console.WriteLine($"Now serving {stack.Next()}");
    }
}
```
