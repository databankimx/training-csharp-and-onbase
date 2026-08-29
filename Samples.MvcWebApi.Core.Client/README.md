# Samples.MvcWebApi.Core.Client

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

A console application consuming `Samples.MvcWebApi.Core` with `HttpClient` and `async`/`await` throughout, the direct, modern contrast to `Samples.MvcWebApi.Client`, which deliberately keeps the old `HttpWebRequest`/`JavaScriptSerializer` style. Worth reading both side by side.

```csharp
var response = await client.PostAsJsonAsync("test", request, jsonOptions);
var testResponse = await response.Content.ReadFromJsonAsync<TestResponse>(jsonOptions);
```

No manual stream writing, no manual JSON string parsing, `System.Text.Json`'s typed HTTP helpers handle serialization and deserialization directly against the request/response body.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Calls `Ping`, `Test`, and `LocationLookup` against the real API routes |
| `appsettings.json` | The API's base URL, editable without a recompile |

Depends on `Samples.MvcWebApi.Core.Common` for the `TestRequest`/`TestResponse`/`LocationLookupResponse`/`Location` shapes.

---

## How to Run

1. Run `Samples.MvcWebApi.Core` first (F5 or `dotnet run`), and leave it running.
2. If the API isn't running on its default port, update `appsettings.json`'s `ApiBaseUrl` to match.
3. Run this project. It calls each of the three API operations in turn.

---

## Related Samples

- **`Samples.MvcWebApi.Core`** — the API this client consumes.
- **`Samples.MvcWebApi.Client`** — the classic, deliberately legacy-style sibling, worth comparing directly.
- **`Samples.MvcWebApi.Core.WebClient`** — a browser-based client for the same API.
