# Ch04 Textbook Code: Order Entry Forms (Real-World Scenario 2)

## What This Is

A variant of `Ch04RealWorldScenario01`, same order-entry form, same four line items and tax calculation, but the Tax Rate field now accepts a trailing `%` (`"15%"` as well as `"0.15"`).

No functional bugs found. `taxRateString.Replace("%", "")` strips the symbol before parsing, then `if (taxRateTextBox.Text.Contains("%")) taxRate /= 100;` checks the *original* string (not the stripped one) to decide whether to divide by 100, which correctly distinguishes `"15%"` (→ 0.15) from `"0.15"` (→ 0.15, unchanged) using the same downstream bounds check either way.

---

## A Copy-Paste Artifact From the Textbook Itself

The original download's `Program.cs` and `Form1.cs` both declared `namespace Ch04RealWorldScenario01`, in the Scenario 2 folder. Not something introduced during this migration, the publisher's own packaging carried the wrong namespace over from the first scenario. Since every `TextbookCode.*` project in this training set already gets renamed to match its own folder, this particular quirk disappears on its own, but it's worth knowing it was there, if you ever compare against the raw download and wonder why the namespaces looked identical between the two scenario folders.

---

## Worth Noticing: Checking the Original, Not the Modified, String

```csharp
string taxRateString = taxRateTextBox.Text;
taxRateString = taxRateString.Replace("%", "");

decimal taxRate;
if (!decimal.TryParse(taxRateString, out taxRate)) { /* ... */ }

if (taxRateTextBox.Text.Contains("%")) taxRate /= 100;
```

The `Contains("%")` check reads from `taxRateTextBox.Text`, the untouched original, not `taxRateString`, which already had the `%` stripped out by that point. Checking the modified copy here would always evaluate to `false`, since the `%` is already gone. Worth noticing as a small but real example of a common bug shape: once you've derived a "cleaned" version of a value, later logic needs to know which version, original or cleaned, it's actually supposed to be reading.
