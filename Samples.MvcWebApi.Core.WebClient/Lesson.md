# Samples.MvcWebApi.Core.WebClient

## What This Is

A browser-based test console for `Samples.MvcWebApi.Core`, using the browser's native `fetch()` API with `async`/`await` -- no jQuery, no library. Hosted by a three-line ASP.NET Core static-file app, a real contrast against the classic web clients, which each needed a full legacy Web Application Project just to serve static files.

---

## When to Use This Pattern

For any browser client calling a modern REST API. The native `fetch()` API is available in every current browser, handles JSON cleanly, and requires no dependencies.

---

## How It Works

```javascript
const response = await fetch(`${apiUrl}/locationlookup/${zipCode}`);
const data = await response.json();
displayResults(data.locations);
```

For POST:

```javascript
const response = await fetch(`${apiUrl}/test`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request)
});
```

No jQuery, no `$.ajax()`, no library dependency at all. Compare this against `Samples.MvcWebApi.WebClient`'s jQuery-based equivalent.

The hosting app is genuinely minimal:

```csharp
// Program.cs -- three lines
var app = WebApplication.Create(args);
app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();
```

---

## Creating a Similar Project

### Visual Studio

**File > New > Project**, "ASP.NET Core Empty". Delete everything except `Program.cs`. Replace its contents with the three lines above. Add an `index.html` under `wwwroot/`.

### VS Code

```powershell
dotnet new web -n MyWebClient -f net10.0
```

Replace `Program.cs` with the three-line static-file version. Create `wwwroot/index.html` with your HTML/JS/CSS. Run with `dotnet run`.

For local development without a .NET host at all, VS Code's **Live Server** extension serves any folder of static files directly -- no `Program.cs` needed.

---

## Running This Project

1. Start `Samples.MvcWebApi.Core` (F5 or `dotnet run`) and leave it running.
2. Run this project (F5 or `dotnet run`). Use the operation cards and the Swagger link.

---

## Related Projects

- `Samples.MvcWebApi.Core` -- the API this page calls.
- `Samples.MvcWebApi.WebClient` -- the jQuery-based sibling for comparison.
- `Samples.MvcWebApi.Core.Client` -- a .NET console client for the same API.
