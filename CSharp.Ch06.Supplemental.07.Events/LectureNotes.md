# Chapter 6 Supplemental 07: Events

## What This Is

The fullest events lesson in this chapter set, five progressively better ways to implement the same "overdrawn account" event, from a bare custom delegate up through the standard `EventHandler<T>` pattern, event inheritance, and multi-subscriber unsubscription. Genuinely excellent, comprehensive content.

---

## The Bug That Was Here (Compile-Breaking)

`OverdrawnEventArgs` didn't inherit from `System.EventArgs`:

```csharp
public class OverdrawnEventArgs
{
    public decimal CurrentBalance { get; set; }
    public decimal DebitAmount { get; set; }
    ...
}
```

But `ImprovedBankAccount` declared its event using the generic `EventHandler<T>` delegate:

```csharp
public event EventHandler<OverdrawnEventArgs> Overdrawn;
```

`EventHandler<TEventArgs>` carries a generic constraint, `TEventArgs` must derive from `EventArgs`. `OverdrawnEventArgs` didn't, so this line, and everything downstream of it (`MoneyMarketAccount`, which inherits from `ImprovedBankAccount`, and every method in `Program.cs` that references `ImprovedBankAccount`, `MoneyMarketAccount`, or `OnAccountOverdrawn`), would fail to compile with a real compiler error along the lines of "there is no implicit reference conversion from `OverdrawnEventArgs` to `System.EventArgs`."

This is the most significant bug found anywhere in this migration so far, not a runtime gotcha or a portability issue, a genuine compile failure that would have stopped this entire project from building.

**Fixed** by adding the missing inheritance:

```csharp
public class OverdrawnEventArgs : EventArgs
{
    ...
}
```

No other changes were needed, `OverdrawnEventArgs`'s existing constructor already runs correctly against `EventArgs`'s implicit parameterless base constructor.

---

## The Five Versions, Read in Order

1. **`SimpleBankAccount`**: a hand-declared delegate (`OverdrawnEventHandler`), no parameters, no event args. The event just says "something happened," nothing about what.
2. **`ActionBankAccount`**: identical to `SimpleBankAccount`, but uses the built-in `Action` delegate instead of declaring a custom one. Same behavior, one less type to maintain.
3. **`ImprovedBankAccount`**: the real, idiomatic .NET pattern, `EventHandler<OverdrawnEventArgs>`, `sender` plus a custom `EventArgs` subclass carrying actual data about what happened (`CurrentBalance`, `DebitAmount`), and a protected virtual `OnOverdrawn()` method (the standard "raise method" convention).
4. **`MoneyMarketAccount`**: inherits from `ImprovedBankAccount` and calls the *inherited* `OnOverdrawn()` directly, demonstrating that a subclass can raise a parent's event without needing to redeclare anything, exactly why `OnOverdrawn()` is `protected virtual` rather than `private`.
5. **`Program.cs`'s `OversubscribingExample()`**: the same event subscribed twice (`account.Overdrawn += OnAccountOverdrawnMulti;` called twice), demonstrating that both handlers fire on every raise, then a single `-=` to unsubscribe just one, leaving the other still active.

---

## Worth Noticing: Why `OnOverdrawn` Exists at All

```csharp
protected virtual void OnOverdrawn(OverdrawnEventArgs args)
{
    Overdrawn?.Invoke(this, args);
}
```

`Debit()` doesn't call `Overdrawn?.Invoke(...)` directly, it calls `OnOverdrawn(...)`, which then does that invocation. This extra layer of indirection is the standard `OnXxx` convention for a reason: it's `protected virtual`, so a derived class (`MoneyMarketAccount`) can override it to add behavior before/after the event fires, or, as here, just call it directly to raise the *inherited* event without needing access to the private event field itself (events can only be invoked from within the declaring class, `MoneyMarketAccount` couldn't call `Overdrawn?.Invoke(...)` itself even if it wanted to, only `OnOverdrawn()` can).

---

## Worth Noticing: Oversubscription and Unsubscription

```csharp
account.Overdrawn += OnAccountOverdrawnMulti;
account.Overdrawn += OnAccountOverdrawnMulti;
...
account.Overdrawn -= OnAccountOverdrawnMulti;
```

Subscribing the same method twice means it runs twice per raise, watch the `Overdrawn Count:` output jump by two each time, then by one after the single `-=`. This is the same multicast mechanics `Supplemental.04.MulticastDelegates` covers directly, applied here to an actual event rather than a bare delegate variable.
