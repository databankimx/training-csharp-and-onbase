# Samples.MvcWebApi.WebClient

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

A browser-based test console for `Samples.MvcWebApi`, offering both a JSON POST call (via jQuery `$.ajax()`) and a plain REST-style GET call for each operation, plus a direct link to the API's Swagger documentation.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `MvcWebApiWebClient.html` | The page itself, three operation cards, request/response panes |
| `Scripts/MvcWebApiWebClient.js` | AJAX calls for the JSON operations, direct navigation for the REST ones |
| `Styles/MvcWebApiWebClient.css` | Styling, matching the ASMX/WCF sample web clients' look |
| `Web.config` | Bare-minimum ASP.NET hosting config |

---

## How to Run

1. Run `Samples.MvcWebApi` first (F5 in Visual Studio, IIS Express), and leave it running.
2. Run this project. Try the "JSON" and "REST" buttons on each card, and the Swagger link at the top for full interactive API documentation.

---

## Related Samples

- **`Samples.MvcWebApi`** — the Web API this page calls.
- **`Samples.MvcWebApi.Client`** — a .NET console client calling the same API via raw `HttpWebRequest`.
- **`Samples.MvcWebApi.Common`** — the shared DTOs the API itself uses (this client works from plain JSON directly, no compiled dependency on them).
