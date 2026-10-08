---
title: "Generic Collections - List, Dictionary, Queue, Stack"
chapter: 9
index: 3
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

public class Book
{
    public string Title  { get; set; }
    public string Author { get; set; }
    public int    Year   { get; set; }
    public Book(string title, string author, int year) { Title = title; Author = author; Year = year; }
    public override string ToString() => $"{Title} by {Author} ({Year})";
}

internal static class Program
{
    private static void Main()
    {
        // --- List<T>: general-purpose ordered, indexed, resizable collection ---
        // Internally an array that doubles capacity when full.
        // Indexing and appending: O(1). Insert/remove in the middle: O(n).
        var books = new List<Book>
        {
            new("Fahrenheit 451", "Ray Bradbury",  1953),
            new("1984",           "George Orwell", 1949),
            new("Brave New World","Aldous Huxley", 1932)
        };
        books.Sort((a, b) => a.Year.CompareTo(b.Year));
        Console.WriteLine("List<Book> sorted by year:");
        foreach (var b in books) Console.WriteLine($"  {b}");
        Console.WriteLine($"Find Bradbury: {books.Find(b => b.Author == "Ray Bradbury")}");
        Console.WriteLine($"Exists pre-1940: {books.Exists(b => b.Year < 1940)}");

        // --- Dictionary<TKey,TValue>: O(1) key-based lookup ---
        // Use TryGetValue() -- it hashes the key once and handles a miss without throwing.
        // ContainsKey() + indexer hashes TWICE; the bare indexer throws KeyNotFoundException.
        // Enumeration order is NOT guaranteed.
        Console.WriteLine("\nDictionary<string, Book>:");
        var byTitle = new Dictionary<string, Book>
        {
            ["1984"]           = new("1984",           "George Orwell", 1949),
            ["Fahrenheit 451"] = new("Fahrenheit 451", "Ray Bradbury",  1953)
        };
        if (byTitle.TryGetValue("1984", out var found))
            Console.WriteLine($"  TryGetValue(\"1984\"): {found}");
        Console.WriteLine($"  ContainsKey(\"Dune\"): {byTitle.ContainsKey("Dune")}");

        // --- Queue<T>: FIFO ---
        // Dequeue() throws InvalidOperationException when empty; use TryDequeue() to avoid.
        Console.WriteLine("\nQueue<Book> (FIFO):");
        var queue = new Queue<Book>();
        queue.Enqueue(new("1984",            "George Orwell", 1949));
        queue.Enqueue(new("Brave New World", "Aldous Huxley", 1932));
        Console.WriteLine($"  Dequeue: {queue.Dequeue()}   <-- first in");

        // --- Stack<T>: LIFO ---
        // Pop() throws InvalidOperationException when empty; use TryPop() to avoid.
        Console.WriteLine("\nStack<Book> (LIFO):");
        var stack = new Stack<Book>();
        stack.Push(new("1984",            "George Orwell", 1949));
        stack.Push(new("Brave New World", "Aldous Huxley", 1932));
        Console.WriteLine($"  Pop: {stack.Pop()}   <-- last in");
    }
}
```
