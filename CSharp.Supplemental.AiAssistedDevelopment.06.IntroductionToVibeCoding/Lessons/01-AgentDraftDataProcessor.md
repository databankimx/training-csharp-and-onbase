---
title: "Review an Agent Draft"
chapter: 0
index: 1
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;

internal static class Program
{
    private static void Main()
    {
        var rows = new List<string>
        {
            "1,Avery,12.50",
            "2,Blair,not-a-number",
            "3,Casey,35.75",
            "bad row",
            "4,Drew,20.00"
        };

        var summary = Summarize(rows);

        Console.WriteLine($"Valid rows: {summary.ValidRowCount}");
        Console.WriteLine($"Total amount: {summary.TotalAmount:C}");
        Console.WriteLine($"Highest amount name: {summary.HighestAmountName}");
    }

    private static Summary Summarize(IEnumerable<string> rows)
    {
        if (rows == null)
            throw new ArgumentNullException(nameof(rows));

        int count = 0;
        decimal total = 0m;
        decimal highestAmount = decimal.MinValue;
        string highestName = string.Empty;

        foreach (string row in rows)
        {
            if (!TryParse(row, out var record))
                continue;

            count++;
            total += record.Amount;

            if (record.Amount > highestAmount)
            {
                highestAmount = record.Amount;
                highestName = record.Name;
            }
        }

        return new Summary(count, total, highestName);
    }

    private static bool TryParse(string row, out Record record)
    {
        record = null;

        if (string.IsNullOrWhiteSpace(row))
            return false;

        string[] parts = row.Split(',');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out int id))
            return false;

        string name = parts[1].Trim();
        if (name.Length == 0)
            return false;

        if (!decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
            return false;

        record = new Record(id, name, amount);
        return true;
    }

    private sealed class Record
    {
        public Record(int id, string name, decimal amount)
        {
            Id = id;
            Name = name;
            Amount = amount;
        }

        public int Id { get; }
        public string Name { get; }
        public decimal Amount { get; }
    }

    private sealed class Summary
    {
        public Summary(int validRowCount, decimal totalAmount, string highestAmountName)
        {
            ValidRowCount = validRowCount;
            TotalAmount = totalAmount;
            HighestAmountName = highestAmountName;
        }

        public int ValidRowCount { get; }
        public decimal TotalAmount { get; }
        public string HighestAmountName { get; }
    }
}
```
