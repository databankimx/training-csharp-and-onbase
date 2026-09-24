# RestApi.TestHarness.Web

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

The ASP.NET Core MVC counterpart to `Unity.TestHarness.Web` — the same web app, same six pages (Connect, Taxonomy, Retrieval, Archiving, Settings, Help), same look and feel, just talking to the OnBase Document Management (REST) API via `RestApi.00`–`04` instead of the Unity API.

`net10.0`, SDK-style ASP.NET Core MVC — a genuinely different framework from classic ASP.NET MVC 5, not just a newer version. Every user gets their own IdP token and OnBase session, not a shared one across the application (a confirmed, explicit requirement that reshaped `RestApi.01`'s `SessionManagement` from a static class into a per-user instance — see `RestApi.01`'s own `LectureNotes.md`).

---

## What's Built

| Page | Status |
|---|---|
| **Connect** | Done — collects your own username/password, obtains an IdP token, establishes your own OnBase session. No Session ID reconnect, no Test API Availability ping (confirmed gaps). |
| **Taxonomy** | Done — cascading Document Type Group → Document Type → Keyword Group Type browser, plus Custom Query/File Type/Unity Form Template lookups. One fewer AJAX round trip than `Unity.TestHarness.Web`: `KeywordGroupItem` carries its own resolved `KeywordTypes` directly. |
| **Retrieval** | Done — three search modes, results list, full detail pane (metadata, keyword groups, Revision/Rendition browsing, file download). No DocPop/UnityPop links (confirmed gap). |
| **Archiving** | Done — Store New, Modify Metadata, Add Revision, Add Rendition (both always offered once a document is loaded — no REST pre-flight check exists), and Delete (no Purge option — confirmed gap). |
| **Settings** | Done — `Apply` affects only your own session (not every user simultaneously, unlike the Unity version — an unavoidable consequence of per-user sessions); `Save to appsettings.json` also persists to disk via direct JSON file I/O (`IConfiguration` is read-only in ASP.NET Core). |
| **Help** | Done — per-page usage instructions, an About section, and a License/Support popup. |

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs`, `appsettings.json` | DI registration, Serilog (standard `Serilog.AspNetCore`, no workaround needed here), session middleware. |
| `Infrastructure/SessionManagementStore.cs` | The core per-user design: one `SessionManagement` instance per browser session, held in a singleton server-memory store keyed by `HttpContext.Session.Id` (`ISession` itself can't hold a live `HttpClient`). |
| `Infrastructure/OrphanedSessionCleanupService.cs` | A `BackgroundService` replacing classic ASP.NET's `Session_End` event (no ASP.NET Core equivalent) with an active periodic sweep. |
| `Infrastructure/SessionLogService.cs`, `Models/LogEntry.cs` | The shared output log, DI-registered and scoped, storing entries as JSON directly in `ISession` (unlike `SessionManagement`, this data genuinely is serializable). |
| `Controllers/`, `Views/` | One controller + view(s) per page, following the same Post-Redirect-Get + AJAX patterns as `Unity.TestHarness.Web`. |
| `wwwroot/` | Static assets (`css/site.css`, `Resources/AppIcon.png`, `Resources/LICENSE`) — ASP.NET Core's static-file convention, unlike classic ASP.NET's top-level `Content`/`Resources` folders. |

---

## Related Samples

- **`Unity.TestHarness.Web`** — the Unity API version this project reproduces.
- **`RestApi.TestHarness`** — the WPF desktop version, single-user, sharing the same underlying `RestApi.00`–`04` library suite.
- **`RestApi.00.CommonFunctionality`** through **`RestApi.04.DocumentArchiving`** — the library suite this project exercises.
