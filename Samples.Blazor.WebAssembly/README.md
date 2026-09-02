# Samples.Blazor.WebAssembly

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Blazor WebAssembly, the other Blazor hosting model. The .NET runtime itself is compiled to WebAssembly and downloaded to run **entirely inside the browser**, no persistent server connection needed for UI updates, but also no way to access server-side resources directly. It calls `Samples.MvcWebApi.Core`'s real HTTP API to look up city/county/state by ZIP code, the exact same task every other sample in this training set performs.

`Samples.Blazor.Server` is the other hosting model, running on the server and accessing the database directly, no API call needed. See that project's own notes for the full contrast.

**No `net48` sibling exists for this project.** Blazor is purely a modern ASP.NET Core technology.

---

## When to Use Blazor WebAssembly

For public internet-facing apps, offline-capable scenarios, or anywhere a persistent server connection (Blazor Server's requirement) isn't acceptable. Trade-off: a larger initial download (the .NET runtime itself), and, like any client-side app, it needs a real backend API for anything requiring server-side resources, this project genuinely cannot query a database directly.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Pages/Home.razor` | The search form and results, calling the API via `HttpClient` |
| `Layout/MainLayout.razor` | Shared page chrome |
| `Program.cs` | `WebAssemblyHostBuilder` setup, `HttpClient` registration |
| `wwwroot/appsettings.json` | `ApiBaseUrl`, fetched by the browser at startup |

Depends on `Samples.MvcWebApi.Core.Common` for the `Location`/`LocationLookupResponse` shapes, reused directly rather than duplicated.

---

## How to Run

This project needs `Samples.MvcWebApi.Core` running alongside it, it calls that API rather than touching the database directly.

1. Point `Samples.MvcWebApi.Core\appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Set up multiple startup projects: right-click the Solution node in Solution Explorer > Configure Startup Projects... > select "Multiple startup projects" > set both `Samples.MvcWebApi.Core` and `Samples.Blazor.WebAssembly` to Action "Start", leave everything else "None".
3. Press F5. Both launch together, `Samples.MvcWebApi.Core` opens its Swagger UI, this project opens its own page. If Visual Studio reports a WebTools/port conflict launching them simultaneously, restart Visual Studio, or start each individually instead: right-click each project > Debug > Start New Instance, `Samples.MvcWebApi.Core` first.
4. If the API isn't running on its default port (`https://localhost:44314`), update `wwwroot/appsettings.json`'s `ApiBaseUrl` to match.
5. Enter a ZIP code and click Search. Open the browser's Network tab first if you want to see the actual request fire: a real `GET` to `/api/locationlookup/{zipCode}`, genuinely different from `Samples.Blazor.Server`'s equivalent search, which produces no new HTTP request at all.

CORS is already configured for this: `Samples.MvcWebApi.Core`'s `appsettings.json` `Cors:AllowedOrigins` includes this project's origins (`https://localhost:44321`, `http://localhost:5183`), no changes needed there unless you change this project's port.

---

## Related Samples

- **`Samples.Blazor.Server`** — the other Blazor hosting model, accesses the database directly instead of calling an API, worth comparing directly.
- **`Samples.MvcWebApi.Core`** — the API this project calls.
- **`Samples.MvcWebApi.Core.Client`** — a .NET console client for the same API, using the same `HttpClient`/`System.Net.Http.Json` pattern.
