---
title: "Queue - FIFO, O(1) Enqueue and Dequeue"
chapter: 0
index: 4
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("--- Queue (FIFO) ---");

        LinkedListNode qHead = null, qTail = null;

        void Enqueue(int value)
        {
            var node = new LinkedListNode(value);
            if (qTail == null) { qHead = qTail = node; return; }
            qTail.Next = node;
            qTail = node;
        }

        int Dequeue()
        {
            if (qHead == null) throw new InvalidOperationException("Queue is empty.");
            int value = qHead.Value;
            qHead = qHead.Next;
            if (qHead == null) qTail = null;
            return value;
        }

        Enqueue(10); Enqueue(20); Enqueue(30);
        Console.WriteLine("Enqueued 10, 20, 30. Front is 10.");
        Console.WriteLine($"Dequeue: {Dequeue()} (first in, first out)");
        Console.WriteLine($"Dequeue: {Dequeue()}");
        Console.WriteLine($"Remaining front: {qHead.Value}");
        Console.WriteLine();
        Console.WriteLine("Key properties:");
        Console.WriteLine("  - Enqueue and Dequeue: O(1) - because we track both head and tail");
        Console.WriteLine("  - Without the tail pointer, Enqueue would walk to the end: O(n)");
        Console.WriteLine("  - FIFO order: first in, first out");
        Console.WriteLine("  - Use for: task queues, print spoolers, message buffers, BFS traversal");
        Console.WriteLine("  - BCL equivalent: System.Collections.Generic.Queue<T> (circular array internally)");
    }
}

internal sealed class LinkedListNode
{
    public int Value { get; }
    public LinkedListNode Next { get; set; }
    public LinkedListNode(int value) { Value = value; }
}
```
