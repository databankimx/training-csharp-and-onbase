# Samples.Blazor.Server

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port, Blazor has no `net48` equivalent). See `README.md` for the fuller when-to-use discussion and the contrast against `Samples.Blazor.WebAssembly`.

---

## `@rendermode InteractiveServer`: Interactivity Is Opt-In

```razor
@page "/"
@rendermode InteractiveServer
```

Since .NET 8, Blazor components are **static** (server-rendered HTML, no live connection) by default, a deliberate performance choice, most of a typical page doesn't need one. `@rendermode InteractiveServer` on `Home.razor` is what opts *this specific component* into a live SignalR-backed circuit, everything below it (the `@bind` on the text input, the `@onclick` handler, the re-rendered results table) works because of that one directive. Leaving it off would render the page once and never update it again without a full page reload.

---

## No API, No HTTP Call, Direct Database Access

```csharp
private async Task SearchAsync()
{
    await using var db = await DbContextFactory.CreateDbContextAsync();
    locations = await db.ZipCodes.Where(z => z.ZipCode1 == zipCode).ToListAsync();
    hasSearched = true;
}
```

`SearchAsync` runs **on the server**, in the same process as the EF Core `DbContext`. There's no `HttpClient`, no serialization, no network round-trip to a separate API, the click event itself travels over the existing SignalR connection, and the query result comes straight back as C# objects. This is the single biggest practical difference from `Samples.Blazor.WebAssembly`, whose equivalent `SearchAsync` genuinely cannot do this, its code runs inside the browser's sandbox, with no way to open a direct SQL connection at all.

---

## `IDbContextFactory<T>`, Not `AddDbContext`, and Why

```csharp
builder.Services.AddDbContextFactory<LocationLookupContext>(options => ...);
```

A Blazor Server **circuit** (one connected browser tab) is long-lived, potentially for an entire user session, genuinely different from a single HTTP request's lifetime. Registering `LocationLookupContext` the usual scoped way (`AddDbContext`) and injecting it directly into `Home.razor` would keep **one** `DbContext` instance alive for the whole circuit, the same class of problem as holding a scoped service inside a singleton (see `Samples.WindowsService.NetCore`'s own `IServiceScopeFactory` usage for the analogous fix in a different context). `AddDbContextFactory` plus `IDbContextFactory<LocationLookupContext>.CreateDbContextAsync()` in `SearchAsync` creates a genuinely fresh, short-lived context for each individual search instead, this is the Microsoft-recommended registration specifically for Blazor Server, not something invented for this sample.

---

## Try It Yourself

Run the project, open the browser's Network tab, and search a ZIP code, notice there's no new HTTP request for the search itself, only WebSocket frames on the existing SignalR connection. Then compare this directly against `Samples.Blazor.WebAssembly`, where the search genuinely does trigger a visible HTTP request to a separate API.
