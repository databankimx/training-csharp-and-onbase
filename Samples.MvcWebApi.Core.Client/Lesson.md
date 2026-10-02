# Samples.MvcWebApi.Core.Client

## What This Is

A .NET console client consuming `Samples.MvcWebApi.Core` with `HttpClient` and `async`/`await` throughout -- the direct, modern contrast to `Samples.MvcWebApi.Client`'s legacy `HttpWebRequest` approach. Worth reading both side by side.

---

## When to Use This Pattern

For any new .NET client consuming a REST API. `HttpClient` with `System.Net.Http.Json`'s typed helpers is the current standard: less boilerplate, proper cancellation, genuinely async I/O, and no risk of the thread-blocking behavior raw `HttpWebRequest` exhibits.

---

## How It Works

```csharp
using var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

// POST with JSON body, typed deserialization
var response = await client.PostAsJsonAsync("test", request, jsonOptions);
var testResponse = await response.Content.ReadFromJsonAsync<TestResponse>(jsonOptions);

// GET with typed deserialization
var location = await client.GetFromJsonAsync<LocationLookupResponse>($"locationlookup/{zipCode}");
```

`PostAsJsonAsync`/`GetFromJsonAsync`/`ReadFromJsonAsync` are extension methods from `System.Net.Http.Json` (included in the BCL since .NET 5). No manual stream writing, no manual JSON parsing. Compare this against `Samples.MvcWebApi.Client`'s equivalent, which takes about ten times as many lines.

---

## Creating a Similar Project

### Visual Studio

**File > New > Project**, "Console App", choose the .NET version. `System.Net.Http.Json` is already part of the BCL -- no extra package needed. Add a reference to the shared Common library.

### VS Code

```powershell
dotnet new console -n MyApiClient -f net10.0
dotnet add reference ../Samples.MvcWebApi.Core.Common/Samples.MvcWebApi.Core.Common.csproj
```

`System.Net.Http.Json` is available without any package reference on `net10.0`. Add it explicitly only if targeting `net48`:

```powershell
dotnet add package System.Net.Http.Json
```

---

## Running This Project

1. Start `Samples.MvcWebApi.Core` (F5 or `dotnet run`) and leave it running.
2. If the API isn't on its default port, update `appsettings.json`'s `ApiBaseUrl`.
3. Run this project.

---

## Related Projects

- `Samples.MvcWebApi.Core` -- the API this client consumes.
- `Samples.MvcWebApi.Client` -- the legacy `HttpWebRequest` sibling for direct comparison.
- `Samples.MvcWebApi.Core.Common` -- the shared DTOs.
