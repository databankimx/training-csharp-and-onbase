# RestApi.TestHarness

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

The REST API counterpart to `Unity.TestHarness` — the same WPF desktop app, same six pages (Connect, Taxonomy, Retrieval, Archiving, Settings, Help), same look and feel, just talking to the OnBase Document Management (REST) API via `RestApi.00`–`04` instead of the Unity API.

`net10.0-windows` (not net48): nothing in the `RestApi.*.*` track depends on `Hyland.Unity`, so there's no architectural reason to stay on net48 here.

---

## What's Built So Far

| Page | Status |
|---|---|
| **Connect** | Done — connects via the Hyland IdP, establishes/disconnects an OnBase session, Keep Alive toggle. No "Reconnect to Session ID", no "Test API Availability" ping — confirmed gaps, see `LectureNotes.md`. |
| **Taxonomy** | Done — cascading Document Type Group → Document Type → Keyword Group Type browser, plus Custom Query/File Type/Unity Form Template lookups (Unity Forms genuinely work here, via the separate Forms API). |
| **Retrieval** | Done — three search modes (Document Type(s), Custom Query, direct Document ID), a results list, and a full document detail pane (metadata, keyword groups, Revision/Rendition browsing, file retrieval with server-decided PDF conversion). |
| **Archiving** | Done — Store New (drag/drop or file picker, dynamic keyword editing), Modify Metadata, Add Revision, Add Rendition (both always offered once a document is loaded — no REST equivalent of a pre-flight Revisable/Renditionable check exists), and Delete (no Purge option — confirmed gap). |
| **Settings** | Done — connection settings editor, Apply (in-memory) vs Save to App.config (persists); an admin-style, global page (matching `RestApi.00`'s own confirmed design). |
| **Help** | Done — per-page usage instructions, an About section, and a License/Support popup. |

---

## What's in This Project

| Path | Purpose |
|---|---|
| `App.xaml`/`.cs` | Serilog bootstrap, shared styling resources (colors/brushes/control templates), matching `Unity.TestHarness`'s own palette so the two apps look identical. |
| `MainWindow.xaml`/`.cs` | The shell: collapsible sidebar navigation, the shared Output log, and a `Closing` handler that awaits a proper async disconnect before actually letting the window close (see `LectureNotes.md` — this genuinely differs from Unity's synchronous version). |
| `ViewModels/MainViewModel.cs` | Sidebar navigation state, lazy-created-and-cached page view models. |
| `ViewModels/ConnectionViewModel.cs` | The Connect page, and the shared connection state every other page checks before acting. |
| `ViewModels/TaxonomyViewModel.cs`, `RetrievalViewModel.cs`, `DocumentDetailViewModel.cs`, `ArchivingViewModel.cs`, `KeywordEditorSet.cs`/`KeywordGroupEditor.cs`/`StandaloneKeywordEditor.cs`, `SettingsViewModel.cs`, `HelpViewModel.cs` | One (or more) view model(s) per remaining page. |
| `Behaviors/PasswordBoxBehavior.cs`, `Converters/` | Generic WPF infrastructure, ported essentially verbatim from `Unity.TestHarness` (no Unity/REST API dependency at all). |
| `Models/` | Page-facing DTOs/enums (`SearchMode`, `ArchivingMode`, `SelectableItem<T>`, `SearchKeywordField`, `GroupInstance`). |

---

## Related Samples

- **`Unity.TestHarness`** — the Unity API version this project reproduces. Four confirmed feature gaps exist between the two apps (no Session ID reconnect, no DocPop/UnityPop links, no Purge, Add Revision/Add Rendition always enabled rather than conditionally) — each researched and confirmed against the REST API's own OpenAPI specs, not assumed. See `LectureNotes.md` for the full picture.
- **`RestApi.00.CommonFunctionality`** through **`RestApi.04.DocumentArchiving`** — the library suite this project exercises.
