# Samples.Wpf

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port), demonstrating WPF and MVVM. See `README.md` for the fuller when-to-use discussion.

---

## `net48`, This Solution's Baseline

This project was originally built directly on `net10.0-windows`, on the reasoning that "this is the current correct way to build a new WPF app." That didn't match this solution's actual policy: `net48` is the baseline for every `Samples.*` project, a `net10.0`/Core sibling is only added when there's a genuinely illustrative difference worth showing side by side, the way `Samples.MvcWebApi`/`.Core` demonstrate real, documented contrasts. **Corrected** to `net48`, SDK-style `.csproj` still works fine here (`<UseWPF>true</UseWPF>` in a plain SDK-style project targeting `net48` has been supported since .NET Framework 4.7.2), so this wasn't a structural rewrite, mainly a data-access and configuration story change:

- **EF6 Database-First**, not EF Core Code-First, matching every other `net48` sample's own `.edmx` (`Models/LocationLookupModel.edmx`, reverse-engineered from the same `ZipCodes` table `Samples.MvcWebPortal`/`Samples.WebForms` use).
- **`App.config`**, not `appsettings.json`. `ExternalDataEntities` (the EF6-generated `DbContext`) reads its connection string from `App.config`'s `<connectionStrings>` section automatically via `base("name=ExternalDataEntities")`, no manual configuration-reading code needed anywhere in this project at all, `App.xaml.cs`'s `OnStartup()` is now just `new MainViewModel()` and showing the window.
- **`DatabankException`**, not plain `Exception`. `CSharp.SharedLibrary` is a valid reference again on `net48` (unlike the `net10.0` samples, where it's incompatible), so `MainViewModel.Search()`'s `catch` block wraps failures in `DatabankException`, matching the standard applied throughout every other `net48` `Samples.*` project.
- **Synchronous EF6 query**, not `async`/`await`. `Search()` calls `.ToList()` directly rather than `ToListAsync()`, matching EF6's synchronous-only query API (the same reason `Samples.MvcWebPortal`'s controller is synchronous too).

If a genuine, illustrative difference between classic and modern WPF ever emerges, a `Samples.Wpf.Core` sibling can be added at that point, not by default.

---

## No Separate DTO Layer, and Why That's Fine Here

`Samples.MvcWebApi` needed a shared DTO library (`Samples.MvcWebApi.Common`) specifically because its data has to cross an HTTP boundary to a separate client process, JSON serialization needs a defined shape on both ends. `MainViewModel` has no such boundary: it queries `ExternalDataEntities` and binds the EF6 `ZipCode` entities directly to the `DataGrid`'s `ItemsSource`, in the same process, no serialization involved at all. Worth recognizing this as a genuine architectural difference, not an inconsistency, the DTO pattern solves a problem (a network boundary) that simply doesn't exist here.

---

## `RelayCommand`: Standard, Unavoidable MVVM Boilerplate

WPF's `ICommand` interface has no built-in generic implementation, every MVVM codebase either writes one (as here, `ViewModels/RelayCommand.cs`) or takes a dependency on a library that provides one (CommunityToolkit.Mvvm's `RelayCommand`/`AsyncRelayCommand` being the most common today). Worth knowing this isn't a DataBank-specific pattern, it's close to universal in real WPF codebases, and recognizing it in an unfamiliar codebase is a genuinely useful skill.

---

## Try It Yourself

Run the project, search a ZIP code, and watch the `DataGrid` populate without a single explicit "refresh the UI" call anywhere in the code, adding to the bound `ObservableCollection` is enough. Then open `MainWindow.xaml.cs` and confirm for yourself: no click handler, no query logic, nine lines total.
