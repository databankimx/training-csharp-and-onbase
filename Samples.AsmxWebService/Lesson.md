# Samples.AsmxWebService

## What This Is

ASMX is the original .NET web service technology, part of ASP.NET since .NET 1.0 (2002). You will encounter it in real maintenance work on older internal tools and vendor integrations. You would never choose it for something new.

The shared feature across every Samples project: ZIP code lookup (city/county/state) against a `ZipCodes` SQL Server table. Here that lookup is exposed as a SOAP web service via `[WebMethod]`, with a multi-backend `Database.cs` helper supporting SQL Server, Oracle, MySQL, and ODBC.

---

## When to Use ASMX

**Never for new development.** Include it in your working knowledge because existing services need maintenance and cautious extension. Every project started today should use ASP.NET Core Web API (or WCF if genuinely constrained to classic .NET Framework).

---

## How ASMX Works

Marking a public method with `[WebMethod]` turns it into a service operation. ASP.NET handles all request routing, SOAP envelope parsing, and response serialization automatically. Visiting the `.asmx` URL gives a free auto-generated test page; appending `?wsdl` gives the full WSDL document `wsdl.exe` can generate a proxy class from.

```csharp
[WebMethod]
public LocationLookupResponse LookupLocation(LocationLookupRequest request)
{
    // call Database.cs, return a response
}
```

No contract interface. No binding configuration. The class *is* the service.

**The `d` wrapper.** ASMX wraps every JSON response in `{ "d": <actual result> }`, a security mitigation against an old JSON-hijacking attack. Browser clients reading the response need to access `result.d`, not `result` directly.

---

## Creating an ASMX Web Service

### Visual Studio

**File > New > Project**, search "ASP.NET Web Application (.NET Framework)", pick a project name, click Next, select "Empty" template with "Web Forms" checked (or "MVC" if you want routing too). Once the project is open: **right-click the project > Add > New Item > Web Service (ASMX)**. Name it, click Add.

### VS Code

ASMX requires a classic ASP.NET Web Application project -- no `dotnet new` template exists for this. Create it in Visual Studio. VS Code can edit the files, but the `.csproj`/`.asmx` scaffolding must be created there first.

---

## Running This Project

1. Point `Web.config`'s `<database>` element at a SQL Server instance with a `ZipCodes` table.
2. Press F5 in Visual Studio (IIS Express).
3. Browse to `ExampleWebService.asmx` for the auto-generated test page.

---

## Pros and Cons

**Pros:** Extremely simple to implement. Free test page and WSDL. Broad legacy SOAP interoperability.

**Cons:** SOAP-only, verbose XML payloads, no REST/JSON story, no DI support, superseded entirely since 2006. No modern hosting story.

---

## Related Projects

- `Samples.AsmxWebService.Client` -- .NET console client via generated proxy.
- `Samples.AsmxWebService.WebClient` -- browser client via AJAX.
- `Samples.WcfService` -- the technology that succeeded ASMX.
