---
title: "Callbacks - The Receiving Method Owns When"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

internal static class Program
{
    private static readonly List<string> MatchedFiles = new List<string>();

    // Search fires callback once per match (with running count) and callback2 once at the end.
    // Search owns WHEN. The caller owns WHAT HAPPENS.
    private static void Search(string searchTerm, string directory,
        Action<int> callback, Action callback2)
    {
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"[Search] directory not found: {directory}");
            callback2();
            return;
        }

        int matched = 0;
        foreach (string path in Directory.GetFiles(directory))
        {
            if (!path.Contains(searchTerm)) continue;
            matched++;
            callback(matched);              // once per match
            MatchedFiles.Add(Path.GetFileName(path));
        }
        callback2();                        // once at the end
    }

    private static void OnMatch(int count)
    {
        Console.WriteLine($"Found {count} file(s) so far...");
        Thread.Sleep(100);  // deliberate delay so you can see each callback fire
    }

    private static void OnComplete()
    {
        Console.WriteLine($"\nSearch complete. {MatchedFiles.Count} match(es):");
        foreach (var name in MatchedFiles)
            Console.WriteLine($"  {name}");
    }

    private static void Main()
    {
        // Search the directory this program lives in for .dll files
        string dir = AppContext.BaseDirectory;
        Search(".dll", dir, OnMatch, OnComplete);

        Console.WriteLine("\n--- Same call with an inline lambda instead of a named method ---");
        MatchedFiles.Clear();
        Search(".dll", dir,
            count => Console.WriteLine($"Found {count}..."),
            OnComplete);
    }
}
```
