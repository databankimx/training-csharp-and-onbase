# Unity.TestHarness

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.
>
> **Missing file**: `Resources\AppIcon.png` (the DataBank icon used for the window/taskbar icon and sidebar branding) isn't in source control — copy it in yourself if you want the icon to render; the app builds and runs without it, just without an icon.

## What This Is

A modern WPF/MVVM rebuild of the original 3-form WinForms test harness, exercising `Unity.00`–`04` end to end: connecting with any of the four authentication modes, browsing the full taxonomy hierarchy, retrieving documents, and creating/updating/deleting them, including the intentionally-stubbed paths (repeater support, form revision/rendition updates) those projects left as `NotImplementedException`.

`net48`, matching the rest of the Unity track, not `net10.0`, this exercises the Unity API exactly as a real customer's own desktop app would.

---

## What's Built So Far

| Page | Status |
|---|---|
| **Connect** | Done — connect/disconnect, reconnect-by-session-ID, Application Server ping check, Keep Alive toggle |
| **Taxonomy** | Done — cascading Document Type Group -> Document Type -> Keyword Group Type -> Keyword Type browser, plus Custom Query/File Type/Unity Form Template lookups |
| **Retrieval** | Done — three search modes (Document Type(s), Custom Query, direct Document ID), a results list, and a full document detail pane (metadata, keyword groups, Revision/Rendition browsing, DocPop/UnityPop links, file retrieve/view) |
| **Archiving** | Done — Store New (drag/drop or file picker, document type selection, keyword editing), Modify Metadata, Add Revision, Add Rendition (the latter two gated by `Revisable`/`Renditionable`), and Delete/Purge |
| **Settings** | Done — full ServiceLocation/IdpSettings/DocPop editor, Apply (in-memory) vs Save to App.config (persists) |
| **Help** | Done — per-page usage instructions and an About section |

---

## What's in This Project

| Path | Purpose |
|---|---|
| `App.xaml` | The shared "modern, sleek" design system: colors, button/input styles, converters |
| `App.xaml.cs` | Serilog bootstrap (from `serilog.json`) and shutdown flush |
| `serilog.json` | Structured logging config: console + rolling debug/error log files |
| `MainWindow.xaml`/`MainViewModel.cs` | The sidebar navigation shell, shared output log, shared connection state |
| `ViewModels/ConnectionViewModel.cs` | Both the Connect page's own view model AND the shared connection state every other page reads |
| `ViewModels/SettingsViewModel.cs` | The Settings page |
| `ViewModels/TaxonomyViewModel.cs` | The Taxonomy page |
| `ViewModels/RetrievalViewModel.cs`/`DocumentDetailViewModel.cs` | The Retrieval page: search + results, and the selected document's detail pane |
| `ViewModels/ArchivingViewModel.cs` | The Archiving page: Store New / Modify Metadata / Add Revision / Add Rendition / Delete |
| `ViewModels/KeywordEditorSet.cs`/`KeywordGroupEditor.cs`/`StandaloneKeywordEditor.cs` | Archiving's keyword-editing infrastructure, shared by Store New and Modify Metadata |
| `ViewModels/HelpViewModel.cs` | The Help page (mostly static content, lives directly in HelpView.xaml) |
| `ViewModels/LogViewModel.cs` | The shared output log; every entry also goes to Serilog |
| `ViewModels/RelayCommand.cs`/`AsyncRelayCommand.cs` | ICommand implementations, the latter for genuinely asynchronous work |
| `Behaviors/PasswordBoxBehavior.cs` | Bridges PasswordBox.Password (not bindable, by WPF design) to an ordinary bindable string |
| `Converters/DocumentTypeGroupFilterConverter.cs` | Renders `null` (the "no filter" option) as "All Groups" in Retrieval's group filter dropdown |
| `HarnessSettings.cs` | Reads/writes the harness's own UI preferences (currently just the sidebar's collapsed/expanded state) to/from App.config |

---

## Related Samples

- **`Unity.00.CommonFunctionality`** through **`Unity.04.DocumentArchiving`** - this project is the thing that actually exercises them together, end to end.
