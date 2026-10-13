---
title: "Remove Decorative Comments"
chapter: 0
index: 3
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        var rows = new[] { "A100", "VOID-B200", "C300", "VOID-D400" };
        foreach (string id in GetBillableDocumentIds(rows))
            Console.WriteLine(id);
    }

    private static IEnumerable<string> GetBillableDocumentIds(IEnumerable<string> documentIds)
    {
        foreach (string documentId in documentIds)
        {
            // VOID rows are retained in the source export for audit history, but are not billable.
            if (documentId.StartsWith("VOID-", StringComparison.OrdinalIgnoreCase))
                continue;

            yield return documentId;
        }
    }
}
```
