# Ch07 Textbook Code: WPF App

## What This Is

The first WPF project in this migration, and the WPF counterpart to `CSharp.Ch07.TextbookCode.WinFormApp`, same one-button, one-label `BackgroundWorker` demo, this time in WPF instead of WinForms. No bugs found in the actual logic.

---

## Worth Noticing: Declarative Event Wiring Sidesteps the WinForms Sibling's Bug

```xml
<Button Name="btnRun" Grid.Row="1" Grid.Column="0" Click="btnRun_Click">Run</Button>
```

`CSharp.Ch07.TextbookCode.WinFormApp` had a real bug: its `btnRun.Click` was never wired to `btnRun_Click()` in the generated `Designer.cs` file, so clicking "Run" silently did nothing. That specific class of bug is structurally harder to hit in WPF: the `Click="btnRun_Click"` attribute lives directly in the XAML markup, right next to the control's own declaration, compiled straight into `InitializeComponent()` by the XAML compiler. There's no separate, easy-to-overlook wiring step the way there is with a WinForms `Designer.cs` file. Worth reading both projects back to back as a concrete illustration of how a framework's own design can make a whole category of mistake less likely, not by catching it after the fact, but by not leaving room for it to happen in the first place.

---

## `Dispatcher.Invoke`: WPF's Equivalent of `Control.Invoke`

```csharp
void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
{
    // lblResult.Content = e.Result;
    // Instead of updating the UI directly we call Dispatcher.Invoke
    this.Dispatcher.Invoke(() => lblResult.Content = e.Result);
}
```

Every UI framework has exactly one thread allowed to touch its controls (see `CSharp.Ch07.Supplemental.02.UnblockingTheUI`'s lecture notes for the WinForms version of this same idea). WPF's version of "marshal this call back to the UI thread" is `Dispatcher.Invoke()`, WinForms uses `Control.Invoke()`/`this.Invoke()` for the identical purpose. Worth noticing the commented-out direct assignment (`lblResult.Content = e.Result;`) sitting right above the actual call, again, a labeled "here's the naive approach, and here's why it needs wrapping" left directly in the code for reference. In this specific case, `BackgroundWorker.RunWorkerCompleted` is documented to already fire on the UI thread automatically, so the direct assignment would likely have worked, but the `Dispatcher.Invoke()` wrapper is the safe, general-purpose habit regardless of which specific mechanism triggered the callback.

---

## The Fix: A Namespace Inconsistency, Normalized

`App.xaml`/`App.xaml.cs` (no "Warning!" header, essentially this project's bootstrap plumbing, the WPF equivalent of a WinForms `Program.cs`) originally used `x:Class="CSharp.Ch07.TextbookCode.WpfApp.App"` and `namespace CSharp.Ch07.TextbookCode.WpfApp`, our own house naming convention. `MainWindow.xaml`/`MainWindow.xaml.cs` (the actual "Warning!"-labeled raw textbook content, and the original old-style `.csproj`'s own `RootNamespace`) use `WpfApp.MainWindow` and `namespace WpfApp` instead. Two different namespaces coexisting in one small project, this looks like a partially-completed rename left in the source archive, not raw textbook content (App.xaml.cs has no "Warning!" header at all).

Per established precedent from this migration (`CSharp.Ch05.TextbookCode.TreeEnumerator` and `UniversityClasses` had the same situation in reverse), the fix is to make **our own wrapper file match the raw content's namespace**, not the other way around: `App.xaml`/`App.xaml.cs` were changed to `WpfApp.App`/`namespace WpfApp`, matching `MainWindow` exactly, since `MainWindow` is the file that actually carries the "unedited textbook code" designation. This makes the whole project internally consistent under one namespace (`WpfApp`), while the `.csproj`'s `RootNamespace`/`AssemblyName` still follow this solution's folder-naming convention externally, `RootNamespace` doesn't force-rewrite a file's own explicit namespace declaration.
