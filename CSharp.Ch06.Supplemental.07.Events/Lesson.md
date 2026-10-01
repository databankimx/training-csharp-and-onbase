# Chapter 6 Supplemental 07: Events

## What This Is

Five progressively better implementations of the same "overdrawn account" event, from a bare custom delegate up through the standard `EventHandler<T>` pattern, inheritance, and multi-subscriber unsubscription. Read the five bank account classes in order -- each one fixes a specific shortcoming in the previous one, and that sequence is the lesson.

---

## The Bug That Was Here (Compile-Breaking)

`OverdrawnEventArgs` was missing its inheritance from `System.EventArgs`:

```csharp
// As originally written:
public class OverdrawnEventArgs
{
    public decimal CurrentBalance { get; set; }
    public decimal DebitAmount { get; set; }
}
```

`EventHandler<TEventArgs>` has a generic constraint: `TEventArgs` must derive from `EventArgs`. Without it, every declaration using `EventHandler<OverdrawnEventArgs>` and everything downstream fails to compile. This was the most significant bug found in this migration -- not a runtime gotcha, a complete build failure.

**Fixed** by adding the missing base class:

```csharp
public class OverdrawnEventArgs : EventArgs { ... }
```

No other changes were needed. Worth remembering for diagnosis: a generic constraint violation reports an error at the *declaration site*, not in the type argument's own file. When a constraint error looks nonsensical, check the type argument's declaration.

---

## How to Write This Program

Build all five account classes alongside `Main()`. Each one is a standalone class -- don't modify the previous ones as you go.

### Version 1: SimpleBankAccount -- A Bare Custom Delegate

```csharp
public class SimpleBankAccount
{
    public delegate void OverdrawnEventHandler();
    public event OverdrawnEventHandler Overdrawn;

    public decimal Balance { get; private set; }

    public SimpleBankAccount(decimal initialBalance = 0) { Balance = initialBalance; }

    public void Debit(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        if (Balance >= amount) { Balance -= amount; return; }
        Overdrawn?.Invoke();
    }
}
```

```csharp
var account = new SimpleBankAccount(100);
account.Overdrawn += () => Console.WriteLine("Account overdrawn!");
account.Debit(50);   // succeeds, no event
account.Debit(75);   // overdraws, fires event
```

Run it. One message on the overdraft.

The limitation is right there in the output. The handler can say "Account overdrawn!" and nothing more, because the event carries no information -- not the balance, not the amount, not which account raised it. If two accounts shared this handler, there'd be no way to tell them apart.

Note also that `Debit()` raises the event and then *returns without debiting*. The event is a notification, not a veto. That's a deliberate design decision worth being conscious of.

### Version 2: ActionBankAccount -- Use the Built-In Delegate

Identical to Version 1, except the custom `OverdrawnEventHandler` declaration is gone and the event uses `Action` instead:

```csharp
public event Action Overdrawn;
```

Same behavior, one less type to maintain, and immediately recognizable without reading a separate declaration. This doesn't solve the information problem, though. That takes something different.

### Version 3: ImprovedBankAccount -- The Idiomatic .NET Pattern

```csharp
public class OverdrawnEventArgs : EventArgs
{
    public decimal CurrentBalance { get; }
    public decimal DebitAmount { get; }

    public OverdrawnEventArgs(decimal currentBalance, decimal debitAmount)
    {
        CurrentBalance = currentBalance;
        DebitAmount = debitAmount;
    }
}

public class ImprovedBankAccount
{
    public event EventHandler<OverdrawnEventArgs> Overdrawn;

    public decimal Balance { get; set; }

    public ImprovedBankAccount(decimal initialBalance = 0) { Balance = initialBalance; }

    public void Debit(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        if (Balance >= amount) { Balance -= amount; return; }
        OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
    }

    protected virtual void OnOverdrawn(OverdrawnEventArgs args)
    {
        Overdrawn?.Invoke(this, args);
    }
}
```

