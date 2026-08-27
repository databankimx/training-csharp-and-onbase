# Samples.AsmxWebService.WebClient

> **Looking for implementation details, bugs found, or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

A plain HTML page demonstrating the *other* way to consume an ASMX web service: directly from browser JavaScript, using jQuery's `$.ajax()` to POST JSON to the service and read the JSON response back. No compiled proxy class, no build step, just three buttons calling `Ping()`, `TestService()`, and `LookupLocation()` and displaying the raw request/response JSON on the page.

---

## When to Use This Pattern

The same "maintenance only" framing as ASMX itself, see `Samples.AsmxWebService`'s own `README.md` for the fuller discussion. Worth knowing specifically: ASMX *can* be made to accept and return JSON (via `[ScriptMethod]`, already applied to every method on the service side), which is exactly what makes this direct-AJAX consumption style possible at all, but the service is still fundamentally SOAP-based underneath. A genuinely modern REST API doesn't need this workaround.

One quirk worth knowing if you ever debug something like this: ASMX wraps every JSON response in an object with a single property named `d` (`{ "d": <actual result> }`), specifically as an older security mitigation against a JSON-hijacking attack vector. This client accounts for that directly (`result.d` in the success handler), a detail easy to trip over the first time you see it.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `AsmxWebServiceWebClient.html` | The page itself, three buttons, two textareas showing request/response JSON |
| `Scripts/AsmxWebServiceWebClient.js` | All the client-side logic, validation, AJAX calls, logging |
| `Styles/AsmxWebServiceWebClient.css` | Minimal styling |
| `Web.config` | Bare-minimum ASP.NET hosting config, no C# code here to configure a compiler for |

---

## How to Run

1. Run `Samples.AsmxWebService` first (F5 in Visual Studio, IIS Express), and leave it running.
2. Run this project. Click "Ping Service", "Test Service", or "Lookup Location" and watch the Request/Response panes populate with the actual JSON sent and received.

---

## Related Samples

- **`Samples.AsmxWebService`** — the ASMX service this page calls.
- **`Samples.AsmxWebService.Client`** — a compiled .NET console client consuming the same service via a generated proxy class, a very different (and, for real application code, generally preferable) consumption style worth comparing against this one.
