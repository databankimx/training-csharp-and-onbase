---
title: "Events - Version 1 and 2: Bare Delegate and Action"
chapter: 6
index: 1
dependencies: []
---

```csharp
using System;

// Version 1: custom delegate type, void, no data
internal class SimpleBankAccount
{
    public delegate void OverdrawnEventHandler();
    public event OverdrawnEventHandler Overdrawn;

    public decimal Balance { get; private set; }

    public SimpleBankAccount(decimal initial = 0) { Balance = initial; }

    public void Debit(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (Balance >= amount) { Balance -= amount; return; }
        Overdrawn?.Invoke();    // raises the event; returns without debiting
    }
}

// Version 2: exact same behavior, but uses the built-in Action instead of a custom delegate type
internal class ActionBankAccount
{
    public event Action Overdrawn;

    public decimal Balance { get; private set; }

    public ActionBankAccount(decimal initial = 0) { Balance = initial; }

    public void Debit(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (Balance >= amount) { Balance -= amount; return; }
        Overdrawn?.Invoke();
    }
}

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("--- SimpleBankAccount (custom delegate) ---");
        var v1 = new SimpleBankAccount(100);
        v1.Overdrawn += () => Console.WriteLine("V1: Account overdrawn!");
        v1.Debit(50);   // succeeds, no event
        v1.Debit(75);   // overdraws, fires event

        Console.WriteLine("\n--- ActionBankAccount (Action) ---");
        var v2 = new ActionBankAccount(100);
        v2.Overdrawn += () => Console.WriteLine("V2: Account overdrawn!");
        v2.Debit(50);
        v2.Debit(75);

        // Limitation of both: the handler receives no information.
        // It can only say "something happened" -- not which account, what balance, or what amount.
        // That's what Version 3 (EventHandler<T>) fixes.
    }
}
```
