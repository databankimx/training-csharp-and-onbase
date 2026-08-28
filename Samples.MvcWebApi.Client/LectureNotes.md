# Samples.MvcWebApi.Client

## What This Is

A console client consuming `Samples.MvcWebApi` via raw `HttpWebRequest` and `JavaScriptSerializer`, deliberately the pre-`HttpClient`, pre-`async`/`await` style, referencing `Samples.MvcWebApi.Common` for the shared request/response DTOs (since, unlike WCF/ASMX, Web API has no WSDL or generated proxy to consume instead).

---

## Porting Notes

Per an earlier decision, kept as-is rather than modernized to `HttpClient` + `async`/`await`, matching the same "here's what you'll find in legacy code" framing already applied to `Samples.AsmxWebService.Client`. SDK-style console project; `System.Web.Extensions` (for `JavaScriptSerializer`) referenced as a plain Framework assembly, no package needed.

Every `throw new ApplicationException(...)` replaced with `throw new DatabankException(...)`, matching the standard now applied across every `Samples.*` project. `WebApiUrl`'s port was verified against `Samples.MvcWebApi`'s own `.csproj` (`44312`) rather than assumed.

No other bugs found in the original code.

---

## Try It Yourself

Run `Samples.MvcWebApi` first (F5, IIS Express), then run this client. It walks through `Ping`, `Test`, and `LocationLookup`, printing the raw JSON sent and received at each step, worth reading directly to see exactly what a hand-rolled `HttpWebRequest` call looks like on the wire.
