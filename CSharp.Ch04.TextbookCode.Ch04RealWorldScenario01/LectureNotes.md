# Ch04 Textbook Code: Order Entry Forms (Real-World Scenario 1)

## What This Is

The "Order Entry Forms" real-world scenario `CSharp.Ch04.UsingTypes` mentions and explicitly skips covering in lecture. Unlike `CastingArrays`, this one's a fully working, interactive WinForms application, four line items (description, quantity, price each), a tax rate field, and computed subtotal/sales tax/grand total, all with real input validation.

No bugs found on this pass. Every validation path was traced by hand: empty rows are allowed through untouched, partially-filled rows correctly demand every field, quantity and price bounds are checked, and the tax rate is bounded between 0.00 and 0.20. Code is otherwise identical to the download, only the project file was modernized to SDK-style.

---

## Worth Actually Running This One

Unlike `CastingArrays`, there's no reason to need a debugger here, this form is meant to be used: type values into the four rows, enter a tax rate, hit OK, and watch the validation actually catch bad input (try leaving a quantity blank while filling in a description, or entering a tax rate over 0.20). It's a solid worked example of the "validate before you trust the data" pattern that shows up constantly in real form-driven applications, worth treating as a genuine lesson, not just an artifact to skip past.

## The Validation Pattern Worth Noticing

`ValidateRow()` returns `true` on error, `false` on success, which reads backwards at first (`if (ValidateRow(...)) return;` looks like "if valid, stop"), but makes the calling code in `okButton_Click()` read cleanly: each row gets one line to validate-and-bail, no nested `if`/`else`, no flag variables. `ValidateRequiredTextBox()` and `DisplayErrorMessage()` are both small, single-purpose helpers `ValidateRow()` leans on repeatedly, exactly the kind of decomposition that keeps a four-row, multi-field validation routine from turning into an unreadable wall of `if` statements.
