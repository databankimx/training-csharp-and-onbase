# Samples.MvcWebApi.Client

## What This Is

A .NET console client consuming `Samples.MvcWebApi` via raw `HttpWebRequest` -- the old-fashioned approach, kept deliberately to show what pre-`HttpClient` .NET networking code looked like. References `Samples.MvcWebApi.Common` for the shared request/response types.

---

## When to Use This Pattern

**Only in legacy code you're already maintaining.** Any new client consuming a REST API should use `HttpClient` with `async`/`await` instead (see `Samples.MvcWebApi.Core.Client`). `HttpWebRequest` blocks the calling thread and requires far more boilerplate.

---

## How It Works

```csharp
var request = (HttpWebRequest)WebRequest.Create($"{WebApiUrl}/api/locationlookup");
request.Method = "POST";
request.ContentType = "application/json";
using var writer = new StreamWriter(request.GetRequestStream());
writer.Write(JsonSerializer.Serialize(locationRequest));

var response = (HttpWebResponse)request.GetResponse();
using var reader = new StreamReader(response.GetResponseStream());
string json = reader.ReadToEnd();
var result = JsonSerializer.Deserialize<LocationResponse>(json);
```

Compare this directly against `Samples.MvcWebApi.Core.Client`'s two-line equivalent using `PostAsJsonAsync`.

---

## Creating a Similar Project

### Visual Studio

**File > New > Project**, "Console App (.NET Framework)". Add a project reference to `Samples.MvcWebApi.Common` (or the equivalent shared DTO library).

### VS Code

```powershell
dotnet new console -n MyApiClient -f net48
dotnet add reference ../Samples.MvcWebApi.Common/Samples.MvcWebApi.Common.csproj
```

---

## Running This Project

1. Start `Samples.MvcWebApi` (F5 in Visual Studio) and leave it running.
2. Run this project. It calls each API method and prints the raw JSON sent and received.

---

## Related Projects

- `Samples.MvcWebApi` -- the API this client consumes.
- `Samples.MvcWebApi.Common` -- the shared DTOs.
- `Samples.MvcWebApi.Core.Client` -- the modern `HttpClient`/`async` equivalent.
