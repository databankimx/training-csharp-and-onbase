# Events

## Introduction

An event lets an object signal that something happened, without needing to know who (if anyone) is listening. This lesson builds the same idea five times, each version a little closer to the idiomatic .NET pattern.

- **Publisher**: the object that raises the event (the bank account).
- **Subscriber**: the object that listens for it (`Program.cs`'s handler methods).

---

## Version 1: A Bare Custom Delegate

```csharp
public delegate void OverdrawnEventHandler();
public event OverdrawnEventHandler Overdrawn;
```

```csharp
Overdrawn?.Invoke();
```

The simplest possible event: no information travels with it, subscribers just find out *that* an overdraft happened, not by how much or what the balance was.

---

## Version 2: The Same Thing, With `Action`

```csharp
public event Action Overdrawn;
```

`Action` is a built-in delegate matching "no parameters, no return value", exactly `OverdrawnEventHandler`'s shape. No real difference in behavior, just one less type to declare and maintain.

---

## Version 3: The Idiomatic Pattern, `EventHandler<T>`

```csharp
public class OverdrawnEventArgs : EventArgs
{
    public decimal CurrentBalance { get; set; }
    public decimal DebitAmount { get; set; }

    public OverdrawnEventArgs(decimal currentBalance, decimal debitAmount)
    {
        CurrentBalance = currentBalance;
        DebitAmount = debitAmount;
    }
}
```

```csharp
public event EventHandler<OverdrawnEventArgs> Overdrawn;

protected virtual void OnOverdrawn(OverdrawnEventArgs args)
{
    Overdrawn?.Invoke(this, args);
}
```

This is the standard shape almost every .NET event follows: a custom class deriving from `EventArgs`, carrying whatever data subscribers might need, and an `EventHandler<T>` event that always passes `(object sender, TEventArgs e)`. Note the important detail: **`OverdrawnEventArgs` must inherit from `EventArgs`**, `EventHandler<T>` requires it. Leaving that out doesn't compile.

The `OnOverdrawn()` method (the "raise method") is `protected virtual` by convention, not called `Overdrawn.Invoke(...)` directly from `Debit()`. That indirection matters, see the next section.

---

## Version 4: Event Inheritance

```csharp
public class MoneyMarketAccount : ImprovedBankAccount
{
    public void DebitFree(decimal amount)
    {
        if (Balance >= amount) { Balance -= amount; return; }
        OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
    }
}
```

`MoneyMarketAccount` calls `OnOverdrawn(...)`, inherited straight from `ImprovedBankAccount`, to raise the *same* `Overdrawn` event its parent declared. It can't call `Overdrawn?.Invoke(...)` directly, events can only be invoked from inside the class that declares them, even by a subclass. That's exactly why the `protected virtual OnOverdrawn()` method exists: it's the sanctioned way for a subclass to trigger an inherited event.

---

## Subscribing, Oversubscribing, and Unsubscribing

```csharp
account.Overdrawn += OnAccountOverdrawnMulti;
account.Overdrawn += OnAccountOverdrawnMulti;
```

Subscribing the same method twice means it fires twice per raise, an event is a multicast delegate under the hood, same mechanics as `+`/`-` on any other delegate.

```csharp
account.Overdrawn -= OnAccountOverdrawnMulti;
```

`-=` removes one subscription (the most recent matching one), leaving the other still active.

---

## Try It Yourself

Run the project and watch the `Overdrawn Count:` output in the oversubscription example, it climbs by two per overdraft while both subscriptions are active, then by one after the single `-=`. Then try adding a third handler method and subscribing all three, predict how many times each prints before running it.
