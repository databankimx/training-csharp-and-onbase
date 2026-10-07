---
title: "Stack - LIFO, O(1) Push and Pop"
chapter: 0
index: 3
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("--- Stack (LIFO) ---");

        LinkedListNode top = null;

        void Push(int value) => top = new LinkedListNode(value) { Next = top };

        int Pop()
        {
            if (top == null) throw new InvalidOperationException("Stack is empty.");
            int value = top.Value;
            top = top.Next;
            return value;
        }

        int Peek() => top?.Value ?? throw new InvalidOperationException("Stack is empty.");

        Push(10); Push(20); Push(30);
        Console.WriteLine($"Pushed 10, 20, 30. Top (Peek): {Peek()}");
        Console.WriteLine($"Pop: {Pop()} (last in, first out)");
        Console.WriteLine($"Pop: {Pop()}");
        Console.WriteLine($"Top after two pops: {Peek()}");
        Console.WriteLine();
        Console.WriteLine("Key properties:");
        Console.WriteLine("  - Push and Pop: O(1) - only the top node changes");
        Console.WriteLine("  - LIFO order: last in, first out");
        Console.WriteLine("  - Use for: call frames, undo history, expression evaluation, DFS traversal");
        Console.WriteLine("  - BCL equivalent: System.Collections.Generic.Stack<T> (uses an array internally)");
    }
}

internal sealed class LinkedListNode
{
    public int Value { get; }
    public LinkedListNode Next { get; set; }
    public LinkedListNode(int value) { Value = value; }
}
```
