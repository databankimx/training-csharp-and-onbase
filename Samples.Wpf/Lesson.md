# Samples.Wpf

## What This Is

A WPF (Windows Presentation Foundation) desktop application demonstrating **MVVM** (Model-View-ViewModel) -- WPF's standard architectural pattern. Same ZIP code lookup as every other Samples project, backed by EF6 Database-First. A long-lived window with declarative XAML data binding, not a request/response cycle.

Targets `net48`. WPF was ported to modern .NET, so a `net10.0` sibling could be added if a meaningfully illustrative difference emerges -- not added by default.

---

## When to Use WPF

For Windows-only desktop applications where rich, flexible UI (custom styling, complex layouts, animations, data virtualization for large lists) matters more than cross-platform reach. WinForms (`Samples.WinForms`) is simpler and faster for straightforward forms-over-data applications; WPF is the better fit once UI needs are more demanding.

---

## How MVVM Works in WPF

**The core idea:** the View (XAML) knows nothing about the data. The ViewModel knows nothing about WPF. They communicate through data binding and commands.

**`MainWindow.xaml.cs` is nine lines** -- `InitializeComponent()` and nothing else. No event handlers, no control manipulation:

```xml
<!-- MainWindow.xaml -->
<TextBox Text="{Binding ZipCode, UpdateSourceTrigger=PropertyChanged}" />
<Button Command="{Binding SearchCommand}" Content="Search" />
<DataGrid ItemsSource="{Binding Locations}" />
```

**`MainViewModel.cs` imports no `System.Windows` namespace at all.** It exposes properties and commands; WPF's binding engine connects them to controls automatically:

```csharp
public class MainViewModel : ViewModelBase
{
    public string ZipCode { get; set; } // raises PropertyChanged via ViewModelBase
    public ObservableCollection<ZipCode> Locations { get; } = new();
    public ICommand SearchCommand { get; }

    public MainViewModel()
    {
        SearchCommand = new RelayCommand(_ => Search());
    }

    private void Search()
    {
        Locations.Clear();
        using var db = new ExternalDataEntities();
        foreach (var row in db.ZipCodes.Where(z => z.ZipCode == ZipCode))
            Locations.Add(row);
    }
}
```

**`INotifyPropertyChanged`** (implemented in `ViewModelBase`) is what makes two-way binding work: when `ZipCode` changes on the ViewModel, `PropertyChanged` fires and WPF's binding engine keeps the TextBox in sync.

**`ObservableCollection<T>`** is what makes the DataGrid update automatically: adding to it fires `CollectionChanged`, which WPF's binding engine responds to. A plain `List<T>` assigned once to `ItemsSource` would never update the UI after the initial bind.

**`RelayCommand`** wraps a delegate as an `ICommand`. The button's `Command` binding calls `Execute` when clicked; `CanExecute` (optionally) controls whether the button is enabled.

---

## Creating a WPF Project

### Visual Studio

**File > New > Project**, search "WPF Application", choose the one matching your target framework (`.NET Framework` for `net48`, plain "WPF Application" for modern .NET), click Next, name it, click Create.

To apply MVVM: create `ViewModels/` and `ViewModels/ViewModelBase.cs` (implementing `INotifyPropertyChanged`), `ViewModels/RelayCommand.cs` (implementing `ICommand`), and `ViewModels/MainViewModel.cs`. In `MainWindow.xaml.cs`, set `DataContext`:

```csharp
public MainWindow()
{
    InitializeComponent();
    DataContext = new MainViewModel();
}
```

Add EF6 Database-First: right-click > **Add > New Item > ADO.NET Entity Data Model**.

### VS Code

```powershell
dotnet new wpf -n MyWpfApp -f net10.0-windows
```

VS Code has no XAML Designer or WPF visual tooling. XAML can be edited as text, and IntelliSense is available with the C# Dev Kit extension. For any significant UI work, Visual Studio's XAML Designer and live property inspector are strongly preferred. Use VS Code for the ViewModel and model code.

---

## Running This Project

1. Point `App.config`'s `ExternalDataEntities` connection string at a SQL Server instance.
2. Press F5.
3. Enter a ZIP code and click Search.

---

## Pros and Cons

**Pros:** ViewModel is fully unit-testable with no UI involved. Declarative XAML binding keeps UI logic out of code-behind. Rich styling and templating. `ObservableCollection` + binding makes data-driven UIs much simpler than WinForms' manual DataSource assignments.

**Cons:** Windows-only. XAML binding errors fail silently at runtime (no compile-time checking). Steeper learning curve than WinForms. No cross-platform story -- for cross-platform desktop, MAUI or Avalonia are the alternatives.

---

## Related Projects

- `Samples.WinForms` -- the simpler, event-driven sibling for direct comparison.
- `Unity.TestHarness` -- a full WPF/MVVM application in this training set, showing the same pattern applied to a much larger feature set.
