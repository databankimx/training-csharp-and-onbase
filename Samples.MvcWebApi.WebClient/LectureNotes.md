# Samples.MvcWebApi.WebClient

## What This Is

A browser-based test console for `Samples.MvcWebApi`, demonstrating both JSON POST (via `$.ajax()`) and plain GET REST-style calls, the same dual-style pattern already seen in `Samples.WcfService.WebClient`.

---

## Porting Notes

Same situation as the ASMX/WCF web clients: the original was an old-style ASP.NET "Web Site" project with no `.csproj`. Converted to a proper Web Application Project. No C# code here, so no packages or `<system.codedom>` configuration needed. jQuery bumped to 3.7.1, `<pre>` used instead of `<textarea>` for the request/response panels (proactively, matching the lesson learned from `Samples.AsmxWebService.WebClient`). Applied the same "service console" visual design as the other two sample web clients.

No bugs found in the JavaScript itself.

---

## The "Default Help" Button Removed

The original page had two documentation buttons: "Default Help" (linking to the classic Web API HelpPage) and "Swagger". `Areas/HelpPage` was dropped entirely from `Samples.MvcWebApi` per an explicit decision made before porting that project (genuinely redundant with Swagger). The "Default Help" button, which pointed at that now-nonexistent `/help` route, was removed here to match; only the Swagger documentation link remains.

---

## Try It Yourself

Run `Samples.MvcWebApi` first (F5, IIS Express), then run this project. Try both buttons on each card, "JSON" populates the Request/Response panes; "REST" opens a plain URL in a new tab. The Swagger link at the top opens the API's own interactive documentation directly.
