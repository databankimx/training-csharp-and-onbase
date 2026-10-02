# Unity.TestHarness

## What This Is

A modern WPF/MVVM rebuild of the original 3-form WinForms test harness, exercising `Unity.00` through `Unity.04` end to end: connecting with any of the four authentication modes, browsing the full taxonomy hierarchy, retrieving documents, and creating/updating/deleting them, including the intentionally-stubbed paths those projects left as `NotImplementedException`.

`net48`, matching the rest of the Unity track -- this exercises the Unity API exactly as a real customer's own desktop app would.

---

## What's Built

| Page | Status |
|---|---|
| **Connect** | Connect/disconnect, session-ID reconnect, Application Server ping check, Keep Alive toggle |
| **Taxonomy** | Cascading Document Type Group -> Document Type -> Keyword Group Type -> Keyword Type browser, plus Custom Query/File Type/Unity Form Template lookups |
| **Retrieval** | Three search modes (Document Type(s), Custom Query, direct Document ID), a results list, and a full document detail pane (metadata, keyword groups, Revision/Rendition browsing, DocPop/UnityPop links, file retrieve/view) |
| **Archiving** | Store New (drag/drop or file picker, document type selection, keyword editing), Modify Metadata, Add Revision, Add Rendition, Delete/Purge |
| **Settings** | Full ServiceLocation/IdpSettings/DocPop editor, Apply (in-memory) vs Save to App.config (persists) |
| **Help** | Per-page usage instructions and an About section |

---

## Architecture Overview

### Why MVVM, Not WinForms

The original harness was three WinForms with cascading combo-box event handlers doing the work inline. That pattern works but doesn't scale cleanly to a sidebar-navigation shell with shared connection state and a shared output log across pages. MVVM: each page is its own view model, `MainViewModel` owns navigation and the two things every page needs (the shared `Log` and the shared `Connection`).

### ConnectionViewModel Is Two Things at Once

`ConnectionViewModel` is both the Connect page's own view model and the shared connection state (`MainViewModel.Connection`) every other page reads to get `CurrentApplication`. One instance serves both roles deliberately -- there's exactly one connected `Application` at a time, exactly one place that should own it. `Taxonomy`, `Retrieval`, and `Archiving` never call `Connect()`/`Disconnect()` themselves; they just read `Connection.CurrentApplication`.

### Settings: Apply vs. Save to App.config

`Apply` mutates `SessionManagement.ServiceLocation`/`IdpSettings` in-memory only. `Save to App.config` also writes to disk. Constructing a `ServiceLocation` manually (as `Apply()` does) never runs `PostDeserialize()`, so `Apply()` calls a new public `ServiceLocation.Validate()` explicitly -- extracted from `PostDeserialize()` precisely for this purpose -- to get the same "AuthenticationMode 'X' requires Y" errors that App.config loading gets automatically.

### Pages Are Built Lazily

Pages are only constructed the first time they're visited. Their view model state is created on first access, not at startup. This keeps startup fast and means a page's initialization logic (loading taxonomy, checking the connection) only runs when that page is actually opened.

---

## Key Design Decisions

### AsyncRelayCommand

`ICommand.Execute` is a framework-mandated `void Execute(object)` signature -- there's no eliminating `async void` entirely when implementing commands. But a view model's actual async method (`ConnectionViewModel.TestServer()`, `TaxonomyViewModel.Load()`, etc.) can be written as a genuine `private async Task Foo()`, satisfying static analyzers at the layer that can actually do something about it.

`AsyncRelayCommand` also makes `CanExecute` automatically return `false` while the command is running, preventing double-invocation of anything network-bound.

### PasswordBoxBehavior

`PasswordBox.Password` deliberately isn't a `DependencyProperty` -- WPF's designers made that choice to prevent credentials from sitting in binding expressions visible to UI frameworks. `Behaviors/PasswordBoxBehavior.cs` is a standard attached-property bridge, routing the `Password` value through an attached property that IS bindable, without exposing the actual password in XAML.

### Taxonomy: StandAlone vs. Named Groups

`DocumentType.KeywordRecordTypes` includes a `StandAlone` pseudo-group holding keyword types that don't belong to any named group. Selecting a Document Type in the Taxonomy page splits that pseudo-group's own Keyword Types into `StandaloneKeywordTypes` directly -- no extra click needed to reach them. The same `RecordType.StandAlone` distinction drives behavior in `DocumentRetrieval.GetKeywordColumns` and `DocumentStorage.UpdateKeywordGroups`.

