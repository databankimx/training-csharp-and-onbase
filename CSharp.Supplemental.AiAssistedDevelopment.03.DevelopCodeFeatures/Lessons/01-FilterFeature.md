---
title: "Implement a Small Feature"
chapter: 0
index: 1
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

internal static class Program
{
    private static void Main()
    {
        var documents = new List<DocumentRecord>
        {
            new DocumentRecord(1, "Avery", 125m),
            new DocumentRecord(2, "Blair", 25m),
            new DocumentRecord(3, "Avery", 75m),
            new DocumentRecord(4, "Casey", 200m),
        };

        foreach (var doc in FilterDocuments(documents, owner: "Avery", minimumAmount: 100m))
            Console.WriteLine($"{doc.Id}: {doc.Owner} - {doc.Amount:C}");
    }

    private static IReadOnlyList<DocumentRecord> FilterDocuments(
        IReadOnlyList<DocumentRecord> documents,
        string owner,
        decimal? minimumAmount)
    {
        if (documents == null)
            throw new ArgumentNullException(nameof(documents));

        IEnumerable<DocumentRecord> query = documents;

        if (!string.IsNullOrWhiteSpace(owner))
            query = query.Where(doc => string.Equals(doc.Owner, owner, StringComparison.OrdinalIgnoreCase));

        if (minimumAmount.HasValue)
            query = query.Where(doc => doc.Amount >= minimumAmount.Value);

        return query.ToList();
    }

    private sealed class DocumentRecord
    {
        public DocumentRecord(int id, string owner, decimal amount)
        {
            Id = id;
            Owner = owner;
            Amount = amount;
        }

        public int Id { get; }
        public string Owner { get; }
        public decimal Amount { get; }
    }
}
```
