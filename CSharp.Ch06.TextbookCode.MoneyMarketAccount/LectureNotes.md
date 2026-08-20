# Ch06 Textbook Code: Money Market Account

## What This Is

The direct source `CSharp.Ch06.Supplemental.07.Events`'s `MoneyMarketAccount` was adapted from: event inheritance demonstrated via a third button ("Fee") alongside the usual Credit/Debit, `MoneyMarketAccount : BankAccount` defines its own `DebitFee()` method and correctly raises the inherited `Overdrawn` event by calling the inherited `OnOverdrawn()`.

---

## The Bug That Was Here

```csharp
// Original:
private void feeButton_Click(object sender, EventArgs e)
{
    TheAccount.Debit(decimal.Parse(amountTextBox.Text, NumberStyles.Currency));
    DisplayBalance();
}
```

Byte-for-byte identical to `debitButton_Click`. The "Fee" button was supposed to call `TheAccount.DebitFee(...)`, exercising `MoneyMarketAccount`'s own method (the entire reason this project exists), but instead it called the inherited `Debit()` a second time under a different button label. This meant `DebitFee()` was dead code, defined, never actually invoked by anything.

**Fixed**:

```csharp
private void feeButton_Click(object sender, EventArgs e)
{
    TheAccount.DebitFee(decimal.Parse(amountTextBox.Text, NumberStyles.Currency));
    DisplayBalance();
}
```

Worth noting: `DebitFee()` and `Debit()` happen to contain identical logic (same balance check, same `OnOverdrawn()` call), so this bug never produced visibly wrong *behavior*, clicking "Fee" always worked correctly, it just worked by accident, running the wrong method that happened to do the same thing. The fix doesn't change what the button does, it changes *which method* does it, so the demo now actually exercises the code it was built to showcase.

---

## Worth Noticing: A Class Named the Same as Its Namespace

```csharp
namespace CSharp.Ch06.TextbookCode.MoneyMarketAccount
{
    class MoneyMarketAccount : BankAccount
    {
        ...
    }
}
```

The class `MoneyMarketAccount` sits inside a namespace whose last segment is also `MoneyMarketAccount`. This is legal C#, and the original download already had exactly this structure (`namespace MoneyMarketAccount { class MoneyMarketAccount ... }`), preserved here rather than "fixed", since it isn't actually a problem, just a naming coincidence worth being aware of if you ever go looking for the class and get momentarily confused about which `MoneyMarketAccount` a reference means.

---

## Try It Yourself

Start with the default $100 balance. Click "Fee" with the default amount ($50) twice in a row, the second click overdraws and the message box fires, exactly as it would from "Debit". Now that both buttons genuinely call their own distinct methods, you can confirm this yourself by setting a breakpoint in `DebitFee()` versus `Debit()` and watching which one actually hits when you click "Fee".
