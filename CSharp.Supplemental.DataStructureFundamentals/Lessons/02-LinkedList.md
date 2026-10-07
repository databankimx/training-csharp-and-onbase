---
title: "Linked List - Dynamic Size, O(1) Insert at Head"
chapter: 0
index: 2
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("--- Linked List ---");

        LinkedListNode head = null;

        // Inserting at the head is just a couple of reference reassignments - O(1),
        // regardless of how many nodes are already in the list.
        foreach (int value in new[] { 30, 20, 10 })
        {
            var node = new LinkedListNode(value) { Next = head };
            head = node;
            Console.WriteLine($"Inserted {value} at the head");
        }

        Console.Write("Traversal (walks node by node - no index jump): ");
        var current = head;
        while (current != null)
        {
            Console.Write($"{current.Value} ");
            current = current.Next;
        }
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Key properties:");
        Console.WriteLine("  - Insert at head: O(1)");
        Console.WriteLine("  - Access by position: O(n) - must walk from the head");
        Console.WriteLine("  - No fixed size - grows as nodes are added");
        Console.WriteLine("  - Nodes are not contiguous in memory (poorer cache locality than arrays)");
    }
}

internal sealed class LinkedListNode
{
    public int Value { get; }
    public LinkedListNode Next { get; set; }
    public LinkedListNode(int value) { Value = value; }
}
```
