---
title: "dynamic Beyond COM"
chapter: 4
index: 18
dependencies: []
---

```csharp
using System;
using System.Text.Json;

internal static class Program
{
    private static void Main()
    {
        const string json = "{\"Id\":\"1234-5678\",\"Data\":{\"FirstName\":\"Maria\",\"LastName\":\"Warden\"}}";
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        string id        = root.GetProperty("Id").GetString();
        string firstName = root.GetProperty("Data").GetProperty("FirstName").GetString();
        string lastName  = root.GetProperty("Data").GetProperty("LastName").GetString();
        Console.WriteLine($"{id}: {firstName} {lastName}");
    }
}
```
