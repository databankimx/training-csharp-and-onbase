# Samples.MvcWebApi.Core.Client

## What This Is

The direct, modern contrast to `Samples.MvcWebApi.Client`: `HttpClient` + `async`/`await` + `System.Text.Json`'s typed HTTP extension methods (`PostAsJsonAsync`, `ReadFromJsonAsync`), instead of raw `HttpWebRequest` and manual `JavaScriptSerializer` calls. Both projects call the equivalent operations, worth reading side by side to see exactly what's simplified.

---

## Routes Match the Actual Core API, Not the Classic One

`Samples.MvcWebApi.Core`'s routes are genuinely different from the classic API's: `GET /api/ping`, `POST /api/test`, `GET /api/locationlookup/{zipCode}`, real, idiomatic ASP.NET Core attribute routing, not the classic API's `/api/{controller}/{id}/{data}` convention-based routing. This client calls those actual routes directly, it isn't a copy of the classic client's URL patterns.

---

## camelCase JSON, a Real Wire-Format Detail

ASP.NET Core's default `System.Text.Json` configuration serializes property names as camelCase (`"requestId"`, not `"RequestId"`) even though the C# record properties themselves are PascalCase. This client's `JsonSerializerOptions` sets both `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` (for what it sends) and `PropertyNameCaseInsensitive = true` (for what it reads back), matching the API's own defaults. Worth knowing if you ever need to hand-craft a request against this API with a different tool, the JSON on the wire genuinely isn't PascalCase.

---

## No `DatabankException` Here Either

Same reason as `Samples.MvcWebApi.Core` itself: `CSharp.SharedLibrary` targets `net48`, incompatible with this project's `net10.0` target. Standard exceptions are used directly.

---

## `ApiBaseUrl` Moved to `appsettings.json`

The original had `ApiBaseUrl` as a hardcoded `const string`, which static analysis correctly flagged (`csharpsquid:S1075`, "Refactor your code not to use hardcoded absolute paths or URIs"), the same class of finding already fixed for `Samples.MvcWebApi.Core`'s CORS origins.

This project is a plain console app (`Microsoft.NET.Sdk`, not `Microsoft.NET.Sdk.Web`), so it doesn't get `appsettings.json` loading for free the way a `WebApplicationBuilder`-hosted project does. `Microsoft.Extensions.Configuration` + `Microsoft.Extensions.Configuration.Json` are added specifically to make a small, manually-built `ConfigurationBuilder().AddJsonFile("appsettings.json")` work in `Program.cs`, the same underlying mechanism `Samples.MvcWebApi.Core` gets automatically, just wired up by hand here since there's no web host doing it for this project.

---

## Try It Yourself

Run `Samples.MvcWebApi.Core` first, then this client. Compare its `Program.cs` against `Samples.MvcWebApi.Client`'s directly, same three operations, genuinely different amount of code required for each.
