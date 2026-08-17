# Ch05 Textbook Code: Shape Resources (Real-World Scenario 1)

## What This Is

The "Shape Resources" real-world scenario `CSharp.Ch05.ImplementingClassHierarchies` mentions and deliberately skips covering in lecture. `Ellipse` and `Circle` (`Circle : Ellipse`, with constructor validation requiring width equals height) are the classes carried into that project for reference, this is where they actually come from.

---

## The Bug That Was Here

Same pattern as `CSharp.Ch04.TextbookCode.ShortPathNames`: `Form1_Load()` is fully written and clearly meant to run automatically, testing six different `Ellipse`/`Circle` constructions, some valid, some deliberately invalid, but the original `Form1.Designer.cs` never actually subscribed it to the form's `Load` event. The method just sat there, never called, `InitializeComponent()` had no `this.Load += ...` line at all. Running the raw download would open a blank window and never run a single line of the demo.

Fixed by adding the missing wire-up:

```csharp
this.Load += new System.EventHandler(this.Form1_Load);
```

Two labs in this same chapter, downloaded from the same publisher, hitting the identical class of bug, an event handler defined but never subscribed, is worth noticing as its own pattern. It's an easy mistake to make (write the handler, forget the one line that actually connects it), and it fails silently rather than throwing or warning, which is exactly why it's worth specifically checking for whenever a WinForms lab seems to do nothing at all.

---

## Worth Knowing, Not a Bug

```csharp
try
{
    Ellipse e1 = new Ellipse(new RectangleF(0, 0, -10, -10)); // throws immediately, negative width
    Ellipse e2 = new Ellipse(0, 0, -10, -10);
    // ... four more lines, none of which ever run
}
catch (Exception ex)
{
    MessageBox.Show(ex.Message);
}
```

All six test constructions are wrapped in a single `try`. The very first one (`e1`) is deliberately invalid, negative width, so it throws immediately, and every line after it never executes at all. This isn't something to fix, it's meant to be stepped through in the debugger one line at a time (or with earlier lines commented out once you've seen them fail), the same pattern `CastingArrays` and `CloneArray` use elsewhere in this chapter set. Run it straight through without a debugger, and all you'll ever see is the first failure.
