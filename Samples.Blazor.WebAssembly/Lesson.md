# Samples.Blazor.WebAssembly

## What This Is

Blazor WebAssembly -- the other Blazor hosting model. The .NET runtime itself is compiled to WebAssembly and downloaded to run **entirely inside the browser**. No persistent server connection needed for UI updates, but also no way to access server-side resources directly. It calls `Samples.MvcWebApi.Core`'s HTTP API to perform the ZIP code lookup -- the same task every other Samples project performs, but the data genuinely cannot come from anywhere except an HTTP call.

**No `net48` sibling.** Blazor is purely a modern ASP.NET Core technology.

---

## When to Use Blazor WebAssembly

For public internet-facing apps, offline-capable scenarios, or anywhere a persistent server connection (Blazor Server's requirement) isn't acceptable. Trade-offs:

- Larger initial download -- the .NET runtime itself ships to the browser.
- Needs a backend API for any server-side resource access -- this project genuinely cannot query a database directly.
- No server-side debugger -- browser developer tools are the primary debugging surface.
- Works offline once loaded (with service worker configuration).

---

## How Blazor WebAssembly Works

The component structure looks identical to Blazor Server -- `.razor` files mixing markup and C#. The difference is where the code runs and how data is fetched:

```razor
@page "/"
@inject HttpClient Http

<input @bind="zipCode" />
<button @onclick="Search">Search</button>

@foreach (var row in results)
{
    <p>@row.City, @row.State</p>
}

@code {
    string zipCode;
    List<Location> results = [];

    async Task Search()
    {
        var response = await Http.GetFromJsonAsync<LocationLookupResponse>(
            $"locationlookup/{zipCode}");
        results = response?.Locations ?? [];
    }
}
```

`@inject HttpClient Http` injects an `HttpClient` pre-configured with the API's base URL from `wwwroot/appsettings.json`. This runs in the browser's WebAssembly sandbox -- there is no `DbContext`, no file system, no direct database access.

**Verify it:** open the browser's Network tab before searching, then search. A real `GET` request to the API fires and is visible -- fundamentally different from `Samples.Blazor.Server`, which produces no HTTP request at all.

---

## Creating a Blazor WebAssembly Project

### Visual Studio

**File > New > Project**, search "Blazor WebAssembly App", click Next, choose .NET version, click Create.

The `HttpClient` is pre-registered in `Program.cs`. Configure its base address for the API:

```csharp
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
```

Or read the API URL from `wwwroot/appsettings.json` (the browser-side config file -- not `appsettings.json` in the project root):

```json
{ "ApiBaseUrl": "https://localhost:44314/api/" }
```

### VS Code

```powershell
dotnet new blazorwasm -n MyBlazorWasm -f net10.0
dotnet run
```

Configure the `HttpClient` base address in `Program.cs` and set the API URL in `wwwroot/appsettings.json`.

---

## Running This Project

This project requires `Samples.MvcWebApi.Core` running alongside it.

1. Point `Samples.MvcWebApi.Core/appsettings.json`'s `LocationLookupDatabase` at a SQL Server instance.
2. Configure multiple startup projects: right-click the Solution > **Configure Startup Projects** > "Multiple startup projects" > set both `Samples.MvcWebApi.Core` and `Samples.Blazor.WebAssembly` to "Start".
3. Press F5. Both launch together. If Visual Studio reports a port conflict, start each individually: right-click each project > Debug > Start New Instance, starting `Samples.MvcWebApi.Core` first.
4. If the API isn't on its default port, update `wwwroot/appsettings.json`'s `ApiBaseUrl` to match.

CORS is already configured in `Samples.MvcWebApi.Core` to allow this project's origins.

---

## Blazor Server vs. Blazor WebAssembly

| | Blazor Server | Blazor WebAssembly |
|---|---|---|
| Code runs | On the server | In the browser |
| Database access | Direct | Via HTTP API only |
| Network requirement | Persistent SignalR connection | Initial download only |
| Offline support | No | Yes (with service worker) |
| Initial load | Fast | Slower (downloads .NET runtime) |
| Scale model | Per-user circuit on server | Stateless -- server only serves files |

---

## Related Projects

- `Samples.Blazor.Server` -- the other Blazor hosting model, accesses the database directly.
- `Samples.MvcWebApi.Core` -- the API this project calls.
- `Samples.MvcWebApi.Core.Client` -- a .NET console client using the same `HttpClient` pattern.