```csharp
var account = new ImprovedBankAccount(100);
account.Overdrawn += (sender, e) =>
{
    Console.WriteLine($"Account overdrawn!");
    Console.WriteLine($"Balance: {e.CurrentBalance}, Debit attempted: {e.DebitAmount}");
};
account.Debit(150);
```

Run it. The handler now has actual data to work with.

Three components worth naming individually:

**`EventHandler<TEventArgs>`** is the framework's standard event delegate. Its signature is fixed: `void (object sender, TEventArgs e)`. Every event in your codebase sharing this shape means tooling, designers, and other developers can work with your events without reading their declarations.

**`sender`** is the object that raised the event, typed as `object` because the delegate is generic over the args, not the sender. A handler serving multiple accounts casts it to find out which one fired.

**A custom `EventArgs` subclass** carries the data. Make properties immutable -- get-only, set in the constructor. All subscribers receive the same instance, so one subscriber shouldn't be able to alter what later subscribers see.

**`protected virtual void OnOverdrawn`** exists for a concrete reason: an event can only be *invoked* from inside the class that declares it. Derived classes literally cannot write `Overdrawn?.Invoke(...)` -- the compiler rejects it. `OnOverdrawn()` is how the base class delegates that capability. `protected virtual` means derived classes can either call it to raise the event, or override it to inject behavior before or after. For a `sealed` class, `private` is the correct modifier instead.

### Version 4: MoneyMarketAccount -- Raising an Inherited Event

```csharp
public class MoneyMarketAccount : ImprovedBankAccount
{
    public MoneyMarketAccount(decimal initialBalance = 0) : base(initialBalance) { }

    public void DebitFree(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        if (Balance >= amount) { Balance -= amount; return; }
        OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
    }
}
```

```csharp
var account = new MoneyMarketAccount(100);
account.Overdrawn += (sender, e) => Console.WriteLine($"MoneyMarket overdrawn! Balance: {e.CurrentBalance}");
account.DebitFree(200);
```

Run it. The handler fires exactly the same way as before, even though the event was declared two levels up.

The derived class raises the parent's event by calling the inherited `OnOverdrawn()`. It declares no event of its own. Subscribers attach to `account.Overdrawn` exactly as before -- they can't tell the difference.

### Version 5: OversubscribingExample -- Multicast in Practice

```csharp
var account = new ImprovedBankAccount(100);

// Subscribe the same handler twice
account.Overdrawn += OnAccountOverdrawnMulti;
account.Overdrawn += OnAccountOverdrawnMulti;

account.Debit(200); // fires the handler twice

// Remove one subscription
account.Overdrawn -= OnAccountOverdrawnMulti;
account.Debit(200); // fires the handler once
```

Run it. The first overdraft triggers two handler calls; the second triggers one.

**`+=` doesn't check for duplicates.** It appends unconditionally. Subscribing the same handler twice is a common bug -- typically a component that subscribes in an initialization method called more than once. Symptom: an operation happens twice.

**`-=` removes one occurrence, not all.** After the single `-=`, one subscription remains.

**`-=` requires the same delegate instance.** A handler subscribed as a lambda cannot be removed unless you stored a reference to it. That's the leading cause of event-handler memory leaks -- the object can't be collected because the event still holds a reference to it.

---

## Takeaways

- `EventArgs` inheritance is a hard constraint on `EventHandler<T>` -- omitting it is a compile failure at the declaration site.
- Start from the standard pattern: `EventHandler<TEventArgs>`, a custom `EventArgs` subclass, and a `protected virtual OnXxx()` raise method.
- Prefer `Action`/`EventHandler<T>` over hand-declared delegate types.
- An event carrying no data can only announce something happened, not what.
- Make `EventArgs` properties immutable -- all subscribers receive the same instance.
- Events can only be invoked inside the declaring class; `protected virtual OnXxx()` is how derived classes raise them.
- `+=` doesn't deduplicate; `-=` removes one occurrence. Match every subscription with an unsubscription.
- `-=` requires the same delegate instance -- lambdas can't be unsubscribed unless stored.
- Don't throw `ApplicationException`; use specific types like `ArgumentOutOfRangeException`.
