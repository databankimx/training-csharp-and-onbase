# Ch06 Textbook Code: Bank Account

## What This Is

The direct source `CSharp.Ch06.Supplemental.07.Events`'s `ImprovedBankAccount` was adapted from: interactive Credit/Debit buttons, a real balance display, and a genuine `EventHandler<T>` event with custom `EventArgs`, click "Debit" for more than the current balance and a message box shows the overdraft details.

No bugs found. Notably, **this original download's `OverdrawnEventArgs` correctly inherits from `EventArgs`**:

```csharp
class OverdrawnEventArgs : EventArgs
{
    public decimal CurrentBalance, DebitAmount;
    ...
}
```

That confirms the missing `: EventArgs` inheritance found and fixed in `Supplemental.07.Events` was introduced when that project's own version was written, it wasn't inherited from this textbook source, which had it right from the start.

---

## Worth Noticing: The Older Null-Check Style

```csharp
if (Overdrawn != null)
    Overdrawn(this, new OverdrawnEventArgs(Balance, amount));
```

`Supplemental.07.Events`'s `ImprovedBankAccount` raises its event with `Overdrawn?.Invoke(this, args)`, the modern null-conditional operator. This textbook version predates that, using the classic explicit `if (Overdrawn != null)` check instead, both do exactly the same thing, just with syntax from different eras of C#. Worth reading side by side as a small, concrete example of how the language's own idioms evolved over time, without the underlying logic changing at all.

---

## Try It Yourself

The account starts at $100. Click "Debit" (with the pre-filled amount of 50) twice in a row, the first succeeds silently, the second overdraws (balance would be $0, amount is $50, so $0 < $50), triggering the message box with the exact `CurrentBalance`/`DebitAmount` values that caused it.
