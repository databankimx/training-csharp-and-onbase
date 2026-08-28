# Samples.WcfService.WebClient

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

A browser-based test console for `Samples.WcfService`'s REST/JSON endpoint, demonstrating the two ways a WCF `webHttpBinding` endpoint can genuinely be called from a browser:

- **JSON POST**, via `$.ajax()`, for `TestService`/`LookupLocation`, the request body is a JSON object, the response is JSON.
- **Plain GET**, for the `...Rest` variants (`TestServiceRest`/`LookupLocationRest`), the parameters are literally part of the URL path (`/TestServiceRest/{requestId}/{data}`), no JavaScript layer required at all, a link (or a browser address bar) works just as well.

---

## When to Use This Pattern

Whenever a browser needs to call a WCF service directly (rather than through a server-side proxy). The GET-style REST URLs are worth knowing about specifically: they work for simple, idempotent lookups without any client-side scripting, useful for testing, for embedding in a plain `<a href>` link, or for any consumer that can't run JavaScript at all.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `WcfServiceWebClient.html` | The page itself, three operation cards, request/response panes |
| `Scripts/WcfServiceWebClient.js` | AJAX calls for the JSON operations, direct navigation for the REST ones |
| `Styles/WcfServiceWebClient.css` | Styling, matching `Samples.AsmxWebService.WebClient`'s look |
| `Web.config` | Bare-minimum ASP.NET hosting config |

---

## How to Run

1. Run `Samples.WcfService` first (F5 in Visual Studio, IIS Express), and leave it running.
2. Run this project. Try both buttons on the "TestService" and "LookupLocation" cards, the "JSON" button populates the panes below; the "REST" button opens a plain URL in a new tab.

---

## Related Samples

- **`Samples.WcfService`** — the WCF service this page calls.
- **`Samples.WcfService.Client`** — a compiled .NET console client calling the *other* endpoint (`appEndpoint`, SOAP) on the same service.
- **`Samples.AsmxWebService.WebClient`** — the equivalent client for the previous-generation ASMX technology, worth comparing directly.
