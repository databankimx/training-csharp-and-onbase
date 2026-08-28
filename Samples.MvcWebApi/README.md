# Samples.MvcWebApi

> **Looking for implementation details, bugs found, or migration notes?** See `LectureNotes.md` in this folder.

## What This Is

ASP.NET Web API 2, the third generation of .NET web service technology (2012), succeeding WCF. Unlike ASMX (SOAP-only) or WCF (contract-first, multi-binding), Web API is natively RESTful and JSON-first: a controller method is an HTTP endpoint, full stop, no separate contract file, no binding configuration to reconcile.

This sample exposes three operations (`Ping`, `Test`, `LocationLookup`, the last backed by a real EF6 Database-First model against a SQL Server database) alongside a small MVC front end (a home page, and Swagger/Swashbuckle for interactive API documentation), and is paired with `Samples.MvcWebApi.Client` (a .NET console client) and `Samples.MvcWebApi.WebClient` (a browser client), both consuming the API through `Samples.MvcWebApi.Common`, a shared DTO library.

---

## When to Use Web API 2

Only for existing classic ASP.NET applications, or genuine constraints that rule out ASP.NET Core. For any new project, ASP.NET Core Web API is the direct, actively-developed successor, same RESTful philosophy, cross-platform, built around dependency injection and middleware from the ground up.

---

## Pros

- **Genuinely RESTful.** URL templates map cleanly to resources and HTTP verbs, no SOAP envelope, no WSDL indirection.
- **Native Entity Framework integration.** A controller action can query a mapped database in a single LINQ statement (see `LocationLookupController.Post()`).
- **JSON-first, browser-friendly.** Every response is JSON by default, consumable directly from any HTTP client, no proxy generation required.
- **Filters replace boilerplate try/catch.** `[LogFilter]`/`[ExceptionFilter]` (see `Filters/`) centralize logging and error handling across every controller, worth comparing directly against the commented-out "what we'd do without filters" blocks left in each controller for exactly that comparison.

## Cons

- **No contract, no WSDL.** Nothing describes a Web API's request/response shapes the way a WCF/ASMX WSDL does. `Samples.MvcWebApi.Common` exists specifically to paper over this, a shared library both the server and every .NET client reference, so the shapes can't silently drift apart, but it's a convention, not something the framework enforces.
- **Superseded by ASP.NET Core.** No further platform investment; ASP.NET Core Web API/minimal APIs are where new work happens now.
- **Tied to classic .NET Framework**, `System.Web`, IIS, no cross-platform story at all.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Controllers/` | `PingController`, `TestController`, `LocationLookupController` (the API), `HomeController` (the landing page) |
| `Filters/` | `LogFilter`, `ExceptionFilter`, applied declaratively to every API controller |
| `App_Start/SwaggerConfig.cs` | Swagger/Swashbuckle setup, the *only* interactive API doc UI kept, see `LectureNotes.md` |
| `LocationLookupModel.edmx` (+ generated files) | EF6 Database-First model, one entity (`ZipCode`) |
| `Samples.MvcWebApi.Common` (separate project) | Shared request/response DTOs |
| `serilog.json` | Log sink configuration |
| `Web.config` | Database connection, bindings, binding redirects |

---

## How to Run

1. Point `Web.config`'s `LocationLookupDatabase` connection string at a real SQL Server instance with a `ZipCodes` table.
2. Press F5 (IIS Express).
3. Browse to `/swagger` for interactive API documentation, or the home page for a quick overview.

---

## Related Samples

- **`Samples.MvcWebApi.Common`** — the shared DTOs every project in this group depends on.
- **`Samples.MvcWebApi.Client`** — a .NET console client.
- **`Samples.MvcWebApi.WebClient`** — a browser-based client.
- **`Samples.WcfService`** — the technology Web API succeeded, worth comparing the WSDL-based contract approach against this project's shared-library convention.
