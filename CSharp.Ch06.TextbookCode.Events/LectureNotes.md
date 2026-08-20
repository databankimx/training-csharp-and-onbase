# Ch06 Textbook Code: Events

## What This Is

Functionally identical to `CSharp.Ch06.TextbookCode.BankAccount`, same `BankAccount`, `OverdrawnEventArgs`, and `Form1` content (only minor control coordinates/`TabIndex` values and the window title differ). The publisher shipped this as a separate standalone sample project in the download, alongside the "Overdraft Account" and "Factorials" real-world scenarios it sits next to.

No bugs found.

---

## Why Keep Both?

`Ch05RealWorldScenario01`/`EllipsesAndCircles` in Chapter 5 were the same situation, and the same reasoning applies here: this is genuinely how the publisher packaged the download, not a mistake introduced during migration. Rather than silently dropping the duplicate, it's preserved so the catalog accurately reflects what's actually in the source material, and so anyone comparing against the original download later finds exactly what they'd expect. See `CSharp.Ch06.TextbookCode.BankAccount`'s `LectureNotes.md` for the full walkthrough, including the older `if (Overdrawn != null)` null-check style worth comparing against the modern `?.Invoke()` used in `CSharp.Ch06.Supplemental.07.Events`.