### Application Server Ping Check

`ConnectionViewModel.TestServer()` calls `Service.asmx`'s own `Ping` HttpGet operation directly, entirely bypassing the Unity API. This is useful for distinguishing "server unreachable" from "server up, but something else (credentials, configuration) is wrong" -- a distinction that a failed `Connect()` alone can't give you. Runs automatically every time the Connect page is navigated to, and on demand via its own button.

### Logging: Serilog + LogViewModel

`App.xaml.cs`'s `OnStartup` builds `Log.Logger` via `Serilog.Settings.Configuration`'s `ReadFrom.Configuration(...)`, deliberately BEFORE `base.OnStartup(e)` (which creates the `StartupUri` window), so logging is ready before anything else runs.

`LogViewModel`'s `Info`/`Success`/`Error` methods write to BOTH the in-app `Entries` collection (what you see while the harness is running) AND Serilog's console/file sinks (what persists past that session). The private helper was renamed from `Log()` to `AddEntry()` specifically because a method name that shadows a type name in the same scope prevents `Log.Information(...)` from compiling.

### Session Disconnect on Window Close

`MainWindow.xaml.cs`'s `Closing` handler calls `Connection.DisconnectCommand` if still connected, releasing the concurrent client license. This only covers a normal close (close button, Alt+F4) -- a killed process can't run any handler at all. This is an inherent limit of any graceful-shutdown hook.

---

## Archiving: Keyword Editor Infrastructure

Keyword editing for both Store New and Modify Metadata flows through three shared view models:

- `KeywordEditorSet` -- the full set of keyword editors for a document type (standalone + named groups)
- `KeywordGroupEditor` -- one keyword group instance (for SingleInstance and MultiInstance groups)
- `StandaloneKeywordEditor` -- standalone keywords (not in any named group)

MultiInstance groups get one `KeywordGroupEditor` per existing instance, plus controls to add/remove instances. The complete set of instances is always submitted on save -- matching `Unity.04`'s `UpdateKeywordGroups` behavior of replacing all existing instances of a group with whatever the request contains.

---

## Missing Resources

`Resources\AppIcon.png` (the DataBank icon for the window/taskbar icon and sidebar branding) isn't in source control. Copy it in manually if you want the icon to render. The app builds and runs without it.

---

## Relationship to Unity.00-04

Unity.TestHarness is the end-to-end integration of the structured projects:

- `Unity.00.CommonFunctionality` -- `ServiceLocation`, `AuthenticationMode`, secrets management
- `Unity.01.ConnectingToOnBase` -- `SessionManagement.Connect()`, `IdpAuthentication`
- `Unity.02.AccessingTaxonomy` -- taxonomy browsing, `SplitKeywordGroups`, `GetCommonKeywordTypes`
- `Unity.03.DocumentRetrieval` -- `MakeDocumentQuery`, `GetDocumentInfo`, `GetDocumentFile`, DocPop links
- `Unity.04.DocumentArchiving` -- `CreateDocument`, `UpdateDocument`, `DeleteDocument`

The intentionally-stubbed paths in `Unity.03` and `Unity.04` (repeater support, form revision/rendition updates) appear in this harness as disabled controls or explicit "not yet implemented" messages where they would otherwise be expected.

---

## Takeaways

- `ConnectionViewModel` is both a page view model and shared connection state. One instance, two roles.
- `ServiceLocation.Validate()` was extracted from `PostDeserialize()` so Settings' `Apply` can validate without going through XML deserialization.
- `AsyncRelayCommand` keeps `async void` confined to the command infrastructure, letting view model logic be proper `async Task` methods.
- `PasswordBoxBehavior` is the standard WPF workaround for `PasswordBox.Password` not being a `DependencyProperty`.
- The Application Server ping check distinguishes "unreachable" from "reachable but misconfigured" before a full `Connect()` is attempted.
- `LogViewModel.AddEntry()` (not `Log()`) -- the name was changed to avoid shadowing `Serilog.Log` in the same scope.
- Keyword editing always submits the complete set of instances for MultiInstance groups, matching `Unity.04`'s replace-not-merge behavior.
