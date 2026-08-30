# Samples.WinForms

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

A Windows Forms desktop application, the older, simpler sibling of `Samples.Wpf`. Same domain (look up city/county/state by ZIP code, backed by EF Core), a deliberately different, direct interaction pattern: event handlers manipulating controls imperatively, not MVVM data binding.

Targets **.NET 10**, WinForms was ported to modern .NET alongside WPF.

---

## When to Use WinForms

For simple, forms-over-data Windows-only desktop applications where the UI needs are straightforward and development speed matters more than visual flexibility. WPF (`Samples.Wpf`) is the better choice once the UI needs are more demanding, richer styling, complex layouts, genuine MVVM testability.

---

## What Makes This Genuinely Different From `Samples.Wpf`

- **No ViewModel, no data binding, no `ICommand`.** `BtnSearch_Click` in `MainForm.cs` reads `txtZipCode.Text` directly, runs the query, and writes results directly into `gridResults.DataSource`.
- **The UI is generated C# code, not markup.** `MainForm.Designer.cs` builds every control with `new Label()`, `Controls.Add(...)`, and direct property assignments, contrast this against `Samples.Wpf/MainWindow.xaml`'s declarative bindings for an equivalent UI shape.
- **Direct event subscription.** `btnSearch.Click += BtnSearch_Click;` wires the button straight to a method, no command object in between.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Application entry point |
| `MainForm.cs` | Event handler logic (the search) |
| `MainForm.Designer.cs` | Programmatically-generated UI layout |
| `Models/ZipCode.cs` | EF Core entity (Code-First) |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |

---

## How to Run

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Press F5 (or `dotnet run`).
3. Enter a ZIP code and click Search.

---

## Related Samples

- **`Samples.Wpf`** — the richer, MVVM-based sibling, worth comparing directly.
