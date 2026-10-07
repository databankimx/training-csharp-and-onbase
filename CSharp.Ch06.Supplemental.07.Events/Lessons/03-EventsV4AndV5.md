---
title: "Events - Version 4 and 5: Inheritance and Oversubscription"
chapter: 6
index: 3
dependencies: []
---

```csharp
using System;

internal class OverdrawnEventArgs : EventArgs
{
    public decimal CurrentBalance { get; }
    public decimal DebitAmount    { get; }
    public OverdrawnEventArgs(decimal b, decimal d) { CurrentBalance = b; DebitAmount = d; }
}

internal class ImprovedBankAccount
{
    public event EventHandler<OverdrawnEventArgs> Overdrawn;
    public decimal Balance { get; set; }
    public ImprovedBankAccount(decimal initial = 0) { Balance = initial; }

    public void Debit(decimal amount)
    {
        if (Balance >= amount) { Balance -= amount; return; }
        OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
    }

    protected virtual void OnOverdrawn(OverdrawnEventArgs args)
        => Overdrawn?.Invoke(this, args);
}

// Version 4: derived class raises an inherited event via the protected OnOverdrawn method
internal class MoneyMarketAccount : ImprovedBankAccount
{
    public MoneyMarketAccount(decimal initial = 0) : base(initial) { }

    public void DebitFee(decimal amount)
    {
        if (Balance >= amount) { Balance -= amount; return; }
        OnOverdrawn(new OverdrawnEventArgs(Balance, amount)); // calls inherited raise method
    }
}

internal static class Program
{
    private static void OnOverdrawnMulti(object sender, OverdrawnEventArgs e)
        => Console.WriteLine($"  Handler called -- balance: {e.CurrentBalance}");

    private static void Main()
    {
        Console.WriteLine("--- Version 4: derived class raises inherited event ---");
        var mma = new MoneyMarketAccount(100);
        mma.Overdrawn += (s, e) => Console.WriteLine($"MoneyMarket overdrawn! Balance: {e.CurrentBalance}");
        mma.DebitFee(200); // fires exactly as if ImprovedBankAccount raised it

        Console.WriteLine("\n--- Version 5: oversubscription ---");
        var account = new ImprovedBankAccount(100);

        // += doesn't check for duplicates -- subscribing the same handler twice is a common bug
        account.Overdrawn += OnOverdrawnMulti;
        account.Overdrawn += OnOverdrawnMulti;

        Console.WriteLine("First overdraft (two subscriptions -- handler fires twice):");
        account.Debit(200);

        // -= removes one occurrence, not all
        account.Overdrawn -= OnOverdrawnMulti;
        account.Balance = 100;

        Console.WriteLine("\nSecond overdraft (one subscription removed -- handler fires once):");
        account.Debit(200);

        // -= requires the same delegate INSTANCE.
        // A lambda subscribed without storing a reference can never be unsubscribed -- memory leak.
    }
}
```
