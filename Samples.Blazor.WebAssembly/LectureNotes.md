# Samples.Blazor.WebAssembly

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port, Blazor has no `net48` equivalent). See `README.md` for the fuller when-to-use discussion and the contrast against `Samples.Blazor.Server`.

---

## The Same Component, Genuinely Different Constraints

`Home.razor` here and `Samples.Blazor.Server`'s `Home.razor` look almost identical at the markup level, same input, same button, same results table. The `SearchAsync` method is where the two projects genuinely diverge:

```csharp
// Samples.Blazor.WebAssembly
var response = await Http.GetFromJsonAsync<LocationLookupResponse>($"locationlookup/{zipCode}");
locations = response?.Data ?? [];
```

```csharp
// Samples.Blazor.Server
await using var db = await DbContextFactory.CreateDbContextAsync();
locations = await db.ZipCodes.Where(z => z.ZipCode1 == zipCode).ToListAsync();
```

This isn't a stylistic choice, it's a hard constraint. This project's code is downloaded to and executed **inside the browser's sandbox**, the same environment JavaScript runs in, and browsers don't let arbitrary code open raw SQL connections, full stop. Calling an HTTP API is the *only* option, exactly the situation any JavaScript SPA is in.

---

## Reusing `Samples.MvcWebApi.Core.Common` Directly

Rather than defining another copy of `Location`/`LocationLookupResponse`, this project references `Samples.MvcWebApi.Core.Common` directly, the same shared library `Samples.MvcWebApi.Core` (the API) and `Samples.MvcWebApi.Core.Client` (the console client) both already use. Blazor WebAssembly projects can reference ordinary .NET class libraries just like any other .NET project, as long as the library itself doesn't depend on anything the browser sandbox can't support (this one doesn't, it's just plain `record` types), a genuine, practical benefit of Blazor WASM being real .NET rather than a transpiled subset.

---

## `wwwroot/appsettings.json`: Config the Browser Can See

Unlike every server-hosted project in this training set, this file lives in `wwwroot`, meaning it's served as a **plain static asset**, downloaded by the browser like any image or stylesheet, and `WebAssemblyHostBuilder` fetches it automatically over HTTP during startup. Nothing in it is private: anyone can view it directly at `/appsettings.json` in a browser. Worth remembering when deciding what belongs here versus what belongs in the server-side API you're calling instead, connection strings or secrets have no business in a Blazor WebAssembly project's configuration at all.

---

## CORS: A Genuine Requirement Here

Because this page's origin (`https://localhost:44321`) is different from `Samples.MvcWebApi.Core`'s own origin (`https://localhost:44314`), the browser enforces CORS on every request `Home.razor` makes. `Samples.MvcWebApi.Core`'s `appsettings.json` `Cors:AllowedOrigins` list includes this project's origin specifically for that reason, without it, `SearchAsync`'s `HttpClient` call would fail with a CORS error the browser reports directly in its console, not something `Samples.Blazor.Server` ever has to think about, its calls never leave the server at all.

---

## Try It Yourself

Run both `Samples.MvcWebApi.Core` and this project, open the browser's Network tab, and search a ZIP code, watch a real `GET` request fire to `/api/locationlookup/{zipCode}`. Then run `Samples.Blazor.Server` and do the same search there, no equivalent request appears at all.
