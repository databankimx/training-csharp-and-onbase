# Ch06 Textbook Code: Overdraft Account (Real-World Scenario 1)

## What This Is

Two linked, fully interactive accounts: an overdraft-protected checking account (`OverdraftAccount`) backed by a plain savings account (`BankAccount`). Credit/Debit buttons for both, and both accounts' `Overdrawn` events are wired to separate message-box handlers.

No bugs found. Two design details are worth reading closely, both easy to misread as bugs at a glance.

---

## Worth Reading Closely: `new`, Not `override`

```csharp
class OverdraftAccount : BankAccount
{
    public BankAccount SavingsAccount { get; set; }

    public new void Debit(decimal amount)
    {
        ...
    }
}
```

`BankAccount.Debit()` is never declared `virtual`, so `OverdraftAccount` genuinely can't `override` it, `new` is the only option the compiler allows. `new` here means *method hiding*, not polymorphic override: which `Debit()` runs depends on the *compile-time* type of the reference it's called through, not the actual runtime type of the object.

That distinction matters and can bite you elsewhere: `Form1.cs` declares `private OverdraftAccount TheAccount;`, so every call to `TheAccount.Debit(...)` correctly resolves to `OverdraftAccount.Debit()`. But if this variable had instead been declared `private BankAccount TheAccount = new OverdraftAccount();` (referring to the object through its base type), calling `.Debit()` would silently call `BankAccount.Debit()` instead, the base account's simple insufficient-funds check, completely bypassing the overdraft/savings logic, with no compiler warning that anything unusual happened. This project avoids that trap by declaring `TheAccount` as the derived type throughout, but it's exactly the kind of mistake method hiding makes easy to introduce elsewhere.

---

## Worth Reading Closely: Why Savings Is Debited Directly, Not Via `.Debit()`

```csharp
public new void Debit(decimal amount)
{
    if (Balance + SavingsAccount.Balance < amount)
    {
        OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
    }
    else
    {
        if (Balance >= amount) Balance -= amount;
        else
        {
            amount -= Balance;
            Balance = 0m;

            // If there's still an unpaid amount, take it from savings.
            if (amount > 0m) SavingsAccount.Balance -= amount;
        }
    }
}
```

Notice the last line manipulates `SavingsAccount.Balance` directly, rather than calling `SavingsAccount.Debit(amount)`. This looks like it might be a shortcut or an oversight, it's actually a deliberate, careful choice.

By the time this line runs, the method has already confirmed that combined funds are sufficient (`Balance + SavingsAccount.Balance >= originalAmount`), then zeroed out the overdraft balance and reduced `amount` to just the leftover shortfall. Working through the arithmetic, that guarantees `SavingsAccount.Balance >= amount` at this point too, so calling `SavingsAccount.Debit(amount)` here would, in this specific case, actually succeed rather than trigger `SavingsAccount`'s own overdraft path.

But that's exactly why direct manipulation is the right call anyway: `OverdraftAccount.Debit()` already verified the invariant it needs (enough money exists somewhere to cover this withdrawal) before touching `SavingsAccount` at all. Calling `SavingsAccount.Debit(amount)` instead would mean this method's correctness now also depends on `SavingsAccount`'s own independent business logic running exactly as expected, coupling two objects together for no benefit, when `OverdraftAccount` already has everything it needs to safely adjust the balance itself. Worth sitting with, since "just call the existing method" is usually good advice, and this is a case where doing so anyway would add a needless dependency rather than remove one.

---

## Try It Yourself

Start with $50 in both accounts (the default). Debit $30 from Overdraft twice in a row: the first succeeds from the overdraft balance alone. The second needs $30 but only $20 remains in overdraft, watch the savings balance drop by $10 to cover the shortfall, no `Overdrawn` message box, because combined funds were sufficient. Now try debiting more than $100 total, and watch the real overdraft message box appear.
