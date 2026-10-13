---
title: "Improve Security Posture"
chapter: 0
index: 4
dependencies: []
---

```csharp
using System;
using System.IO;

internal static class Program
{
    private static void Main()
    {
        TryResolve(@"C:\DataBank\Imports", "today.csv");
        TryResolve(@"C:\DataBank\Imports", @"..\secrets.txt");
    }

    private static string ResolveImportPath(string baseDirectory, string fileName)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
            throw new ArgumentException("Base directory is required.", nameof(baseDirectory));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));

        string root = Path.GetFullPath(baseDirectory);
        string candidate = Path.GetFullPath(Path.Combine(root, fileName));

        if (!candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Requested file is outside the import directory.");

        return candidate;
    }

    private static void TryResolve(string root, string fileName)
    {
        try
        {
            Console.WriteLine(ResolveImportPath(root, fileName));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
        }
    }
}
```
