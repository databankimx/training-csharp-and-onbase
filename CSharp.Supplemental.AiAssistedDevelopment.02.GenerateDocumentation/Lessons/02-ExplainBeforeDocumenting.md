---
title: "Explain Before Documenting"
chapter: 0
index: 2
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        var invoice = new Invoice(125m, 0.0825m, true);
        Console.WriteLine(Explain(invoice));
        Console.WriteLine($"Total: {CalculateTotal(invoice):C}");
    }

    private static decimal CalculateTotal(Invoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));

        decimal subtotal = invoice.Subtotal;
        decimal tax = subtotal * invoice.TaxRate;
        decimal discount = invoice.IsPreferredCustomer ? subtotal * 0.10m : 0m;

        return subtotal + tax - discount;
    }

    private static string Explain(Invoice invoice)
    {
        return "Total equals subtotal plus tax, minus a 10% subtotal discount " +
               "when the invoice belongs to a preferred customer.";
    }

    private sealed class Invoice
    {
        public Invoice(decimal subtotal, decimal taxRate, bool isPreferredCustomer)
        {
            Subtotal = subtotal;
            TaxRate = taxRate;
            IsPreferredCustomer = isPreferredCustomer;
        }

        public decimal Subtotal { get; }
        public decimal TaxRate { get; }
        public bool IsPreferredCustomer { get; }
    }
}
```
