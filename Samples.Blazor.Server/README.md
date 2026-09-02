# Samples.Blazor.Server

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Blazor Server, one of Blazor's two hosting models. Component code runs **on the server**, and UI updates are pushed to the browser over a persistent SignalR connection, no full-page reload, no traditional HTTP request/response cycle for interactions after the initial page load. Because the code genuinely runs server-side, a component can access server resources (a database, in this case) **directly**, no API layer needed at all.

Like every other sample in this training set, it looks up city/county/state by ZIP code.

`Samples.Blazor.WebAssembly` is the other hosting model, running entirely in the browser, and genuinely *cannot* do what this project does, it has to call an HTTP API instead. See that project's own notes for the full contrast.

**No `net48` sibling exists for this project.** Blazor is purely a modern ASP.NET Core technology with no classic equivalent at all, the same situation as `Samples.RazorPages`.

---

## When to Use Blazor Server

For internal or low-latency-network applications where a persistent connection to the server is acceptable, and where direct server-side access (databases, file systems, existing server-side libraries) simplifies the architecture. Less suitable for public internet-facing apps at large scale (each connected user holds a server-side circuit and a SignalR connection) or for offline/poor-connectivity scenarios, where Blazor WebAssembly is the better fit.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Components/Pages/Home.razor` | The search form and results, directly querying EF Core |
| `Components/Layout/MainLayout.razor` | Shared page chrome |
| `Models/ZipCode.cs` | EF Core entity (Code-First) |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |

---

## How to Run

This project is self-contained, no other project needs to be running.

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Right-click this project in Solution Explorer and choose Debug > Start New Instance (or set it as the sole startup project and press F5, or `dotnet run`). A browser tab opens automatically.
3. Enter a ZIP code and click Search, watch the results appear without the page reloading.
4. To confirm nothing left the server: open the browser's Network tab *before* searching, then search, no new HTTP request appears, only WebSocket frames on the existing SignalR connection. Compare this directly against `Samples.Blazor.WebAssembly`'s equivalent search, which does produce a visible request.

---

## Related Samples

- **`Samples.Blazor.WebAssembly`** — the other Blazor hosting model, runs entirely in the browser and calls an API instead of accessing the database directly, worth comparing directly.
- **`Samples.MvcWebApi.Core`** — the API `Samples.Blazor.WebAssembly` calls.
