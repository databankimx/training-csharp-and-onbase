# Samples.Wpf

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

A WPF (Windows Presentation Foundation) desktop application, demonstrating **MVVM** (Model-View-ViewModel), WPF's standard architectural pattern. Same domain as every other sample in this training set (look up city/county/state by ZIP code, backed by EF Core against the same conceptual `ZipCodes` table), a different UI paradigm entirely: a long-lived desktop window with declarative data binding, not a request/response cycle.

Targets **.NET 10**, the current, correct way to build a new WPF application, WPF was genuinely ported to modern .NET (unlike Web Forms, which has no modern equivalent at all, see `Samples.WebForms`'s own `LectureNotes.md`).

---

## When to Use WPF

For Windows-only desktop applications where rich, flexible UI (custom styling, complex layouts, animations, data virtualization for large lists) matters more than cross-platform reach. WinForms (`Samples.WinForms`) remains a simpler, faster option for straightforward forms-over-data applications; WPF is the better fit once the UI needs are more demanding.

---

## What Makes MVVM Genuinely Different

- **No event handlers in code-behind.** `MainWindow.xaml.cs` is nine lines, `InitializeComponent()` and nothing else. The Search button's `Command` is bound directly to `SearchCommand` on the ViewModel in XAML markup, not wired up with a `Click="..."` handler.
- **The ViewModel knows nothing about WPF.** `MainViewModel.cs` imports no `System.Windows` namespace at all. It could be unit tested with zero UI involved, a genuine, practical benefit, not just an architectural nicety.
- **Long-lived, mutable UI state.** `Locations` is an `ObservableCollection<ZipCode>`, adding to it updates the on-screen `DataGrid` automatically. Contrast this against every web sample here, where a "changed" value is simply rendered fresh on the next request, there's no persistent UI object being mutated in place.
- **`INotifyPropertyChanged`** is the mechanism that makes two-way binding possible at all: when `ZipCode` changes, `PropertyChanged` fires, and WPF's binding engine knows to keep the `TextBox` and the ViewModel property in sync.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `MainWindow.xaml` / `.xaml.cs` | The View, pure declarative bindings, essentially no code-behind |
| `ViewModels/MainViewModel.cs` | Bindable properties, the search command, EF Core query logic |
| `ViewModels/RelayCommand.cs` | Generic `ICommand` implementation (standard MVVM boilerplate) |
| `ViewModels/ViewModelBase.cs` | `INotifyPropertyChanged` base class |
| `Models/ZipCode.cs` | EF Core entity (Code-First) |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |

---

## How to Run

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Press F5 (or `dotnet run`).
3. Enter a ZIP code and click Search.

---

## Related Samples

- **`Samples.WinForms`** — the older, simpler .NET desktop UI framework, worth comparing directly.
- **`Samples.WebForms`** — a very different kind of "long-lived state" illusion (postback + `ViewState` vs. WPF's genuinely persistent window and data binding).
