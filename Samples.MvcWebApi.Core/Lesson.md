# Samples.MvcWebApi.Core

## What This Is

ASP.NET Core Web API -- the current, actively-developed successor to classic Web API 2. This is a modern, separate implementation of the same three operations (Ping, Test, LocationLookup), not a port. The plumbing changed structurally: dependency injection, async EF Core, `ProblemDetails` error responses, and built-in configuration and CORS middleware replace the hand-wired equivalents in the classic project.

Same ZIP code lookup as every other Samples project, backed by EF Core Code-First (not EF6 Database-First).

---

## When to Use ASP.NET Core Web API

For essentially all new REST API work. Cross-platform (Windows, Linux, macOS, containers), built around dependency injection and middleware from the ground up, actively developed by Microsoft. This is the default for new .NET REST APIs.

---

## Key Differences From Classic Web API 2

| Classic (`Samples.MvcWebApi`) | ASP.NET Core (`Samples.MvcWebApi.Core`) |
|---|---|
| EF6 Database-First (`.edmx`) | EF Core Code-First (`ZipCode.cs` is the source of truth) |
| Direct `new DbContext()` construction | DI-injected `LocationLookupContext` |
| Synchronous `.ToList()` | `await db.ZipCodes.ToListAsync()` |
| Always `200 OK`, check `Errors` field | Real HTTP status codes + `ProblemDetails` (RFC 7807) |
| `Newtonsoft.Json` | `System.Text.Json` (built in) |
| Manual `ConfigurationBuilder` for Serilog | One-line `builder.Host.UseSerilog(...)` |
| Manual CORS headers in `Global.asax.cs` | Built-in CORS middleware, configured in `Program.cs` |
| `net48`, Windows/IIS only | `net10.0`, cross-platform |

`CSharp.SharedLibrary` (which targets `net48`) cannot be referenced by a `net10.0` project -- a `GlobalExceptionHandler` returning `ProblemDetails` is the genuine modern equivalent of `DatabankException.Log()`.

---

## How It Works

```csharp
// Program.cs
builder.Services.AddDbContext<LocationLookupContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocationLookupDatabase")));

// LocationLookupController.cs
public class LocationLookupController(LocationLookupContext db) : ControllerBase
{
    [HttpGet("{zipCode}")]
    public async Task<IActionResult> Get(string zipCode)
    {
        var locations = await db.ZipCodes
            .Where(z => z.ZipCode1 == zipCode)
            .Select(z => new Location(z.State, z.County, z.City, z.ZipCode1))
            .ToListAsync();
        return Ok(new LocationLookupResponse(locations));
    }
}
```

---

## Creating an ASP.NET Core Web API Project

### Visual Studio

**File > New > Project**, search "ASP.NET Core Web API", name the project, click Next, choose .NET version, leave "Enable OpenAPI support" checked (this sets up Swagger automatically), click Create.

### VS Code

```powershell
dotnet new webapi -n MyApi -f net10.0
```

Swagger is included by default. Add EF Core:

```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

Run the app:

```powershell
dotnet run
```

Swagger UI opens at `/swagger`.

---

## Running This Project

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a SQL Server instance.
2. Press F5 or `dotnet run`. Swagger UI opens automatically at `/swagger`.

---

## Related Projects

- `Samples.MvcWebApi` -- the classic Web API 2 sibling for direct comparison.
- `Samples.MvcWebApi.Core.Client` -- a modern `HttpClient`/`async` .NET client.
- `Samples.MvcWebApi.Core.WebClient` -- a browser client using native `fetch()`.
- `Samples.Blazor.WebAssembly` -- also calls this API.
