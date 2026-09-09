# Lecture Notes: Unity.TestHarness.Web

This is the ASP.NET MVC reproduction of `Unity.TestHarness` (the WPF version). Most of the actual Unity API usage is identical, that library-facing code isn't repeated here, see `Unity.TestHarness`'s own `LectureNotes.md` for that. This file covers what's genuinely different about being a **web** application, and the mistakes made (and fixed) getting there.

---

## Session State Holds the Live Connection, and That's a Genuine Constraint

WPF keeps one `Application` object alive in memory for the whole process's lifetime. A web app has no such thing, every request is independent. The fix here is `Session` (`InProc` mode specifically, see `Web.config`), holding the live `Application` per browser session.

`InProc` isn't just the default, it's the only built-in ASP.NET session mode that can hold a **non-serializable object reference** at all. `StateServer`/`SQLServer` modes both require session content to serialize, and Unity API's own connection objects were never designed for that. The real tradeoffs: the connection doesn't survive an app pool recycle, and this ties the app to a single server without further work. The documented alternative, not implemented in this first pass, is reconnecting by Session ID on every request instead of holding the `Application` object itself (`SessionManagement.Connect(string)`, the same capability the WPF version's own "Reconnect to Session ID" button exercises).

---

## A Closed Browser Tab Sends the Server Nothing

Unlike WPF's `MainWindow.Closing`, which fires reliably, HTTP gives a web server no signal at all when a tab closes. The best available fallback is `Global.asax`'s `Session_End`, which only fires once the session's own inactivity timeout elapses (60 minutes here), and only under `InProc` mode. `SessionConnectionManager.DisconnectOrphanedSession(HttpSessionState)` handles this, taking the session explicitly rather than reading `HttpContext.Current`, which is unreliable specifically inside `Session_End`, a well-known ASP.NET gotcha.

The Close button (in the sidebar, every page) is the *deliberate* version of this: it disconnects properly via a real POST, then attempts `window.close()`. That attempt is not guaranteed to work: browsers only allow a script to close a tab it opened itself, and block it for an ordinarily-navigated tab as a security measure, with no client-side workaround. The disconnect always happens either way; the "you may now close this tab" fallback text covers the case where the auto-close is blocked.

---

## Serilog Configuration: Abandoned Microsoft.Extensions.Configuration Mid-Build

The first attempt mirrored the WPF version exactly: `Serilog.Settings.Configuration` + `Microsoft.Extensions.Configuration`/`.Json`, reading `serilog.json` via `ReadFrom.Configuration(...)`. This pulled in a deep transitive dependency chain (`Microsoft.Extensions.Configuration.Abstractions`, `.FileExtensions`, `Microsoft.Extensions.FileProviders.*`, `Microsoft.Extensions.Primitives`, `System.Memory`, ...) that triggered a *cascading* series of `FileLoadException`s, one assembly at a time, each fix revealing the next. `AutoGenerateBindingRedirects`, which works reliably for the WPF EXE, was not reliably generating/refreshing entries for this whole chain under a legacy Web Application project.

The fix: `Global.asax.cs` now parses `serilog.json` directly with `Newtonsoft.Json` (already a dependency, far shallower footprint) and builds `LoggerConfiguration` programmatically from the *parsed values*. Settings are still 100% externally configurable, still no recompile needed to change them, only the loading mechanism changed. `Web.config` still carries a couple of hand-written `<bindingRedirect>` entries (`Serilog`, `System.Memory`) as safety nets from this saga; if you see a similar `FileLoadException` for a different assembly, get the *actual* assembly version on disk via `[System.Reflection.AssemblyName]::GetAssemblyName("path\to\the.dll").Version` in PowerShell before adding a redirect, don't guess from the error message's own "requested version", that's what's being asked for, not what's present.

---

## The Razor Nesting Rule (Learned the Hard Way, Repeatedly)

`@if`/`@foreach`/`@using` need the `@` prefix only when transitioning **from markup into code**. The rule that actually holds up: `@` is needed only when a markup tag is **currently, actively open** at that point in the file, tracked across the *whole* nesting, not reset per `{ }` block. A control structure that's the first thing after `else {` needs no `@` if nothing before it (within the whole open-tag chain) is still an open tag; but if a `<div>` that's still open wraps it, it does need `@`.

This bit us more than once: `Html.BeginForm` calls needing `@using` vs. plain `using` depending on what wrapped them, and `@if` checks after a fully-closed `<div class="th-card">...</div>` needing no `@` even though they're visually indented like they're "inside" something. When restructuring a view's layout (e.g., wrapping content in a new outer `<div>` for a multi-column layout), re-check every nested control structure inside it, the correct prefix can flip in either direction depending on what's now open around it.

---

## `Html.Partial` with a `null` Model Doesn't Actually Render `null`

Passing `null` as a partial's model doesn't render the partial with a null model, it falls back to reusing the **calling page's own** `ViewData.Model`. If the partial's `@model` type doesn't match the parent's, this throws `InvalidOperationException: The model item passed into the dictionary is of type 'X', but this dictionary requires a model item of type 'Y'`.

Fix: never let a property that might get passed to `Html.Partial` actually be `null`. `RetrievalPageModel.Detail` and `ArchivingPageModel.NewEditorSchema`/`ExistingEditorSchema` all default to a real, empty instance instead. The partial's own "nothing loaded yet" check (`Model.Metadata == null`, in `_DetailPane.cshtml`'s case) still works fine against an empty-but-non-null instance.

