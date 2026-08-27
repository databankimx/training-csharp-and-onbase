# Samples.AsmxWebService

> **Looking for implementation details, bugs found, or migration notes?** See `LectureNotes.md` in this folder. This README is the front door: what this is, when to use it, and how to run it.

## What This Is

ASMX (`.asmx`) is the original .NET web service technology, part of ASP.NET since .NET 1.0 (2002). It exposes public methods on a class as SOAP web service operations using the `[WebMethod]` attribute, and ASP.NET handles the rest, request routing, SOAP envelope parsing, response serialization, and even a free, auto-generated browsable test page for every method.

This sample is a real, runnable ASMX service (`ExampleWebService.asmx`) with three operations:

- **`Ping()`** — confirms the service is online.
- **`TestService(request)`** — confirms the service can receive and echo back structured data.
- **`LookupLocation(request)`** — looks up city/county/state by ZIP code against a real, switchable database backend (SQL Server, Oracle, MySQL, or ODBC, see `HelperClasses/Database.cs`).

It's paired with two client samples in this same solution folder: `Samples.AsmxWebService.Client` (a console app consuming it via a generated proxy class) and `Samples.AsmxWebService.WebClient` (a plain HTML/JavaScript page calling it directly via AJAX).

---

## When to Use ASMX

**Never, for new development.** Every project started today should use ASP.NET Core Web API (or, if genuinely constrained to classic ASP.NET, at minimum WCF, see `Samples.WcfService`). ASMX is included in this training set specifically because you *will* encounter it in real support and maintenance work, older internal tools, vendor integrations, and systems that predate WCF (which shipped in 2006) are still running ASMX services in production, and knowing how to read, debug, and cautiously extend one is a genuinely useful skill even though you'd never choose it fresh.

---

## Pros

- **Extremely simple to implement.** A method with `[WebMethod]` on it is a web service operation, no separate contract interface, no host configuration, no endpoint bindings to reason about.
- **Free tooling.** ASP.NET auto-generates a browsable test page for every method (visit `ExampleWebService.asmx` directly) and a WSDL document (`?wsdl`), which most SOAP client generators (including .NET's own `wsdl.exe`) can consume directly.
- **Broad legacy interoperability.** Because it's plain SOAP 1.1, extremely old clients and platforms that predate REST/JSON tooling can often still consume an ASMX service without issue.

## Cons

- **SOAP-only.** No REST URI patterns, no content negotiation, nothing resembling a modern HTTP API surface.
- **Verbose payloads.** Every request and response is a full SOAP XML envelope, even for a one-line `Ping()`.
- **No modern hosting or DI story.** ASMX predates dependency injection containers as a mainstream .NET pattern; this sample's `static` settings/logger fields (initialized in a static constructor) are exactly the kind of workaround that was standard practice at the time, and exactly what modern frameworks solve properly.
- **Long since superseded.** WCF (2006) added contract-first design, multiple binding/transport options, and REST support. Web API (2012) added natively RESTful, JSON-first services. ASP.NET Core (2016+) rebuilt the whole stack around DI, middleware, and cross-platform hosting. ASMX has had no meaningful investment from Microsoft in over a decade.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `ExampleWebService.asmx` / `.asmx.cs` | The service itself, three `[WebMethod]` operations |
| `Models/Objects/` | Request/response contracts (`ServiceTestRequest`, `LocationLookupResponse`, etc.) |
| `Models/Configuration/` | Strongly-typed `Web.config` sections (`ServiceSettings`, `DatabaseSettings`) |
| `Models/Enumerations/` | `DbArchitecture` (which database backend is active) |
| `HelperClasses/Database.cs` | The actual multi-backend database query logic |
| `HelperClasses/ErrorHandling.cs` | Centralized exception-to-error-list handling, logged via Serilog |
| `HelperClasses/Stamp.cs` | Small date/time formatting helpers |
| `serilog.json` | Log sink (file path, output format) configuration, see `LectureNotes.md` |
| `Web.config` | Service settings, database connection settings, binding redirects |

---

## How to Run

1. Open `Web.config` and point the `<database>` element at a real SQL Server instance (or Oracle/MySQL/ODBC, matching `architecture`) with a `ZipCodes` table (`State`, `County`, `City`, `ZipCode` columns).
2. Press F5 in Visual Studio (runs under IIS Express).
3. Browse to `ExampleWebService.asmx` directly to see the auto-generated test page, click any method name to try it interactively, no client code needed.

---

## Related Samples

- **`Samples.AsmxWebService.Client`** — console client consuming this service via a generated proxy class.
- **`Samples.AsmxWebService.WebClient`** — plain HTML/JavaScript page calling this service directly via AJAX.
- **`Samples.WcfService`** — the technology that succeeded ASMX, worth comparing directly.
