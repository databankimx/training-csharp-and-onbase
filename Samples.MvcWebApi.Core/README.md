# Samples.MvcWebApi.Core

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

ASP.NET Core Web API, the current, actively-developed successor to classic Web API 2. This is a genuinely separate, modern implementation of `Samples.MvcWebApi`'s three operations (`Ping`, `Test`, `LocationLookup`), not a port, so much of the plumbing changed structurally, not just the syntax. See `LectureNotes.md` for every deliberate divergence.

---

## When to Use ASP.NET Core Web API

For essentially all new REST API work. Cross-platform (Windows, Linux, macOS, containers), built around dependency injection and middleware from the ground up, actively developed by Microsoft, and the direct target of virtually all new tooling, documentation, and community effort in the .NET ecosystem.

---

## Pros (Over Classic Web API 2)

- **Cross-platform.** Runs anywhere .NET runs, not tied to Windows/IIS/`System.Web`.
- **Built-in dependency injection.** `LocationLookupContext` is registered once in `Program.cs` and injected wherever needed, no manual construction.
- **Genuinely async all the way down.** `LocationLookupController`'s query uses EF Core's `ToListAsync()`, which doesn't block a thread waiting on the database round-trip the way EF6's synchronous LINQ-to-Entities calls do.
- **Standard, structured error responses.** `ProblemDetails` (RFC 7807) via a single centralized `IExceptionHandler`, real HTTP status codes instead of always returning `200 OK` with an `Errors` array to check.
- **Configuration and logging integration is built in.** `appsettings.json` is loaded automatically; Serilog reads its own section from that same file with one line of setup.

## What's Different From `Samples.MvcWebApi`

- **EF Core Code-First**, not EF6 Database-First, `Models/ZipCode.cs` is the source of truth, not a reverse-engineered `.edmx`.
- **`System.Text.Json`**, ASP.NET Core's default, not Newtonsoft.Json.
- **No `DatabankException`.** `CSharp.SharedLibrary` targets net48, which a net10.0 project cannot reference at all (one-directional compatibility). ASP.NET Core's own `IExceptionHandler` + `ProblemDetails` is the genuine modern equivalent, not a workaround.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Controllers/` | `PingController`, `TestController`, `LocationLookupController` |
| `Models/ZipCode.cs` | EF Core entity (Code-First) |
| `Models/Dtos.cs` | Request/response records |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |
| `GlobalExceptionHandler.cs` | Centralized exception handling → `ProblemDetails` |
| `appsettings.json` | Connection string, Serilog configuration |

---

## How to Run

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Run `dotnet ef database update` (or let EF Core create the table on first use) against the same `ZipCodes` table structure `Samples.MvcWebApi` uses.
3. Press F5 (or `dotnet run`). Swagger UI opens automatically at `/swagger`.

---

## Related Samples

- **`Samples.MvcWebApi`** — the classic ASP.NET Web API 2 project this one is the modern sibling of, worth comparing directly, especially `LectureNotes.md`'s "what's different" discussion.
