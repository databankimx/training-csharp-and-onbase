# Samples.MvcWebApi.Core

## What This Is

The ASP.NET Core Web API sibling of `Samples.MvcWebApi`, targeting **.NET 10** (the current LTS as of this writing, released November 2025). This is a genuinely separate, modern implementation, not a straight port, since a substantial amount of the underlying plumbing is structurally different between classic ASP.NET and ASP.NET Core, not just renamed. See `README.md` for the pros/cons/when-to-use discussion.

---

## Why This Couldn't Just Reuse `CSharp.SharedLibrary`/`DatabankException`

`CSharp.SharedLibrary` (and the `DatabankException` standard applied throughout every other `Samples.*` project) targets `net48` via this solution's `Directory.Build.props`. .NET Framework and modern .NET compatibility is one-directional: a `net48` library can be referenced by other `net48`/.NET Framework projects, but **not** by a `net10.0` project at all, there's no compatibility shim in that direction. Rather than multi-targeting `CSharp.SharedLibrary` (a real, if plausible, option, but a bigger structural change touching a library used across the entire training curriculum) or duplicating `DatabankException` into a second, `net10.0`-targeted copy, this project uses ASP.NET Core's own built-in `IExceptionHandler` (added in .NET 8) instead, genuinely the modern, idiomatic replacement, not a workaround. See `GlobalExceptionHandler.cs`.

---

## EF Core Code-First vs. EF6 Database-First

`Samples.MvcWebApi`'s `LocationLookupModel.edmx` was reverse-engineered from an *existing* database table, the `.edmx` (and its generated `ZipCode`/`LocationLookupDatabase` classes) are the artifact, the database came first. Here, `Models/ZipCode.cs` **is** the source of truth, a plain C# class with no EDMX indirection at all, EF Core generates (or updates) the actual database schema *from* this class via a migration (`dotnet ef migrations add`, `dotnet ef database update`). Both point at the same conceptual `ZipCodes` table for this sample, worth noting the column name difference: EF6's Database-First tooling appended a `1` to `ZipCode` (`ZipCode1`) specifically because `ZipCode` collided with the table's own name in its naming convention. EF Core Code-First has no such restriction, the C# property could be named `ZipCode` directly, but `LocationLookupContext.OnModelCreating` explicitly maps a `ZipCode1` property to the real `ZipCode` column, so this project stays pointed at the exact same underlying table structure either way, worth seeing both approaches side by side.

---

## Dependency Injection Instead of Direct Construction

`Samples.MvcWebApi.Controllers.LocationLookupController` does `var db = new LocationLookupDatabase();` directly inside the action method. This project instead registers `LocationLookupContext` once, centrally, in `Program.cs` (`builder.Services.AddDbContext<LocationLookupContext>(...)`), and `LocationLookupController`'s constructor simply *receives* it (`LocationLookupController(LocationLookupContext db)`, a primary constructor). This is the actual point of ASP.NET Core's DI container: nothing in the controller decides *how* to construct its dependencies, or *when* they're disposed, that's centralized, testable, and swappable in one place.

---

## Genuinely Async Database Access

```csharp
var locations = await db.ZipCodes
    .Where(z => z.ZipCode1 == zipCode)
    .Select(z => new Location(z.State, z.County, z.City, z.ZipCode1))
    .ToListAsync();
```

`ToListAsync()` genuinely doesn't block a thread while the database round-trip is in flight, that thread is free to serve other requests in the meantime. EF6's synchronous `.ToList()` (used throughout `Samples.MvcWebApi`) blocks the calling thread for the full duration of the query. Under load, this is a real, measurable difference in how many concurrent requests a given number of threads can actually serve.

---

## `ProblemDetails` Instead of "Always 200, Check the Errors Array"

`Samples.MvcWebApi`'s controllers always return HTTP `200 OK`, even on failure, the caller has to inspect a response's `Errors` field to know something went wrong (see its own `LectureNotes.md`/`README.md` for that pattern's rationale, largely a product of `ExceptionFilter` rewriting the response in place). Here, `GlobalExceptionHandler` returns a real `500` status code with a `ProblemDetails` (RFC 7807) body, a standard, structured error shape every modern HTTP client and API tooling already knows how to interpret without any DataBank-specific convention to learn. Worth treating as a genuine design improvement, not just a stylistic change, callers can use ordinary HTTP status-code checking instead of always parsing the body to find out whether a call succeeded.

---

## Configuration and Logging: Built In, Not Wired By Hand

The classic projects (`Samples.AsmxWebService`, `Samples.WcfService`, `Samples.MvcWebApi`) each needed a hand-built `ConfigurationBuilder` + `AddJsonFile("serilog.json")` call just to let Serilog read its own sink configuration, `System.Web`-hosted applications have no built-in equivalent to ASP.NET Core's configuration system. Here, `WebApplicationBuilder` already loads `appsettings.json`, `appsettings.{Environment}.json`, and environment variables into one merged `IConfiguration` before `Program.cs`'s first line even runs, and `builder.Host.UseSerilog(...)` reads Serilog's `"Serilog"` section from that same object directly. No `serilog.json`, no manual `ConfigurationBuilder`, one line of setup.

---

## Try It Yourself

Run the project (`dotnet run`, or F5), Swagger UI opens automatically. Try `LocationLookup`, then compare the actual HTTP response headers/status code against what `Samples.MvcWebApi`'s equivalent call returns for the same failure case (an invalid ZIP code, or a database connection failure), the difference between "always 200, check the body" and "a real status code" is easiest to see side by side.
