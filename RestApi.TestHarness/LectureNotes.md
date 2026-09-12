# Lecture Notes: RestApi.TestHarness

## Scope: An Identical Application, Just a Different API

Built to be genuinely identical to `Unity.TestHarness` — same six pages, same functionality, same look and feel — except where the REST API itself genuinely can't do something Unity API can. Every such gap was researched and confirmed directly against `document-api.json`/`forms-api.json`, never assumed, and is documented explicitly rather than silently papered over:

1. **No "Reconnect to Session ID"** — the REST API's session is carried by an opaque cookie, not a client-suppliable session identifier (`RestApi.00.CommonFunctionality`'s own `ServiceLocation`).
2. **No DocPop/UnityPop links** — nothing in the REST API documentation reviewed exposes an equivalent (`RestApi.00.CommonFunctionality`'s own `RestApiSettings`).
3. **No Purge/permanent-delete option** — confirmed "purge" appears nowhere in `document-api.json` (`RestApi.04.DocumentArchiving`'s own `DeleteRequest`).
4. **Add Revision/Add Rendition are always enabled once a document is loaded, not conditionally** — confirmed no REST equivalent of Unity API's pre-flight `CanAddRevision`/`CanAddRendition` check exists; a disallowed attempt surfaces as an ordinary failed-request error instead (`RestApi.04.DocumentArchiving`'s own `DocumentStorage`).

None of these were discovered while building this app itself — each was confirmed earlier, while building the `RestApi.0X` library layer, and is simply carried through consistently here.

---

## net10.0-windows, Not net48

Unlike `Unity.TestHarness`, nothing in the `RestApi.*.*` track depends on `Hyland.Unity` (a .NET-Framework-only assembly). The `-windows` suffix is needed specifically for `UseWPF` to work on modern .NET — WPF itself is Windows-only regardless of target framework, but the SDK needs that explicit marker to enable it.

---

## Async/Await Ripples Through Every Layer

`RestApi.01`-`04` are genuinely async (real HTTP calls), unlike Unity API's synchronous SDK. This had real, repeated consequences building this app, not just at the library boundary:

- **`ConnectionViewModel.ConnectCommand`/`DisconnectCommand` are `AsyncRelayCommand`, not `RelayCommand`.** This broke a pattern several other view models relied on in the Unity version: calling `connection.ConnectCommand.Execute(null)` synchronously, then immediately checking `connection.IsConnected`, assuming `Execute()` had already finished. `AsyncRelayCommand.Execute()` is fire-and-forget from the caller's perspective, so that assumption silently breaks. Fixed by exposing `ConnectionViewModel.ConnectAsync()` as an ordinary awaitable method, which every page that needs to "connect first if not already connected" now calls directly instead of going through the `ICommand` indirection.
- **`RecomputeCommonKeywords` (Taxonomy/Retrieval) and `SelectedDocumentType`'s setter (Archiving) had to become fire-and-forget async** (`_ = SomeAsync();`), since the property setters that trigger them can't themselves be `async`, but the work they kick off (real HTTP calls via `RestApi.02`) genuinely is.
- **`KeywordEditorSet`'s two constructors became static async factory methods** (`CreateForDocumentTypeAsync`/`CreateForDocumentAsync`) instead, since C# constructors can't be `async` at all, and building one now requires awaiting real HTTP calls.
- **`MainWindow`'s `Closing` handler needed a cancel-then-close pattern**, not a direct call: `DisconnectAsync()` is genuinely async, so a naive port of Unity's synchronous disconnect-on-close would let the window close before the disconnect request even started (or completed). The fix cancels the first `Closing` event, awaits the disconnect, then calls `Close()` again programmatically.

---

## KeywordEditorSet.CreateForDocumentAsync Is Simpler Than Unity's Own Second Constructor

Unity's own `KeywordEditorSet(Document doc)` constructor walks `doc.KeywordRecords` directly to reconstruct each group/standalone value's structure from lower-level Unity API objects. The REST version doesn't need an equivalent walk at all: `RestApi.03`'s own `GetDocumentInfoAsync(documentId)` already returns the document's actual values pre-organized into `KeywordGroups` (each carrying its own `TypeGroupId`, after the correction documented in `RestApi.03`'s own `LectureNotes.md`) and standalone `Keywords`. `CreateForDocumentAsync` just matches each schema group's `TypeGroupId` against the document's own `KeywordGroups` to find its existing instance(s) - a simplification that came from the underlying API's own richer response shape, not extra work added here.

---

## Not Yet Wired: Retrieval's "Edit in Archiving"

`DocumentDetailViewModel.EditInArchivingCommand` raises `EditInArchivingRequested`, and `ArchivingViewModel.LoadDocumentForEditing(id)` exists to handle it, matching Unity.TestHarness's own design - but `MainViewModel` doesn't yet subscribe to that event and perform the actual cross-page navigation. This is a real gap in this build, not a confirmed REST-API limitation like the four above, worth completing when this app is revisited.

---

## Correction: RestApi.01's SessionManagement Is No Longer Static

RestApi.01's `SessionManagement` was originally `static`, and this app's own view models called it that way throughout (`SessionManagement.ConnectAsync()`, `SessionManagement.ServiceLocation`, etc.). Once `RestApi.TestHarness.Web` needed to exist (a web app, where every user needs their own IdP token/session, not one shared across every visitor — see `RestApi.01`'s own `LectureNotes.md`), `SessionManagement` was converted to an ordinary instance class, and every view model here had to be updated to match:

- `ConnectionViewModel` now owns one `SessionManagement` instance (`Session`), constructed once and living for this app's whole lifetime — the same effective lifecycle the old static class had, nothing about THIS app's own single-user behavior changes.
- Every other view model that previously called `OnBaseTaxonomy`/`DocumentRetrieval`/`DocumentStorage` without an explicit `HttpClient` (relying on those libraries' own now-removed static fallback) now passes `connection.GetHttpClient()` (or, for Unity Form lookups, `connection.Session.GetFormsHttpClient()`) explicitly to every call. This touched `TaxonomyViewModel`, `RetrievalViewModel`, `DocumentDetailViewModel`, `ArchivingViewModel`, and `KeywordEditorSet`'s own static factory methods (which gained an `HttpClient client` parameter for the same reason).
- `SettingsViewModel` now takes a `ConnectionViewModel` dependency too, reading/writing `connection.Session.ServiceLocation`/`.IdpSettings` instead of the old static `SessionManagement.ServiceLocation`/`.IdpSettings`.

No behavioral change for this app's own users — it's still single-session, single-user — but every call site that implicitly relied on a global default had to become explicit about which connection's `HttpClient` it means.
