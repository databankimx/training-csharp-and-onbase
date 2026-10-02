# Samples.Blazor.Server

## What This Is

Blazor Server -- one of Blazor's two hosting models. Component code runs **on the server**, and UI updates are pushed to the browser over a persistent SignalR connection. No full-page reloads, no traditional HTTP request/response cycle for interactions after the initial page load. Because the code genuinely runs server-side, a component can access server resources (a database, in this case) **directly** -- no API layer needed.

Same ZIP code lookup as every other Samples project. `Home.razor` queries EF Core directly without calling any API.

**No `net48` sibling.** Blazor is purely a modern ASP.NET Core technology.

---

## When to Use Blazor Server

For internal or low-latency-network applications where:
- A persistent connection to the server is acceptable.
- Direct server-side access (databases, file systems, existing server libraries) simplifies the architecture.
- C# everywhere (no JavaScript) is preferred.

Less suitable for public internet-facing apps at large scale (each connected user holds a server-side circuit and a SignalR connection) or for offline/poor-connectivity scenarios -- where Blazor WebAssembly is the better fit.

---

## How Blazor Server Works

A `.razor` component mixes HTML markup with C# in a single file. Event handlers and state changes happen server-side; the Blazor runtime diffs the component tree and sends only the DOM delta to the browser over SignalR:

```razor
@page "/"
@inject LocationLookupContext Db

<input @bind="zipCode" />
<button @onclick="Search">Search</button>

@foreach (var row in results)
{
    <p>@row.City, @row.State</p>
}

@code {
    string zipCode;
    List<ZipCode> results = [];

    async Task Search()
    {
        results = await Db.ZipCodes
            .Where(z => z.ZipCode1 == zipCode)
            .ToListAsync();
    }
}
```

`@inject` gets the `DbContext` from DI. `@bind` creates two-way binding between the input and the `zipCode` field. `@onclick` wires the button to the `Search` method. No JavaScript, no HTTP call -- the click event travels over SignalR, the query runs on the server, and only the updated DOM diff travels back.

**Verify it:** open the browser's Network tab before searching, then search. No new HTTP request appears -- only WebSocket frames on the existing SignalR connection.

---

## Creating a Blazor Server Project

### Visual Studio

**File > New > Project**, search "Blazor Web App", click Next. On the options page, set **Interactive render mode** to "Server" and **Interactivity location** to "Global". Choose .NET version, click Create.

To add EF Core: Manage NuGet Packages > install `Microsoft.EntityFrameworkCore.SqlServer`. Register the `DbContext` in `Program.cs`:

```csharp
builder.Services.AddDbContextFactory<LocationLookupContext>(options =>
    options.UseSqlServer(connectionString));
```

Note: prefer `AddDbContextFactory` in Blazor Server -- a scoped `DbContext` lives for the duration of the SignalR circuit (potentially hours), not a single request. A factory lets each operation create and dispose its own short-lived context.

To add a new component: right-click `Components/Pages/` > **Add > Razor Component**.

### VS Code

```powershell
dotnet new blazorserver -n MyBlazorServer -f net10.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet run
```

Add `.razor` files under `Components/Pages/`. Each file with `@page "/route"` at the top becomes a routable page.

---

## Running This Project

This project is self-contained -- no other project needs to be running.

1. Point `appsettings.json`'s `LocationLookupDatabase` at a SQL Server instance.
2. Press F5 or `dotnet run`. A browser tab opens automatically.
3. Search a ZIP code. Watch the Network tab to confirm no HTTP request fires.

---

## Pros and Cons

**Pros:** Direct server-side access -- no API layer needed. C# everywhere, no JavaScript required. Full .NET debugger experience in the browser.

**Cons:** Requires a persistent SignalR connection per user -- scales differently from stateless HTTP. No offline support. Initial page load requires server round-trip.

---

## Related Projects

- `Samples.Blazor.WebAssembly` -- the other Blazor hosting model, runs in the browser, must call an API.
- `Samples.MvcWebApi.Core` -- the API `Samples.Blazor.WebAssembly` calls.