---

## Boolean HTML Attributes Are Presence-Based, Not Value-Based

`disabled="@(someBool)"` or `checked="@Model.KeepAlive"` render literally as `disabled="False"`/`checked="False"` when the value is false, and a browser still treats those as present (HTML boolean attributes care whether the attribute exists at all, not what its value string says). The fix used throughout: `@(condition ? "checked" : "")` as a **bare token** in the tag (not inside a quoted attribute value), which Razor renders as the attribute name alone when non-empty, or omits entirely when empty.

---

## File Uploads: No Direct Filesystem Access, Unlike WPF

WPF's `NewDocumentRequest.Files`/`UpdateDocumentRequest.Files` just take local paths a file dialog already provided. A web request instead receives `HttpPostedFileBase` uploads (streams), and those library methods still expect local paths, not streams. `ArchivingController.SaveUploadedFiles` writes each upload to a temp directory first, and `CleanUpTempFiles` removes them afterward regardless of success or failure (`finally` block).

---

## The Dynamic Keyword Editor: Flat Form Fields, Not Model Binding

Archiving's Add/Remove Instance/Value UI (mirroring `KeywordGroupEditor`/`StandaloneKeywordEditor` in the WPF version) can't use strongly-typed model binding, the shape (how many instances, how many values) is only known at submit time, generated by client-side JS cloning `<template>` elements. Submitted fields use a naming convention instead: `kw_group_{groupId}_{instanceKey}_{fieldId}` and `kw_standalone_{keywordId}_{valueKey}`. `ArchivingController.ParseKeywordsFromForm` reconstructs the `KeywordGroup`/`KeywordInfo` lists the library expects by parsing `Request.Form`'s keys directly. Existing values (loaded for Modify Metadata) get stable `existingN` keys; JS-added rows get `newN` keys (an incrementing counter, starting well past any realistic existing count), so there's never a collision between server-rendered and client-added rows.

---

## CSS Load Order: Bootstrap After site.css, Not Before

Every control in this app uses its own `th-*` classes, nothing here actually depends on Bootstrap's own styling. Loading Bootstrap's CDN stylesheet *after* `site.css` meant its default element-level rules (plain, unclassed `<button>`) could override lower-specificity custom rules purely because it loaded later, exactly what happened with Archiving's mode-selector buttons rendering with default browser chrome instead of the intended tab-link look. Fixed by loading `site.css` last in `_Layout.cshtml`'s `<head>`.

---

## Sidebar Height: `height: 100vh`, Not `min-height`

`.th-shell`'s original `min-height: 100vh` is a floor, not a ceiling, if the sidebar's own content (nav + status + Close button) exceeded the viewport height, the whole shell grew taller, pushing Close below the fold on shorter screens. Fixed to a locked `height: 100vh`, with `overflow-y: auto` added to `.th-sidebar` so its own content scrolls internally instead of expanding the page, matching `.th-output`'s existing (correct) pattern.

---

## Settings Is a Global, Admin-Style Page &mdash; Deliberately

`SessionManagement.ServiceLocation`/`IdpSettings` are **static, process-wide** properties, not per-session state, unlike everything else this app holds in `Session`. That means Settings changes here affect every user of the app simultaneously. Confirmed as acceptable and intentional for this harness (an internal, admin-configured tool), not an oversight to fix later.

---

## Retrieval's Search Mode Has to Be Persisted, Not Just Toggled Client-Side

Mode switching itself (Document Type(s)/Custom Query/Document ID) is client-side JS, no server round-trip. But after a `Search` POST-then-redirect, the page has no memory of which mode was actually used, JS re-initializes fresh and defaults back to the first tab regardless of what was searched with. Fixed by adding `RetrievalPageModel.LastSearchMode`, set from the `mode` form field in `Search()`, and the view uses it (not a hardcoded default) to pick which tab/panel renders as active on load.
