---
title: "Events - Version 3: The Idiomatic EventHandler<T> Pattern"
chapter: 6
index: 2
dependencies: []
---

```csharp
using System;

// Custom EventArgs subclass -- carries the data the handler needs
internal class OverdrawnEventArgs : EventArgs
{
    public decimal CurrentBalance { get; }
    public decimal DebitAmount    { get; }

    public OverdrawnEventArgs(decimal balance, decimal debit)
    {
        CurrentBalance = balance;
        DebitAmount    = debit;
    }
}

internal class ImprovedBankAccount
{
    // EventHandler<T> is the .NET standard: void (object sender, TEventArgs e)
    public event EventHandler<OverdrawnEventArgs> Overdrawn;

    public decimal Balance { get; set; }

    public ImprovedBankAccount(decimal initial = 0) { Balance = initial; }

    public void Debit(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (Balance >= amount) { Balance -= amount; return; }
        OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
    }

    // protected virtual so derived classes can raise the event without invoking it directly
    // (events can only be invoked inside the class that declares them)
    protected virtual void OnOverdrawn(OverdrawnEventArgs args)
    {
        Overdrawn?.Invoke(this, args);
    }
}

internal static class Program
{
    private static void Main()
    {
        var account = new ImprovedBankAccount(100);

        account.Overdrawn += (sender, e) =>
        {
            Console.WriteLine($"Account overdrawn!");
            Console.WriteLine($"  Balance: {e.CurrentBalance}, Attempted debit: {e.DebitAmount}");
            Console.WriteLine($"  Sender type: {sender.GetType().Name}");
        };

        account.Debit(50);   // succeeds
        account.Debit(75);   // overdraws -- fires with actual data this time

        // EventArgs properties are immutable -- all subscribers get the same instance.
        // One subscriber must not be able to alter what later subscribers see.
    }
}
```
