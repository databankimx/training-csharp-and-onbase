# Chapter 6 Supplemental 07: Events

## What This Is

The fullest events lesson in this chapter set: five progressively better ways to implement the same "overdrawn account" event, from a bare custom delegate up through the standard `EventHandler<T>` pattern, event inheritance, and multi-subscriber unsubscription.

Read the five bank account classes in order. Each one fixes a specific shortcoming in the previous one, and the sequence is the actual lesson — not any single class.

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

`EventHandler<TEventArgs>` carries a generic constraint — `TEventArgs` must derive from `EventArgs`. `OverdrawnEventArgs` didn't, so this line, and everything downstream of it, would fail to compile with an error along the lines of "there is no implicit reference conversion from `OverdrawnEventArgs` to `System.EventArgs`."

Downstream meant a lot: `MoneyMarketAccount` (which inherits from `ImprovedBankAccount`), and every method in `Program.cs` referencing `ImprovedBankAccount`, `MoneyMarketAccount`, or `OnAccountOverdrawn`.

This is the most significant bug found anywhere in this migration so far — not a runtime gotcha or a portability issue, but a genuine compile failure that would have stopped the entire project from building.

**Fixed** by adding the missing inheritance:

```csharp
public class OverdrawnEventArgs : EventArgs
{
	...
}
```

No other changes were needed. `OverdrawnEventArgs`'s existing constructor already runs correctly against `EventArgs`'s implicit parameterless base constructor.

Worth noting for the diagnostic habit: a generic constraint violation produces an error at the *declaration site*, not inside the generic type. The error appears on the `ImprovedBankAccount` line while the actual problem is in a different file. When a constraint error looks nonsensical, check the type argument's declaration, not the line the compiler is pointing at.

---

## Version 1: `SimpleBankAccount` — A Hand-Declared Delegate

```csharp
public delegate void OverdrawnEventHandler();

public event OverdrawnEventHandler Overdrawn;
```

```csharp
public void Debit(decimal amount)
{
	if (amount < 0) throw new ApplicationException("Amount must be greater than zero!");

	if (Balance >= amount)
	{
		Balance -= amount;
		return;
	}

	Overdrawn?.Invoke();

	// Note: The syntax above takes advantage of null propagation and is equivalent to:
	// if (Overdrawn != null) Overdrawn();
}
```

The minimum viable event. It works, and the subscriber is trivial:

```csharp
private static void Account_Overdrawn()
{
	Console.WriteLine("Account overdrawn!");
}
```

The limitation is right there in the output. The handler can say "Account overdrawn!" and nothing more, because the event carries no information — not the balance, not the attempted amount, not even which account raised it. If two accounts shared this handler, there'd be no way to tell them apart.

Note also that `Debit()` raises the event and then *returns without debiting*. The event is a notification, not a veto — nobody gets to prevent the overdraft, they only get told. That's a design decision worth being conscious of; the `Cancel`-style `EventArgs` pattern exists for the other case.

---

## Version 2: `ActionBankAccount` — Use the Built-In Delegate

Identical to `SimpleBankAccount`, except the custom `OverdrawnEventHandler` declaration is gone and the event is declared with the built-in `Action` instead.

Same behavior, one less type to maintain, and immediately recognizable to any C# developer without having to go read a delegate declaration. This is the same "prefer `Action`/`Func` over custom delegate types" guidance that appears in Supplementals 01, 03, and 04.

It doesn't solve the real problem, though — the event still carries no data. That takes a different fix.

---

## Version 3: `ImprovedBankAccount` — The Idiomatic .NET Pattern

```csharp
public event EventHandler<OverdrawnEventArgs> Overdrawn;
```

```csharp
public void Debit(decimal amount)
{
	if (amount < 0) throw new ApplicationException("Amount must be greater than zero!");

	if (Balance >= amount)
	{
		Balance -= amount;
		return;
	}

	OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
}
```

```csharp
protected virtual void OnOverdrawn(OverdrawnEventArgs args)
{
	Overdrawn?.Invoke(this, args);
}
```

And the subscriber now has something to work with:

```csharp
private static void OnAccountOverdrawn(object sender, OverdrawnEventArgs e)
{
	Console.WriteLine("Account overdrawn!");
	Console.WriteLine($"Balance [{e.CurrentBalance}] less than debit amount [{e.DebitAmount}]!");
}
```

This is the standard pattern, and it has three parts worth naming individually.

**`EventHandler<TEventArgs>`** is the framework's generic event delegate. Its signature is fixed: `void (object sender, TEventArgs e)`. Using it means every event in your codebase has the same shape, which is what allows tooling, designers, and other developers to work with your events without reading their declarations.

**`sender`** is the object that raised the event — `this`, passed in `OnOverdrawn`. It's typed `object` rather than `ImprovedBankAccount` because the delegate is generic over the args, not the sender. A handler shared across several accounts casts it to find out which one fired:

```csharp
if (sender is ImprovedBankAccount account) { /* ... */ }
```

**A custom `EventArgs` subclass** carries the data. `OverdrawnEventArgs` exposes `CurrentBalance` and `DebitAmount` — the information a subscriber actually needs to make a decision. When an event has nothing to report, `EventArgs.Empty` is the conventional stand-in.

