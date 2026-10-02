# RestApi.TestHarness.Web

## What This Is

The ASP.NET Core MVC counterpart to `Unity.TestHarness.Web` -- the same web app, same six pages, same look and feel, just talking to the OnBase Document Management (REST) API via `RestApi.00`-`04` instead of the Unity API.

`net10.0`, SDK-style ASP.NET Core MVC. This is a genuinely different framework from classic ASP.NET MVC 5 (`Unity.TestHarness.Web`'s foundation) -- not a newer version of the same framework. Classic ASP.NET runs on `System.Web` (IIS, `HttpModules`, `HttpContext.Current`). ASP.NET Core is a rewritten, cross-platform framework (Kestrel, middleware pipeline, built-in DI, no `System.Web`). Every infrastructure decision here follows from that difference.

---

## What's Built

| Page | Status |
|---|---|
| **Connect** | Done -- per-user IdP token, per-user OnBase session |
| **Taxonomy** | Done -- cascading Document Type Group -> Document Type -> Keyword Group Type browser, Custom Query/File Type/Unity Form Template lookups |
| **Retrieval** | Done -- three search modes, results list, full detail pane (metadata, keyword groups, Revision/Rendition browsing, file download) |
| **Archiving** | Done -- Store New, Modify Metadata, Add Revision, Add Rendition, Delete |
| **Settings** | Done -- Apply (in-memory, current user's session only) vs Save to appsettings.json (persists) |
| **Help** | Done -- per-page usage instructions, About section |

---

## The Foundation: SessionManagement Had to Stop Being Static

This was done before any page was built. "Every user requires their own IdP token, not a shared one" meant `RestApi.01.ConnectingToOnBase`'s `SessionManagement` -- originally a static class with a single shared connection -- had to become an instance class. That rippled into:

- `RestApi.02`-`04`: the static `SessionManagement.GetHttpClient()` fallback was removed; every call now requires an explicit `HttpClient`.
- `RestApi.TestHarness` (the WPF app): had to be updated to hold and pass its own `SessionManagement` instance explicitly.

See `RestApi.01.ConnectingToOnBase`'s Lesson for the full account. This refactor is a prerequisite for this project.

---

## Where the Live SessionManagement Instance Lives

ASP.NET Core's `ISession` stores `byte[]` -- it cannot hold a live `HttpClient`/`HttpClientHandler`. This is a real capability gap from classic ASP.NET's `InProc` session mode, which `Unity.TestHarness.Web`'s `SessionConnectionManager` relied on.

**`Infrastructure/SessionManagementStore.cs`** is the solution: a singleton service holding a `ConcurrentDictionary<string, SessionManagement>`, keyed by each user's `HttpContext.Session.Id` (a plain string that ASP.NET Core's session middleware does generate and persist via its own cookie).

Two distinct access methods:

```csharp
// Used only when actually establishing or continuing a real connection
SessionManagement GetOrCreate(string sessionId, ServiceLocation sl, IdpSettings idp);

// Used for read-only checks (e.g., the "Connected/Not Connected" sidebar indicator)
SessionManagement TryGet(string sessionId);
```

`TryGet` never fabricates a new disconnected session just because something read it. That distinction matters: the shared layout reads connection status on every page load.

Not everything needs this treatment. `SessionLogService`'s `List<LogEntry>` is plain serializable data -- it lives directly in `ISession` as JSON. Page-level state (search results, a loaded document's detail, a keyword editor schema) also caches as JSON in `ISession`. It's specifically the live `HttpClient` that can't go there.

---

## OrphanedSessionCleanupService

Classic ASP.NET's `Session_End` event fired when a session expired. ASP.NET Core has no equivalent notification.

