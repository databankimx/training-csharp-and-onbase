# Unity.TestHarness

## What This Is

A WPF/MVVM rebuild of the original 3-form WinForms harness. See `README.md` for the file breakdown and current build status.

---

## Why a Rebuild, Not a Port

The original harness (`UnityTestHarnessForm`, `DocumentHitListForm`, `KeywordsForm`) was three WinForms, each owning its own controls directly, with cascading combo-box event handlers (`SelDocTypeGroup_SelectedIndexChanged`, etc.) doing the actual work inline. That pattern works, but doesn't scale cleanly to a "modern, sleek" sidebar-navigation shell with a shared output log and shared connection state across pages, hence MVVM: each page is its own view model, `MainViewModel` owns navigation and the two things every page needs (the shared `Log` and the shared `Connection`), and pages are only built (and their state only created) the first time they're actually visited.

---

## Architecture Decisions Worth Knowing

**`ConnectionViewModel` is two things at once.** It's the `Connect` page's own view model, and it's the shared connection state (`MainViewModel.Connection`) every other page reads to get `CurrentApplication`. One instance serves both roles deliberately, there's exactly one connected `Application` at a time, exactly one place that should own it. `Taxonomy`, and eventually `Retrieval`/`Archiving`, never call `Connect()`/`Disconnect()` themselves, they just read `Connection.CurrentApplication`.

**`Settings`' `Apply` vs `Save to App.config` are genuinely different actions.** `Apply` mutates `SessionManagement.ServiceLocation`/`IdpSettings` in-memory only, `Save to App.config` additionally writes to disk. Constructing a `ServiceLocation` manually (as `Apply()` does) never runs `PostDeserialize()`, so `Apply()` calls a new public `ServiceLocation.Validate()` explicitly (extracted from `PostDeserialize()` for exactly this reason) to get the same "AuthenticationMode 'X' requires Y" errors App.config loading gets automatically. See `Unity.00.CommonFunctionality`'s own `ServiceLocation.cs` Training Notes for the full reasoning.

**Secret fields use real `PasswordBox` controls**, via `Behaviors/PasswordBoxBehavior.cs` (a standard attached-property bridge, since `PasswordBox.Password` deliberately isn't a `DependencyProperty`). This project is meant as a template other developers build real desktop apps from, "do it right" is the baseline here, not "good enough for an internal tool."

**`AsyncRelayCommand` exists to keep `async void` confined to exactly one place.** `ICommand.Execute` is a framework-mandated `void Execute(object)` signature, there's no eliminating `async void` entirely, but a view model's actual async method (`ConnectionViewModel.TestServer()`, `TaxonomyViewModel.Load()`, etc.) can be written as a genuine `private async Task Foo()`, satisfying analyzers like SonarQube's S3168 at the layer that's actually able to. It also makes `CanExecute` automatically false while the command is running, preventing double-invocation of anything network-bound.

**`Taxonomy` is a cascading browser, not a flat search form**, mirroring the original WinForms harness's own combo-box-chain pattern: Document Type Group -> its Document Types -> its Keyword Group Types (+ Standalone Keywords, in parallel) -> a group's own Keyword Types. Not every keyword belongs to a named group, `DocumentType.KeywordRecordTypes` includes a `StandAlone` pseudo-group (`RecordType.StandAlone`), the same distinction `DocumentStorage.cs`/`DocumentRetrieval.cs` check for; selecting a Document Type splits that pseudo-group's own Keyword Types into `StandaloneKeywordTypes` directly; no extra click needed to reach them.

**The Application Server ping check is deliberately separate from `Connect()`.** `Service.asmx`'s own `Ping` HttpGet operation is called directly (`ConnectionViewModel.TestServer()`), entirely bypassing the Unity API, useful for telling apart "server unreachable" from "server up, but something else (credentials, config) is wrong" - a distinction `Connect()` failing on its own can't give you. Runs automatically every time you navigate to `Connect`, and on demand via its own button.

**A connected session is disconnected on window close**, `MainWindow.xaml.cs`'s `Closing` handler calls `Connection.DisconnectCommand` if still connected, releasing the concurrent client license it holds. Only covers a normal close (close button, Alt+F4), a killed process can't run any handler at all, an inherent limit of any graceful-shutdown hook.

**Logging is Serilog, configured from `serilog.json`, not hardcoded in C#.** `App.xaml.cs`'s `OnStartup` builds `Log.Logger` via `Serilog.Settings.Configuration`'s `ReadFrom.Configuration(...)`, deliberately BEFORE calling `base.OnStartup(e)` (which is what actually creates the `StartupUri` window), so logging is ready before anything else runs. `LogViewModel`'s `Info`/`Success`/`Error` methods write to BOTH the in-app `Entries` collection (what you see while the harness is running) AND Serilog's console/file sinks (what persists past that). `LogViewModel`'s own private helper used to be named `Log()` too, renamed to `AddEntry()` specifically because a method name shadows a type name in the same scope in C#, `Log.Information(...)` wouldn't have compiled otherwise.

---

## Try It Yourself

Configure `Settings` against a real (or sandbox) OnBase environment, connect, then load `Taxonomy` and drill from a Document Type Group all the way down to an individual Keyword Type, noting where Standalone Keywords appear versus named groups.
