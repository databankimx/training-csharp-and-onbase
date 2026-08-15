# Ch04 Textbook Code: Order Entry Forms with Currency Display (Real-World Scenario 4)

## What This Is

The "Displaying Currency Values" real-world scenario `CSharp.Ch04.UsingTypes` mentions and skips near `StringFormat()`. Builds on `Scenario02` (the `%`-tolerant tax rate field), the only functional change is that every dollar amount on the form (`extendedPrice`, `subtotal`, `salesTax`, `grandTotal`) now goes through `.ToString("C")` instead of the plain `.ToString()` used in the earlier scenarios.

No new bugs, the underlying validation and tax logic is identical to `Scenario02`. Same publisher-side copy-paste artifact as `Scenario02` too, both `Program.cs` and `Form1.cs` still declared `namespace Ch04RealWorldScenario01` in the raw download, renamed here to match this project's folder.

---

## The Actual Lesson: `"C"` Does More Than You'd Guess

```csharp
extendedPrice1TextBox.Text = extendedPrice.ToString("C");
```

`"C"` is the standard currency format specifier, and it isn't just "add a dollar sign." It pulls the currency symbol, the decimal separator, the digit grouping, and even which side of the number the symbol goes on, all from the current culture. Run this on a machine set to `en-US` and you get `$1,234.56`. Run it on a machine set to `en-GB` and you'd get `£1,234.56` without changing a single line of code. That's the entire value of reaching for `"C"` instead of hand-building a currency string with `"$" + amount`, the formatting adapts to wherever the software actually runs, and you don't have to think about it.
