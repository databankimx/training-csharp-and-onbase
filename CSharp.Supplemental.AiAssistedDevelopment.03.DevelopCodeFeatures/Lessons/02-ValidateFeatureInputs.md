---
title: "Add Feature Validation"
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
        Console.WriteLine(CreateDocumentKey("AP", 42));
        TryCreateDocumentKey("", 42);
        TryCreateDocumentKey("AP", 0);
    }

    private static string CreateDocumentKey(string documentType, int documentId)
    {
        if (string.IsNullOrWhiteSpace(documentType))
            throw new ArgumentException("Document type is required.", nameof(documentType));

        if (documentId <= 0)
            throw new ArgumentOutOfRangeException(nameof(documentId), documentId, "Document ID must be positive.");

        return $"{documentType.Trim().ToUpperInvariant()}-{documentId:D6}";
    }

    private static void TryCreateDocumentKey(string documentType, int documentId)
    {
        try
        {
            Console.WriteLine(CreateDocumentKey(documentType, documentId));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
        }
    }
}
```
