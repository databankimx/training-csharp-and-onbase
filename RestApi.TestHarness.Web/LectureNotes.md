# Lecture Notes: RestApi.TestHarness.Web

## Scope: An Identical Application, on a Genuinely Different Framework

Built to reproduce `Unity.TestHarness.Web` - same six pages, same functionality, same look and feel - on ASP.NET Core MVC, not classic ASP.NET MVC 5. These aren't the same framework at different versions: classic ASP.NET MVC 5 runs on `System.Web` (IIS-hosted, `HttpModules`, `HttpContext.Current`), ASP.NET Core is a rewritten, cross-platform framework (Kestrel, a middleware pipeline, built-in DI, no `System.Web` at all). Every infrastructure decision in this project follows from that difference, not from anything REST-API-specific.

---

## The Big One: SessionManagement Had to Stop Being Static

This was the first, largest piece of work, done *before* any page in this app was built: "every user should require their own IdP token, not a shared one" meant `RestApi.01.ConnectingToOnBase`'s `SessionManagement` - originally `static`, one shared session for the whole process - had to become an ordinary instance class. That rippled into `RestApi.02`-`04` too (their own helper classes previously fell back to a static `SessionManagement.GetHttpClient()` when no client was passed; that fallback is gone, every call now requires an explicit `HttpClient`) and into `RestApi.TestHarness` (the WPF app), which had to be updated to hold and pass its own `SessionManagement` instance explicitly throughout. See `RestApi.01`'s own `LectureNotes.md` for the full account - that refactor is a prerequisite for this whole project, not a side effect of building it.

---

## Where the Live SessionManagement Instance Actually Lives

ASP.NET Core's own `ISession` can only store serializable `byte[]` data - it genuinely cannot hold a `SessionManagement` instance directly (a live `HttpClient`/`HttpClientHandler` isn't serializable). This is a real capability gap from classic ASP.NET's `InProc` session mode, which `Unity.TestHarness.Web`'s own `SessionConnectionManager` relied on to store the live `Application` object directly in `Session[key]`.

The fix: `Infrastructure/SessionManagementStore.cs`, a singleton service holding a `ConcurrentDictionary<string, SessionManagement>` keyed by each user's own `HttpContext.Session.Id` (a plain string ASP.NET Core's session middleware does generate and persist via its own cookie, even though the middleware can't store our object). `GetOrCreate`/`TryGet` are two different methods on purpose: `TryGet` never fabricates a new, disconnected session just because something read it (the shared layout's own "Connected"/"Not Connected" indicator uses `TryGet` for exactly this reason), `GetOrCreate` is reserved for the handful of actions that are actually establishing or continuing a real session.

This also killed classic ASP.NET's `Session_End` event as a cleanup hook - ASP.NET Core's session expiration has no comparable notification. `Infrastructure/OrphanedSessionCleanupService.cs` (a `BackgroundService`) replaces it with an active periodic sweep instead of a passive event handler.

Not everything needed this treatment, though: `SessionLogService`'s own `List<LogEntry>` genuinely is plain, serializable data (timestamps, strings, an enum), so it lives directly in `ISession` as JSON - no separate store needed for it. Worth knowing the difference: it's not that "session storage is broken," it's specifically that a live `HttpClient` can't go there, and most page-level state (search results, a loaded document's detail, a keyword editor schema) doesn't have that problem either - `TaxonomyController`, `RetrievalController`, and `ArchivingController` all cache their own page models as JSON in `ISession` the same way.

---

## IOptions<T> vs IOptionsSnapshot<T> vs IOptionsMonitor<T>

Settings' own "Save to Config" writes to `appsettings.json` directly (`IConfiguration` has no write-back API at all, unlike `System.Configuration`'s `ConfigurationManager.Save()`), but the first version of this code used `IOptions<RestApiWebSettings>` everywhere, which is a **fixed snapshot taken once at startup** - a save would never actually take effect for any new session without restarting the whole app. `IOptionsSnapshot<T>` (recomputed per-request) is the usual fix, but it's scoped, and `SessionManagementStore` is a singleton - DI won't let you capture a scoped service in a singleton safely. `IOptionsMonitor<T>` is the one that's actually valid to inject into a singleton *and* reflects the latest file content (via its own `CurrentValue` property, updated once ASP.NET Core's own `appsettings.json` file-watcher notices a change). Every place that reads the shared settings now uses `IOptionsMonitor<T>`.

---

## Every User Authenticates Their Own Way, but Infrastructure Config Stays Shared

`Models/RestApiWebSettings.cs` (bound from `appsettings.json`'s own `RestApi` section) deliberately holds only the *shared* infrastructure - server/IdP URLs, IdP client credentials - never a username or password. Connect's own form collects those directly from the user, and `SessionManagementStore.BuildSessionManagement()` combines the two: shared config for "where," per-user credentials for "who." Settings' own `Apply` therefore only ever affects the *current* session (it edits `store.GetOrCreate(...)`'s own `ServiceLocation`), a genuine behavioral difference from `Unity.TestHarness.Web`'s own truly-global `Apply` (backed by a static `SessionManagement.ServiceLocation`, so one admin's change was immediately visible to every connected user) - not a design choice, just what's left once the session itself is per-user.

---

## File Uploads: IFormFile, Not HttpPostedFileBase

ASP.NET Core's `IFormFile` replaces classic ASP.NET's `HttpPostedFileBase`. `ArchivingController.SaveUploadedFilesAsync` copies each upload to a temp file via `CopyToAsync` (genuinely async, unlike `HttpPostedFileBase.SaveAs`'s synchronous write) before handing the resulting local paths to `RestApi.04`'s own `NewDocumentRequest`/`UpdateDocumentRequest.Files` - that request shape still just takes local paths either way, the REST API's own three-step upload-staging process (initiate, upload parts, reference the resulting ids) happens entirely inside `DocumentStorage`, never leaking upload mechanics to this controller.

---

## Not Yet Wired: Nothing, But Worth Knowing

Unlike `RestApi.TestHarness` (which has one honestly-documented "Edit in Archiving" navigation gap), every page in this app is fully wired end to end. The confirmed feature gaps carried through from the REST API itself (no Session ID reconnect, no DocPop/UnityPop, no Purge, Add Revision/Add Rendition always enabled) are documented per-controller/view where they apply, consistent with every other project in this training track.
