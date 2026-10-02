# RestApi.TestHarness

## What This Is

The REST API counterpart to `Unity.TestHarness` -- the same WPF desktop app, same six pages, same look and feel, just talking to the OnBase Document Management (REST) API via `RestApi.00`-`04` instead of the Unity API.

`net10.0-windows`. Nothing in the `RestApi.*.*` track depends on `Hyland.Unity`, so there's no architectural reason to stay on `net48`. The `-windows` suffix enables WPF on modern .NET.

---

## What's Built

| Page | Status |
|---|---|
| **Connect** | Done -- IdP token, OnBase session, Keep Alive toggle |
| **Taxonomy** | Done -- cascading Document Type Group -> Document Type -> Keyword Group Type browser, Custom Query/File Type/Unity Form Template lookups |
| **Retrieval** | Done -- three search modes, results list, full detail pane (metadata, keyword groups, Revision/Rendition browsing, file retrieval) |
| **Archiving** | Done -- Store New, Modify Metadata, Add Revision, Add Rendition, Delete |
| **Settings** | Done -- Apply (in-memory) vs Save to App.config (persists) |
| **Help** | Done -- per-page usage instructions, About section |

---

## Four Confirmed Feature Gaps vs. Unity.TestHarness

Each was researched and confirmed against `document-api.json`/`forms-api.json` before this app was built -- none are assumptions:

1. **No "Reconnect to Session ID"** -- the REST API session is an opaque cookie, not a client-suppliable identifier. See `RestApi.00`'s `ServiceLocation`.
2. **No DocPop/UnityPop links** -- no equivalent in the REST API documentation. See `RestApi.00`'s `RestApiSettings`.
3. **No Purge/permanent-delete** -- "purge" appears nowhere in `document-api.json`. See `RestApi.04`'s `DeleteRequest`.
4. **Add Revision/Add Rendition always enabled** -- no REST pre-flight `CanAddRevision`/`CanAddRendition` equivalent. A disallowed attempt surfaces as a failed-request error instead. See `RestApi.04`'s `DocumentStorage`.

---

## Async Ripples Through Every Layer

`RestApi.01`-`04` are genuinely async -- real HTTP calls, not a synchronous SDK. This had repeated consequences building the view models:

### ConnectCommand Can't Be Called Synchronously

`Unity.TestHarness`'s page view models sometimes called `connection.ConnectCommand.Execute(null)` and immediately assumed `IsConnected` was true. `AsyncRelayCommand.Execute()` is fire-and-forget from the caller's perspective -- that assumption silently breaks. Fixed by exposing `ConnectionViewModel.ConnectAsync()` as an awaitable method. Pages that need "connect first if not already connected" call it directly instead of going through the `ICommand`.

### Property Setters Can't Be `async`

`RecomputeCommonKeywords` (triggered by document type selection changes in Taxonomy/Retrieval) and `SelectedDocumentType`'s setter in Archiving need to make real HTTP calls. Property setters can't be `async`. These became fire-and-forget: `_ = RecomputeCommonKeywordsAsync();`.

### KeywordEditorSet Uses Static Async Factory Methods

C# constructors can't be `async`. Building a `KeywordEditorSet` for a document type or a loaded document requires awaiting `RestApi.02`/`RestApi.03` calls. Solution: two static async factory methods instead of constructors:

```csharp
public static async Task<KeywordEditorSet> CreateForDocumentTypeAsync(string docTypeId, HttpClient client)
public static async Task<KeywordEditorSet> CreateForDocumentAsync(string documentId, HttpClient client)
```

`CreateForDocumentAsync` is actually simpler than Unity's `KeywordEditorSet(Document doc)` constructor -- `RestApi.03`'s `GetDocumentInfoAsync` already returns keyword data pre-organized into `KeywordGroups` (each carrying `TypeGroupId`), so no low-level `doc.KeywordRecords` traversal is needed.

### MainWindow Closing Is Cancel-Then-Reconnect

`DisconnectAsync()` is async. A naive port of Unity's synchronous `Closing` handler would let the window close before the disconnect request completed. The fix: cancel the first `Closing` event, await the disconnect, then call `Close()` again programmatically.

```csharp
private async void Window_Closing(object sender, CancelEventArgs e)
{
    if (connection.IsConnected && !isClosingAfterDisconnect)
    {
        e.Cancel = true;
        await connection.Session.DisconnectAsync();
        isClosingAfterDisconnect = true;
        Close();
    }
}
```

---

## SessionManagement Is No Longer Static

`RestApi.01`'s `SessionManagement` was originally static (process-wide shared session). Once `RestApi.TestHarness.Web` needed per-user sessions, it became an instance class. This rippled into this app:

- `ConnectionViewModel` now owns one `SessionManagement` instance (`Session`), constructed once and living for this app's whole lifetime -- same effective lifecycle as before, behavior unchanged for a single-user desktop app.
- Every other view model that previously relied on a now-removed static `GetHttpClient()` fallback now passes `connection.GetHttpClient()` (or `connection.Session.GetFormsHttpClient()` for Unity Form lookups) explicitly to every `RestApi.02`-`04` call.
- `SettingsViewModel` takes a `ConnectionViewModel` dependency, reading/writing `connection.Session.ServiceLocation`/`.IdpSettings` directly.

---

## One Documented Incomplete Item

`DocumentDetailViewModel.EditInArchivingCommand` raises `EditInArchivingRequested`, and `ArchivingViewModel.LoadDocumentForEditing(id)` exists to handle it -- but `MainViewModel` doesn't yet subscribe to that event to perform the actual cross-page navigation. This is a real gap in this build, not a REST API limitation.

---

## Relationship to Unity.TestHarness

| Feature | Unity.TestHarness | RestApi.TestHarness |
|---|---|---|
| Session object | `Application` instance | `SessionManagement` instance wrapping `HttpClient` |
| Commands | `RelayCommand` (sync) | `AsyncRelayCommand` (async) |
| KeywordEditorSet construction | `new KeywordEditorSet(doc)` | `await KeywordEditorSet.CreateForDocumentAsync(id, client)` |
| Window close | Synchronous disconnect | Cancel-then-await-then-close |
| Reconnect to Session ID | Yes | No (confirmed gap) |
| DocPop/UnityPop links | Yes | No (confirmed gap) |
| Purge | Yes | No (confirmed gap) |
| Add Rev/Rend conditional | Yes (pre-flight check) | No (always enabled) |

---

## Takeaways

- Four confirmed feature gaps vs. Unity.TestHarness -- each researched against the OpenAPI spec, not assumed.
- Async propagates from the library layer all the way through view models: async factory methods, fire-and-forget property side effects, cancel-then-close window handling.
- `ConnectionViewModel.ConnectAsync()` is the right entrypoint for "connect if needed," not `ConnectCommand.Execute()` which is fire-and-forget.
- `SessionManagement` is an instance the `ConnectionViewModel` owns. Every other view model gets an `HttpClient` from it explicitly.
- `CreateForDocumentAsync` is simpler than Unity's equivalent because `RestApi.03` already returns pre-organized keyword data.
