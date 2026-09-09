# Unity.TestHarness.Web

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

An ASP.NET MVC reproduction of `Unity.TestHarness` (the WPF version), matching its functionality, look, and feel. It exercises `Unity.00`&ndash;`04` end to end: connecting with any of the four authentication modes, browsing the full taxonomy hierarchy, retrieving documents, and creating, updating, and deleting them.

`net48`, a legacy-style ASP.NET Web Application project (classic ASP.NET has no official SDK-style support), referencing `Unity.00`&ndash;`04` directly, the same way the WPF version does.

---

## What's Built So Far

| Page | Status |
|---|---|
| **Connect** | Done &mdash; connect/disconnect, reconnect-by-session-ID, Application Server ping check (runs automatically on every visit), Keep Alive toggle, a Close button that disconnects and attempts to close the tab |
| **Taxonomy** | Done &mdash; cascading Document Type Group -> Document Type -> Keyword Group Type -> Keyword Type browser (AJAX-driven), plus Custom Query/File Type/Unity Form lookups |
| **Retrieval** | Done &mdash; three search modes (Document Type(s), Custom Query, direct Document ID), a results list, and a full document detail pane (metadata, keyword groups, Revision/Rendition browsing via AJAX, DocPop/UnityPop links, file retrieval) |
| **Archiving** | Done &mdash; Store New (drag/drop or file picker, document type selection, dynamic keyword editing), Modify Metadata, Add Revision, Add Rendition (the latter two gated by `Revisable`/`Renditionable`), and Delete/Purge |
| **Settings** | Done &mdash; full ServiceLocation/IdpSettings/DocPop editor, Apply (in-memory) vs Save to Web.config (persists); an admin-style, global page &mdash; static, process-wide settings intentionally shared across every user of the app, not per-session |
| **Help** | Done &mdash; per-page usage instructions, an About section, and a License/Support popup |

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Global.asax`/`.cs` | Application entry point: Serilog bootstrap (reads `serilog.json` directly via Newtonsoft.Json, not `Microsoft.Extensions.Configuration`, see `LectureNotes.md`), route/bundle/filter registration, and `Session_End` (the delayed safety net for disconnecting an orphaned session when a browser tab closes without a manual Disconnect) |
| `serilog.json` | Structured logging config: rolling debug/error log files (no Console sink, unlike the WPF version, there's no interactive console in a web process) |
| `Web.config` | `InProc` session state (a genuine requirement, not just a default, see `LectureNotes.md`), the `<onBaseSettings>` config section, and hand-written assembly binding redirects |
| `Infrastructure/SessionConnectionManager.cs` | The web equivalent of `ConnectionViewModel`: holds the live `Application` connection in `Session`, not as a field on a long-lived view model instance |
| `Infrastructure/SessionLog.cs` | The web equivalent of `LogViewModel`: the shared output log, held per-session instead of as a bound `ObservableCollection`, still dual-logging to Serilog |
| `Controllers/` | One controller per page (`ConnectController`, `TaxonomyController`, `RetrievalController`, `ArchivingController`, `SettingsController`, `HelpController`), plus `HomeController` for the one cross-cutting action (clearing the output log) every page's layout needs |
| `Models/` | Page-facing DTOs. Where possible these reuse `Unity.03.DocumentRetrieval`'s own plain, serializable objects (`DocumentInfo`, `RevisionInfo`, `RenditionInfo`, `DocumentLink`) directly, rather than duplicating parallel web-specific versions |
| `Views/Shared/_Layout.cshtml` | The shared shell: collapsible sidebar navigation (SVG line icons, not emoji, `localStorage`-persisted collapse state), connection status indicator, and the persistent Output panel |
| `Views/Archiving/_KeywordEditor.cshtml` | The dynamic keyword-group/standalone-keyword editor (Add/Remove Instance/Value), shared by Store New and Modify Metadata |
| `Views/Retrieval/_DetailPane.cshtml` | The AJAX-loaded document detail pane, shared across Retrieval's result-selection and revision/rendition-selection actions |
| `Content/site.css` | The shared "modern, sleek" design language every page draws from &mdash; CSS custom properties mirroring the WPF version's brushes, shared card/button/list/badge classes |
| `Resources/AppIcon.png`, `Resources/LICENSE` | The app icon (also used as the favicon) and the license text, both served as real static files (`Content` build action, not `Resource`, see `LectureNotes.md`) |

---

## Related Samples

- **`Unity.TestHarness`** &mdash; the WPF version this project reproduces. Where behavior differs here, it's because of a genuine web-platform constraint (session state, no direct filesystem access for uploads, no way to detect a closed browser tab), not a deliberate feature gap; those differences are called out in `LectureNotes.md`.
- **`Samples.MvcWebPortal`** &mdash; the project this one's own MVC conventions (project file structure, package version pins) are modeled on.
- **`Unity.00.CommonFunctionality`** through **`Unity.04.DocumentArchiving`** &mdash; the library suite this project exercises, the same one `Unity.TestHarness` exercises.
