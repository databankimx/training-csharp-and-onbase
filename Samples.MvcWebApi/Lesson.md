# Samples.MvcWebApi

## What This Is

ASP.NET Web API 2 (2012), the third generation of .NET web service technology. Where ASMX is SOAP-only and WCF is contract-first with multiple bindings, Web API is natively RESTful and JSON-first: a controller method is an HTTP endpoint, no separate contract file, no binding configuration. Same ZIP code lookup as every other Samples project, backed by EF6 Database-First.

The project also demonstrates **action filters** (`LogFilter`, `ExceptionFilter` in `Filters/`) as a way to centralize cross-cutting concerns across all controllers -- each controller has the equivalent hand-written `try`/`catch` commented out directly alongside the filter-based version, so you can compare both approaches side by side.

---

## When to Use Web API 2

Only for existing classic ASP.NET applications, or genuine constraints ruling out ASP.NET Core. For any new REST API, ASP.NET Core Web API is the actively-developed successor.

---

## How Web API Works

A controller inherits `ApiController` and its methods become HTTP endpoints, with routes and HTTP verbs configured via attributes or convention:

```csharp
[RoutePrefix("api/locationlookup")]
public class LocationLookupController : ApiController
{
    [HttpPost, Route("")]
    [LogFilter, ExceptionFilter]
    public IHttpActionResult Post(LocationRequest request)
    {
        // EF6 query against the ZipCodes table
        return Ok(new LocationResponse { ... });
    }
}
```

No WSDL, no generated proxy. `Samples.MvcWebApi.Common` provides shared request/response DTOs that both the server and any .NET client reference directly -- the convention that fills the gap WSDL used to cover.

---

## Creating a Web API 2 Project

### Visual Studio

**File > New > Project**, search "ASP.NET Web Application (.NET Framework)", name the project, click Next, select "Web API" template. This creates the project with the routing, serialization, and Swagger scaffolding set up. To add Swagger: right-click the project > Manage NuGet Packages, install `Swashbuckle`.

### VS Code

ASP.NET Web API 2 requires the classic `System.Web` hosting model. Create the project in Visual Studio, then open the folder in VS Code for editing. There is no `dotnet new` template for this project type.

---

## Running This Project

1. Point `Web.config`'s `LocationLookupDatabase` connection string at a SQL Server instance with a `ZipCodes` table.
2. Press F5 (IIS Express).
3. Browse to `/swagger` for interactive API documentation.

---

## Pros and Cons

**Pros:** Natively RESTful, JSON-first. Native EF integration. Filters centralize cross-cutting concerns. Swagger/Swashbuckle for interactive docs.

**Cons:** No built-in contract/WSDL -- shared library convention required. Superseded by ASP.NET Core. Tied to classic .NET Framework and IIS.

---

## Related Projects

- `Samples.MvcWebApi.Common` -- shared DTOs.
- `Samples.MvcWebApi.Client` -- .NET console client.
- `Samples.MvcWebApi.WebClient` -- browser client.
- `Samples.MvcWebApi.Core` -- the modern ASP.NET Core successor.
