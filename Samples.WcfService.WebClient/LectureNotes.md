# Samples.WcfService.WebClient

## What This Is

A plain HTML/CSS/JavaScript page calling `Samples.WcfService`'s `webEndpoint` (`webHttpBinding`) directly from the browser, demonstrating both ways WCF's REST support can be consumed: JSON POST via `$.ajax()` (`TestService`/`LookupLocation`) and a plain GET URL with path parameters (`TestServiceRest`/`LookupLocationRest`, opened directly in a new tab).

---

## Porting Notes

Same situation as `Samples.AsmxWebService.WebClient`: the original was an old-style ASP.NET "Web Site" project with no `.csproj` at all. Converted to a proper Web Application Project, matching the established pattern. No C# code to compile here (pure static content), so no packages or `<system.codedom>` compiler configuration needed, both were already dead configuration in the original.

**No bugs found in the JavaScript this time.** Unlike `Samples.AsmxWebService.WebClient`'s `.http()` typo, this file's error handler correctly calls `.html()` throughout. Worth noting the ASMX WebClient's response also needed `result.d` unwrapping (an ASMX-specific JSON-hijacking mitigation), this one doesn't, WCF's `webHttpBinding` doesn't apply that convention, the raw result is used directly.

Applied the same visual redesign as `Samples.AsmxWebService.WebClient` (dark graphite "service console," monospace type, amber accent) for consistency across both sample web clients, and proactively used `<pre>` instead of `<textarea>` for the request/response panels from the start this time, avoiding the class of rendering bug found (and fixed) in the ASMX version, rather than discovering it here too.

---

## The Endpoint Address Was Wrong

`wsUrl` originally pointed at `https://localhost:44357/ExampleWebService.svc/Web/`, the same incorrect assumption made in `Samples.WcfService.Client` (see that project's own `LectureNotes.md` for the full explanation). A real WSDL pulled from the running service confirmed the SOAP `appEndpoint` actually lives at `http://localhost:39417`, plain HTTP, not the HTTPS/44357 address assumed from the `.csproj`'s `<IISUrl>` setting. Since IIS Express serves the entire site, every endpoint, on the same host and port, only the path differs between the SOAP and REST endpoints, this `webEndpoint`'s real address is `http://localhost:39417/ExampleWebService.svc/Web/` too. **Fixed** both `wsUrl` in the JavaScript and the target address shown in the page header to match.

---

## Try It Yourself

Run `Samples.WcfService` first (F5, IIS Express), then run this project. The "JSON" buttons populate the Request/Response panes on this page; the "REST" buttons open the GET-style URL directly in a new browser tab, since that's genuinely how a REST URL like this is meant to be used, no AJAX layer required at all.