One design note: `OverdrawnEventArgs`'s properties have public setters. Event args are conventionally immutable — one subscriber shouldn't be able to alter what later subscribers see, and multicast invocation means they'd all receive the same instance. Get-only properties set through the constructor would be the stricter choice.

Also compare the `Balance` property across versions: `SimpleBankAccount` declares `private set`, while `ImprovedBankAccount` uses a public setter (which `MoneyMarketAccount` needs in order to modify it from the derived class). `protected set` would have been the tighter option there.

---

## Version 4: `MoneyMarketAccount` — Raising an Inherited Event

```csharp
public class MoneyMarketAccount : ImprovedBankAccount
{
	public MoneyMarketAccount(decimal initialBalance = 0) : base(initialBalance) { }

	public void DebitFree(decimal amount)
	{
		if (amount < 0) throw new ApplicationException("Amount must be greater than zero!");

		if (Balance >= amount)
		{
			Balance -= amount;
			return;
		}

		OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
	}
}
```

The derived class raises the parent's event by calling the inherited `OnOverdrawn()`. It declares no event of its own, and subscribers attach to `account.Overdrawn` exactly as before — they can't tell the difference.

### Worth Noticing: Why `OnOverdrawn` Exists at All

This is the payoff for the extra layer of indirection. `Debit()` doesn't call `Overdrawn?.Invoke(...)` directly; it calls `OnOverdrawn(...)`, which does the invocation.

That's the standard `OnXxx` "raise method" convention, and it exists for a concrete reason: **an event can only be invoked from within the class that declares it.** `MoneyMarketAccount` literally cannot write `Overdrawn?.Invoke(this, args)` — the compiler rejects it, even though the class inherits the event. Only `ImprovedBankAccount` can, and `OnOverdrawn()` is how it delegates that capability to its subclasses.

Being `protected virtual` gives derived classes two options:

- **Call it** to raise the inherited event, as `MoneyMarketAccount` does.
- **Override it** to inject behavior before or after the event fires — logging, suppression, additional state changes — while still calling `base.OnOverdrawn(args)` to let subscribers run.

The conventions are worth following exactly: named `On` + event name, `protected virtual`, takes the `EventArgs` instance, `void` return. Anyone reading your class recognizes it instantly. (For a `sealed` class, `private` is the correct modifier instead, since there's no subclass to serve.)

---

## Version 5: `OversubscribingExample()` — Multicast in Practice

```csharp
var account = new ImprovedBankAccount(InitialDeposit);

account.Overdrawn += OnAccountOverdrawnMulti;
account.Overdrawn += OnAccountOverdrawnMulti;

// ... debits that overdraw ...

account.Overdrawn -= OnAccountOverdrawnMulti;

// ... more debits ...
```

The same method is subscribed twice, so it runs **twice** per raise. Watch the `Overdrawn Count:` output jump by two each time, then by one after the single `-=`.

Two conclusions follow:

**`+=` doesn't check for duplicates.** It appends to the invocation list unconditionally. Subscribing the same handler twice is a real and common bug — typically a component that subscribes in an initialization method called more than once. The symptom is an operation happening twice: two emails, two log entries, a doubled counter.

**`-=` removes one occurrence, not all.** After the single `-=`, one subscription remains. Unsubscribing is not idempotent-in-reverse; each `+=` needs a matching `-=`.

This is exactly the multicast mechanics from `Supplemental.04.MulticastDelegates`, applied to a real event rather than a bare delegate variable — confirmation that `+=`/`-=` on an event are the same `+`/`-` operations underneath.

And the related trap from that lesson applies here too: `-=` only works if you pass the *same* delegate you added. A handler subscribed as a lambda cannot be removed unless you stored a reference to it, which is the leading cause of event-handler memory leaks.

---

## A Note on `ApplicationException`

All the account classes throw `ApplicationException` for invalid amounts, with `#pragma warning disable S112` acknowledging that the analyzer objects — and the comment in `MoneyMarketAccount` says outright that it's "generally not recommended in production code."

The analyzer is right. `ApplicationException` was intended to separate application errors from framework errors, that distinction never held up in practice, and Microsoft's own guidance is not to use it. `ArgumentOutOfRangeException` is the correct choice for a negative amount:

```csharp
if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero!");
```

The suppressions are deliberate and commented, so this is legacy style preserved for the lesson rather than an oversight. Worth knowing which one to write in your own code.

---

## Takeaways

- `EventArgs` inheritance is a hard constraint on `EventHandler<T>` — omitting it is a compile error at the event declaration, not in the args class.
- Start from the standard pattern: `EventHandler<TEventArgs>`, a custom `EventArgs` subclass, and a `protected virtual OnXxx` raise method.
- Prefer `Action`/`EventHandler<T>` over hand-declared delegate types.
- An event carrying no data can only announce that something happened, not what.
- `sender` is `object` by design; cast or pattern-match when a handler serves multiple sources.
- Make `EventArgs` properties immutable — all subscribers receive the same instance.
- Events can only be invoked inside the declaring class; `protected virtual OnXxx()` is how derived classes raise them.
- Derived classes can raise a base class event without redeclaring anything.
- `+=` doesn't deduplicate; `-=` removes one occurrence. Match every subscription with an unsubscription.
- `-=` requires the same delegate instance — lambdas can't be unsubscribed unless stored.
- Don't throw `ApplicationException`. Use a specific framework type such as `ArgumentOutOfRangeException`.
