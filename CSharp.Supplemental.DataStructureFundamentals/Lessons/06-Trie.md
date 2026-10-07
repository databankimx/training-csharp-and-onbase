---
title: "Trie - Prefix Tree"
chapter: 0
index: 6
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    // A small but representative word list - enough to demonstrate prefix search,
    // exact lookup, and autocomplete without needing an external file.
    private static readonly string[] Words =
    {
        "apple", "application", "apply", "apt",
        "bat", "batch", "bath", "bathroom", "battle",
        "can", "candy", "candle", "cannot", "cap", "cape", "car", "card", "care", "carry",
        "cat", "catch",
        "dog", "door", "double", "down", "draw", "drive",
        "ear", "early", "earth", "east", "eat",
        "face", "fact", "fail", "fair", "fall", "false", "far", "farm",
        "get", "give", "glass", "go", "goal", "good", "great", "green", "ground", "group",
        "hand", "hard", "have", "head", "heat", "help", "high", "hold", "home", "hope",
        "idea", "image", "in", "include", "inside",
        "job", "join", "just",
        "keep", "key", "kind", "know",
        "land", "large", "last", "late", "lead", "learn", "left", "let", "life", "light",
        "like", "line", "list", "live", "long", "look", "lose", "low",
        "main", "make", "man", "map", "mark", "mean", "meet", "mind", "miss", "more",
        "move", "much", "must",
        "name", "near", "need", "new", "next", "night", "note", "now",
        "object", "off", "offer", "old", "on", "open", "order", "other", "out", "over",
        "own",
        "part", "past", "path", "pay", "pick", "plan", "play", "point", "poor", "press",
        "price", "print", "problem", "process", "produce", "program", "provide", "public",
        "put",
        "question", "quick", "quite",
        "race", "range", "rate", "read", "real", "reason", "remain", "report", "rest",
        "result", "return", "right", "rise", "road", "role", "room", "round", "run",
        "safe", "same", "save", "say", "search", "seem", "self", "send", "sense", "serve",
        "set", "share", "short", "show", "sign", "simple", "since", "size", "small",
        "sort", "sound", "start", "state", "stay", "step", "still", "stop", "store",
        "street", "strong", "study", "such", "sure", "system",
        "take", "talk", "term", "test", "than", "that", "the", "then", "there", "they",
        "think", "this", "time", "to", "top", "town", "tree", "trie", "true", "try", "turn",
        "under", "until", "up", "use",
        "value", "view",
        "wait", "walk", "want", "watch", "water", "way", "well", "what", "when", "where",
        "which", "while", "who", "wide", "will", "win", "with", "word", "work", "world",
        "write",
        "year", "yet", "you",
    };

    private static void Main()
    {
        var trie = new Trie();
        foreach (var word in Words)
            trie.Insert(word);

        Console.WriteLine($"Loaded {Words.Length} words into the trie.");
        Console.WriteLine();

        // Exact lookup
        string[] lookups = { "trie", "tree", "apple", "application", "xyz", "sort", "sorting" };
        Console.WriteLine("Exact lookup:");
        foreach (var w in lookups)
            Console.WriteLine($"  \"{w}\" -> {(trie.Search(w) ? "found" : "not found")}");

        Console.WriteLine();

        // Prefix search (autocomplete)
        string[] prefixes = { "app", "car", "so", "tr", "pr" };
        Console.WriteLine("Autocomplete (words starting with prefix):");
        foreach (var prefix in prefixes)
        {
            var matches = trie.StartsWith(prefix);
            Console.WriteLine($"  \"{prefix}\" -> [{string.Join(", ", matches)}]");
        }

        Console.WriteLine();
        Console.WriteLine("Why a trie?");
        Console.WriteLine("  Exact lookup: O(m) where m = length of the word - no hash collisions,");
        Console.WriteLine("  no comparisons against unrelated keys, just one node per character.");
        Console.WriteLine("  Prefix search: O(m + k) where k = number of matches - a dictionary");
        Console.WriteLine("  or sorted list would need O(n) to enumerate all matching keys.");
    }
}

internal sealed class Trie
{
    private readonly TrieNode _root = new();

    public void Insert(string word)
    {
        var node = _root;
        foreach (char c in word)
        {
            if (!node.Children.TryGetValue(c, out var child))
            {
                child = new TrieNode();
                node.Children[c] = child;
            }
            node = child;
        }
        node.IsEndOfWord = true;
    }

    public bool Search(string word)
    {
        var node = FindNode(word);
        return node is not null && node.IsEndOfWord;
    }

    // Returns all words in the trie that start with the given prefix.
    public List<string> StartsWith(string prefix)
    {
        var results = new List<string>();
        var node    = FindNode(prefix);
        if (node is not null)
            Collect(node, prefix, results);
        return results;
    }

    private TrieNode? FindNode(string prefix)
    {
        var node = _root;
        foreach (char c in prefix)
        {
            if (!node.Children.TryGetValue(c, out var child)) return null;
            node = child;
        }
        return node;
    }

    private static void Collect(TrieNode node, string prefix, List<string> results)
    {
        if (node.IsEndOfWord) results.Add(prefix);
        foreach (var (ch, child) in node.Children)
            Collect(child, prefix + ch, results);
    }
}

internal sealed class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; } = new();
    public bool IsEndOfWord { get; set; }
}
```