**`Infrastructure/OrphanedSessionCleanupService.cs`** replaces it with an active periodic sweep (`BackgroundService`) that checks `SessionManagementStore` for sessions whose `HttpContext.Session` has expired (by checking whether ASP.NET Core's session middleware still knows about that session ID) and disconnects them.

---

## IOptionsMonitor\<T\>, Not IOptions\<T\>

Settings' "Save to appsettings.json" writes directly to the file (`IConfiguration` has no write-back API in ASP.NET Core). But `IOptions<T>` is a fixed snapshot taken once at startup -- a save would never take effect without restarting the app.

`IOptionsSnapshot<T>` is recomputed per-request but is scoped, and `SessionManagementStore` is a singleton -- DI won't allow a scoped service injected into a singleton.

`IOptionsMonitor<T>` is the correct choice: valid to inject into a singleton, and its `CurrentValue` property reflects the latest file content once ASP.NET Core's own `appsettings.json` file-watcher picks up the change.

---

## Shared vs. Per-User Settings

`Models/RestApiWebSettings.cs` (bound from `appsettings.json`'s `RestApi` section) holds only shared infrastructure: server/IdP URLs, IdP client credentials. It never holds a username or password.

Connect's form collects credentials directly from the user. `SessionManagementStore.BuildSessionManagement()` combines the two: shared config for "where," per-user credentials for "who."

Settings' `Apply` therefore only ever affects the current user's session. This is a genuine behavioral difference from `Unity.TestHarness.Web`'s `Apply`, which was process-wide (backed by a static `SessionManagement.ServiceLocation`). Not a design choice -- an unavoidable consequence of per-user sessions.

---

## File Uploads: IFormFile, Not HttpPostedFileBase

ASP.NET Core's `IFormFile` replaces classic ASP.NET's `HttpPostedFileBase`:

```csharp
// Classic ASP.NET
HttpPostedFileBase upload = Request.Files[0];
upload.SaveAs(tempPath);

// ASP.NET Core
IFormFile upload = Request.Form.Files[0];
await upload.CopyToAsync(fileStream);
```

`ArchivingController.SaveUploadedFilesAsync` copies each upload to a temp file via `CopyToAsync` (genuinely async) before handing local paths to `RestApi.04`'s `NewDocumentRequest`/`UpdateDocumentRequest.Files`. The REST API's own three-step upload-staging process (initiate, upload parts, reference IDs) happens entirely inside `DocumentStorage` -- the controller never sees those mechanics.

---

## Static Assets: wwwroot/, Not Content/

Classic ASP.NET served static files from top-level `Content`/`Resources` folders. ASP.NET Core uses `wwwroot/` as the static-file root. `css/site.css`, `Resources/AppIcon.png`, and `Resources/LICENSE` all live there.

---

## Serilog: Standard, No Workaround

`Unity.TestHarness.Web` (classic ASP.NET) needed a workaround to avoid `Microsoft.Extensions.Configuration`'s dependency cascade (`FileLoadException` chain, hand-written `<bindingRedirect>` entries, JSON parsed via Newtonsoft directly). ASP.NET Core's first-class `Microsoft.Extensions.Configuration` support means `Serilog.AspNetCore` works normally here with no workaround at all.

---

## Confirmed Feature Gaps (Same as RestApi.TestHarness)

1. No "Reconnect to Session ID"
2. No DocPop/UnityPop links
3. No Purge/permanent-delete
4. Add Revision/Add Rendition always enabled (no pre-flight check)

All four are documented per-controller/view where they apply.

---

## Takeaways

- `ISession` can't hold a live `HttpClient`. `SessionManagementStore` bridges this with a server-memory `ConcurrentDictionary` keyed by session ID.
- `TryGet` vs `GetOrCreate` are intentionally different: read-only status checks should never fabricate a new session.
- `OrphanedSessionCleanupService` replaces `Session_End` with an active periodic sweep.
- `IOptionsMonitor<T>` is the right options type for a singleton that needs to see config file changes after startup.
- `Apply` in Settings only affects the current user's session -- there's no process-wide session to affect.
- `IFormFile.CopyToAsync` is the ASP.NET Core equivalent of `HttpPostedFileBase.SaveAs`.
- `wwwroot/` is the static-file root -- not `Content/` or `Resources/`.
- Serilog works normally here with `Serilog.AspNetCore`. The `FileLoadException` workaround in `Unity.TestHarness.Web` is a classic-ASP.NET-specific problem.
