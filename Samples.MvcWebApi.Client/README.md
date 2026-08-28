# Samples.MvcWebApi.Client

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

A console application consuming `Samples.MvcWebApi` the old-fashioned way: raw `HttpWebRequest`, manual JSON serialization via `JavaScriptSerializer`, no `HttpClient`, no `async`/`await`. Kept deliberately, since Web API has no WSDL or contract format the way WCF/ASMX do, this client instead references `Samples.MvcWebApi.Common` directly for the shared request/response shapes.

```csharp
var request = (HttpWebRequest)WebRequest.Create($"{WebApiUrl}{method}");
request.Accept = "application/json";
request.Method = "POST";
// ... write JSON payload to request.GetRequestStream() ...
var response = (HttpWebResponse)request.GetResponse();
string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
```

---

## When to Use This Pattern

Genuinely only in legacy code you're maintaining, or as a teaching example of what pre-`HttpClient` .NET networking code looked like. Any new client consuming a REST API should use `HttpClient` with `async`/`await` instead, dramatically less boilerplate, proper cancellation support, and no risk of the thread-blocking behavior raw `HttpWebRequest` calls like this one exhibit.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Walks through `Ping`, `Test`, and `LocationLookup`, printing raw JSON at each step |

Depends on `Samples.MvcWebApi.Common` for the `TestRequest`/`TestResponse`/`LocationRequest`/`LocationResponse` shapes.

---

## How to Run

1. Run `Samples.MvcWebApi` first (F5 in Visual Studio, IIS Express), and leave it running.
2. Run this project. It calls each of the three API methods in turn, showing the exact JSON sent and received.

---

## Related Samples

- **`Samples.MvcWebApi`** — the Web API this client consumes.
- **`Samples.MvcWebApi.Common`** — the shared DTOs this client (and the API) both reference.
- **`Samples.MvcWebApi.WebClient`** — a browser-based client calling the same API.
