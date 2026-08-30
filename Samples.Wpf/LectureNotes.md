# Samples.Wpf

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port), demonstrating WPF and MVVM. See `README.md` for the fuller when-to-use discussion.

---

## WPF Genuinely Has a Modern .NET Story

Unlike `Samples.WebForms` (which has **no** ASP.NET Core equivalent at all, a permanent Microsoft decision), WPF was ported to modern .NET starting with .NET Core 3.0, still Windows-only, but running on the same current, actively-developed runtime as every other `net10.0` project in this training set. This project targets `net10.0-windows` directly, `<UseWPF>true</UseWPF>` in a plain SDK-style `.csproj`, no legacy project format needed, `dotnet build`/`dotnet run` both work exactly as they do for the console and web samples.

---

## No `DatabankException`, No DI Container Automatically Provided

Same reasoning as every `net10.0` sample: `CSharp.SharedLibrary` targets `net48`, incompatible here, so standard exceptions are used directly (see `MainViewModel.SearchAsync()`'s plain `try`/`catch`).

Worth noting a second, related point: a WPF app has no `WebApplicationBuilder`-style host wiring up configuration or dependency injection automatically. `App.xaml.cs`'s `OnStartup()` builds a `ConfigurationBuilder` by hand (the same situation `Samples.MvcWebApi.Core.Client` was in, also a non-hosted app) and constructs `MainViewModel` directly, passing the connection string in. A more elaborate WPF application might use `Microsoft.Extensions.Hosting`'s Generic Host for a real DI container, that pattern is demonstrated on its own in `Samples.GenericHostConsole` rather than folded in here, to keep this sample focused specifically on MVVM and data binding.

---

## No Separate DTO Layer, and Why That's Fine Here

`Samples.MvcWebApi`/`Samples.MvcWebApi.Core` both needed a shared DTO library (`Samples.MvcWebApi.Common`/`.Core.Common`) specifically because their data has to cross an HTTP boundary to a separate client process, JSON serialization needs a defined shape on both ends. `MainViewModel` has no such boundary: it queries `LocationLookupContext` and binds the EF Core `ZipCode` entities directly to the `DataGrid`'s `ItemsSource`, in the same process, no serialization involved at all. Worth recognizing this as a genuine architectural difference, not an inconsistency, the DTO pattern solves a problem (a network boundary) that simply doesn't exist here.

---

## `RelayCommand`: Standard, Unavoidable MVVM Boilerplate

WPF's `ICommand` interface has no built-in generic implementation, every MVVM codebase either writes one (as here, `ViewModels/RelayCommand.cs`) or takes a dependency on a library that provides one (CommunityToolkit.Mvvm's `RelayCommand`/`AsyncRelayCommand` being the most common today). Worth knowing this isn't a DataBank-specific pattern, it's close to universal in real WPF codebases, and recognizing it in an unfamiliar codebase is a genuinely useful skill.

---

## Try It Yourself

Run the project, search a ZIP code, and watch the `DataGrid` populate without a single explicit "refresh the UI" call anywhere in the code, adding to the bound `ObservableCollection` is enough. Then open `MainWindow.xaml.cs` and confirm for yourself: no click handler, no query logic, nine lines total.
