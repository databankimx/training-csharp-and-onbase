# Samples.MvcWebApi.Core.WebClient

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

A browser-based test console for `Samples.MvcWebApi.Core`, using the browser's native `fetch()` with `async`/`await`, no jQuery, no library at all. Hosted by a genuinely minimal ASP.NET Core app (`Program.cs` is three lines: `UseDefaultFiles()`, `UseStaticFiles()`, `Run()`), a real contrast against the classic web clients, which each needed a full legacy Web Application Project just to serve static files.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Minimal static-file hosting, three lines |
| `wwwroot/index.html` | The page itself, three operation cards, request/response panes |
| `wwwroot/js/site.js` | `fetch()`-based calls to the API |
| `wwwroot/css/site.css` | Styling, matching every other sample web client's look |

---

## How to Run

1. Run `Samples.MvcWebApi.Core` first (F5 or `dotnet run`), and leave it running.
2. Run this project. Try each operation card, and the Swagger link at the top.

---

## Related Samples

- **`Samples.MvcWebApi.Core`** — the API this page calls.
- **`Samples.MvcWebApi.Core.Client`** — a .NET console client for the same API, using `HttpClient`.
- **`Samples.MvcWebApi.WebClient`** — the classic sibling, using jQuery `$.ajax()`, worth comparing directly.
