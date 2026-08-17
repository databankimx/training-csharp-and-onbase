# Ch05 Textbook Code: Ellipses and Circles

## What This Is

Functionally identical to `CSharp.Ch05.TextbookCode.Ch05RealWorldScenario01`, same `Ellipse`, `Circle`, and `Form1_Load()` validation test. The publisher shipped this as a separate standalone sample project in the download, alongside the "Shape Resources" real-world scenario it's a duplicate of.

The one real difference: `Form1.Designer.cs` in **this** copy correctly wires `Form1_Load` to the `Load` event. No bug to fix here, unlike `Ch05RealWorldScenario01`, which had exactly this bug.

---

## Worth Knowing, Not a Bug

Same as `Ch05RealWorldScenario01`: all six test constructors run inside a single `try`, so the first invalid one (`e1`, negative width) throws immediately and every line after it never executes. Meant to be stepped through in the debugger one line at a time, or with earlier lines commented out once you've seen them fail.
