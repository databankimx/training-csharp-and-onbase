---
title: "Exception Tests"
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
        var ex = Capture<ArgumentException>(() => NormalizeDocumentType(" "));
        Console.WriteLine(ex.ParamName == "documentType"
            ? "PASS: parameter name verified"
            : $"FAIL: expected documentType, got {ex.ParamName}");
    }

    private static string NormalizeDocumentType(string documentType)
    {
        if (string.IsNullOrWhiteSpace(documentType))
            throw new ArgumentException("Document type is required.", nameof(documentType));

        return documentType.Trim().ToUpperInvariant();
    }

    private static TException Capture<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException ex)
        {
            return ex;
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
    }
}
```
