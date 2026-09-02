# Samples.WinForms

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port), demonstrating WinForms' genuinely different, imperative interaction pattern, deliberately built alongside `Samples.Wpf` rather than sharing its MVVM approach. See `README.md` for the fuller when-to-use discussion.

---

## `net48`, This Solution's Baseline

Like `Samples.Wpf`, this project was originally built directly on `net10.0-windows`, then corrected to `net48` to match this solution's actual policy (`net48` is the baseline; a `net10.0`/Core sibling is only added to illustrate a genuine difference, not by default). The correction was a data-access and startup change, not a structural rewrite:

- **EF6 Database-First**, not EF Core Code-First, matching every other `net48` sample's own `.edmx`.
- **`App.config`**, not `appsettings.json`. `ExternalDataEntities` reads its connection string automatically via `base("name=ExternalDataEntities")`, so `MainForm`'s constructor no longer needs any manual configuration-reading code at all.
- **`DatabankException`**, not plain `Exception`, in `BtnSearch_Click`'s `catch` block, `CSharp.SharedLibrary` is a valid reference again on `net48`.
- **`ApplicationConfiguration.Initialize()` doesn't exist on `net48`.** That's a modern .NET 6+-only WinForms SDK feature. `Program.cs`'s `Main()` uses the classic, equivalent pattern instead: `Application.EnableVisualStyles()` + `Application.SetCompatibleTextRenderingDefault(false)`.
- **Explicit `using` directives in `MainForm.Designer.cs`.** Dropping `<ImplicitUsings>` (a modern-SDK-only feature not carried over to the `net48` `.csproj`) meant `System.Drawing`/`System.Windows.Forms`/`System.ComponentModel` needed explicit `using` statements that the original version got for free.

---

## Deliberately Not MVVM

`Samples.Wpf` is built around MVVM (`ViewModelBase`, `RelayCommand`, `ObservableCollection`, zero `System.Windows` references in the ViewModel). This project deliberately does **not** follow that pattern, WinForms genuinely doesn't have the same culture around it. `MainForm.cs`'s `BtnSearch_Click` reads `txtZipCode.Text`, runs the EF6 query, and assigns `gridResults.DataSource` all directly, in one method, on the code-behind class itself. Worth recognizing this isn't a shortcut or an oversight, it's what real-world WinForms code overwhelmingly looks like. (WinForms does support data binding via `BindingSource`, but it's an opt-in feature most codebases don't reach for, not the default expectation the way binding is in WPF.)

---

## The UI Is Generated Code, Not Markup

```csharp
lblZipCode = new Label();
lblZipCode.Location = new Point(16, 20);
lblZipCode.Text = "Zip Code:";
...
Controls.Add(lblZipCode);
```

`MainForm.Designer.cs` is what Visual Studio's WinForms Designer would normally generate automatically as you drag controls onto a form in the visual editor, plain C# object construction and property assignment, not a separate markup language. Compare this directly against `Samples.Wpf/MainWindow.xaml`, which declares an equivalent UI shape (a text input, a search button, an error label, a results grid) in XAML instead. Both approaches produce a working desktop window; the mechanism is genuinely different.

---

## Try It Yourself

Run the project, search a ZIP code, and open `MainForm.cs` directly, notice there's no `ViewModel`, no `Command`, no `ObservableCollection`, just a `Click` event handler doing the whole job start to finish. Then compare it line-by-line against `Samples.Wpf/ViewModels/MainViewModel.cs`.
