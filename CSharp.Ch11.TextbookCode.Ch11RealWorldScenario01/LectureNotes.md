# Ch11 Textbook Code: Order Entry Form (Real World Scenario 01)

## What This Is

A genuinely runnable, interactive WinForms order-entry form, and the single richest, most complete example of this chapter's "two-tier" validation pattern (syntax checks that block outright, sanity checks that ask "continue anyway?"), matching the exact distinction covered in the main lesson's `UsingSanityChecks()`.

---

## A Real Bug: `SanityCheckRow()`'s Copy-Paste Mistake

```csharp
// Check the unit price.
decimal price = decimal.Parse(rowTextBoxes[row, 1].Text, NumberStyles.Currency);
if ((price < minUnitCost) || (price > maxUnitCost))
{
    message += "Unit price " + (row + 1) + " is unusual.\n";
    ...
}

// Check the quantity.
int quantity = int.Parse(rowTextBoxes[row, 2].Text);
if ((price < minUnitCost) || (price > maxUnitCost))   // bug: should check quantity!
{
    message += "Quantity " + (row + 1) + " is unusual.\n";
    ...
}
```

Worth reading closely: the quantity check block computes `quantity`, then never actually uses it, the `if` condition re-tests `price` against `minUnitCost`/`maxUnitCost`, the exact same condition as the block directly above it. `minQuantity`/`maxQuantity` are declared as constants at the top of the file and were never referenced anywhere before this fix, a clear signal the quantity check block was copy-pasted from the price check block and the variable names inside the condition were never updated to match.

Two real, practical consequences of this bug:
1. **Quantity was never sanity-checked at all.** An order for 100,000 units of something would never trigger the "some values look unusual, continue anyway?" prompt, since the check that was supposed to catch it was silently checking `price` again instead.
2. **A merely-unusual price could generate two misleading messages** ("Unit price is unusual" *and* "Quantity is unusual") for what was really a single issue, since both conditions fired off the exact same `price` comparison.

**Fixed** by changing the condition to actually test `quantity` against `minQuantity`/`maxQuantity`, and correcting `focusTextBox` to point at the quantity field (`rowTextBoxes[row, 2]`) rather than the unit cost field (`rowTextBoxes[row, 1]`) when it fails, matching what the rest of the method does consistently for every other field it checks.

---

## Worth Reading: The Full Two-Tier Validation Pattern in One Place

```csharp
// Hard validation (okButton_Click, first block): blocks outright
if (firstNameTextBox.Text.Length == 0) { message += "First name cannot be blank.\n"; ... }
...
if (message.Length > 0) { MessageBox.Show(...); return; }   // stops here, no way past this

// Sanity checks (okButton_Click, second block): asks first
if (firstNameTextBox.Text.Length < minNameLength) { message += "...is unusually short.\n"; ... }
...
if (message.Length > 0)
{
    message = "Some fields contain unusual values.\n\n" + message + "\nDo you want to continue anyway?";
    if (MessageBox.Show(..., MessageBoxButtons.YesNo, ...) == DialogResult.Yes) Close();
    else focusTextBox.Focus();
}
```

This is the fullest, most concrete version of the distinction the main lesson's `UsingSanityChecks()` only summarized: two entirely separate passes over the form's data, one that can never be overridden (blank required fields), and one the user can explicitly choose to override (unusual-but-possible values), each with its own message-building loop and its own dialog. Worth reading start to finish as the complete, real-world shape that pattern actually takes in a full application, not just the isolated concept.

---

## Real-Time Validation, Field by Field

```csharp
private void firstNameTextBox_TextChanged(object sender, EventArgs e)
{
    ValidateTextBoxPattern(firstNameTextBox, false, namePattern);
    EnableOkButton();
}
```

Every input field validates itself the moment its text changes, painting itself yellow (`invalidColor`) or leaving it the normal window color (`validColor`), and `EnableOkButton()` re-checks every field's current background color to decide whether the OK button should even be clickable yet. Worth noticing this means the OK button being enabled is itself a *derived* signal, computed from every field's validation state, not a separate thing that has to be kept in sync by hand at each call site.
