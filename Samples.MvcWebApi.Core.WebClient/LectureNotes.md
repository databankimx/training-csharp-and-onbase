# Samples.MvcWebApi.Core.WebClient

## What This Is

The direct, modern contrast to `Samples.MvcWebApi.WebClient`, `fetch()`/`async`/`await` instead of jQuery's `$.ajax()`, and a genuinely minimal ASP.NET Core static-file host instead of a full legacy Web Application Project.

---

## Hosting: Three Lines, Not a Whole Legacy Project

```csharp
var app = WebApplication.CreateBuilder(args).Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();
```

The classic web clients (`Samples.AsmxWebService.WebClient`, `Samples.WcfService.WebClient`, `Samples.MvcWebApi.WebClient`) each needed a full legacy Web Application Project, a `.csproj` with IIS Express settings and project type GUIDs, a `Web.config`, purely to get IIS Express to serve a handful of static files at all (the originals were even older "Web Site" projects with no `.csproj` whatsoever). ASP.NET Core's minimal hosting model does the identical job in three lines, no IIS/`System.Web` dependency, no legacy project format needed anywhere.

---

## No "REST vs. JSON" Button Pairs

`Samples.MvcWebApi.WebClient` has two buttons per operation, one calling a JSON POST endpoint, one calling a separate `...Rest`-suffixed GET endpoint, because the classic Web API 2 controllers exposed both as genuinely different routes. `Samples.MvcWebApi.Core`'s attribute-routed endpoints are already properly RESTful on their own (`GET /api/ping`, `POST /api/test`, `GET /api/locationlookup/{zipCode}`), there's no separate REST-variant endpoint to demonstrate as an alternative, so this page has just one button per operation.

---

## `fetch()` Instead of jQuery

```javascript
const response = await fetch(url, {
    method,
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined
});
```

Every classic web client in this training set uses jQuery's `$.ajax()`, genuinely the dominant pattern when those services were first written. Modern browsers have had a built-in, promise-based HTTP API (`fetch()`) for years now, no library needed at all for what this page does. Worth reading directly alongside `Scripts/MvcWebApiWebClient.js` (the classic sibling) to see the same operations expressed both ways.

---

## Try It Yourself

Run `Samples.MvcWebApi.Core` first, then this project. Compare `wwwroot/js/site.js` directly against `Samples.MvcWebApi.WebClient/Scripts/MvcWebApiWebClient.js`, same three operations, genuinely different amount of code and no external library required here.
