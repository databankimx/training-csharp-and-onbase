# Samples.WcfService.Client

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

A console application consuming `Samples.WcfService`'s SOAP endpoint (`appEndpoint`), using `ChannelFactory<IExampleWebService>`, a genuinely common WCF client pattern:

```csharp
var binding = new BasicHttpBinding();
var address = new EndpointAddress(settings.WebServiceUrl);
var factory = new ChannelFactory<IExampleWebService>(binding, address);
var channel = factory.CreateChannel();

var result = channel.Ping();   // calls the real service, looks like a local call
```

No wrapper client class needed, `CreateChannel()` produces a live proxy that implements the contract interface directly. This is arguably cleaner than ASMX's approach (a separately generated client class with its own method signatures mirroring the service), since the same interface (`IExampleWebService`) describes both what the service *offers* and what the client *calls*.

---

## When to Use This Pattern

Whenever consuming a WCF SOAP endpoint from another .NET application. The interface (and its DTOs) can come from a shared assembly reference (as here, both projects live in the same solution) or from a `svcutil`-generated proxy against a service you don't control the source of.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Walks through `Ping()`, `TestService()`, and `LookupLocation()` interactively |
| `WebService/ExampleWebService.cs` | The `svcutil`-generated contract interface, DTOs, and client wrapper class (kept for reference; this project actually uses `ChannelFactory` directly) |
| `WebService/ExampleWebService.wsdl` | The WSDL the proxy was generated from, verified operation-by-operation against the actual service before reuse |
| `Models/Configuration/WebServiceSettings.cs` | Strongly-typed `App.config` section (the service URL) |
| `App.config` | Where the actual service URL is configured |

---

## How to Run

1. Run `Samples.WcfService` first (F5 in Visual Studio, IIS Express), and leave it running.
2. Run this project. It calls each of the three service methods through the SOAP endpoint, pausing after each so you can read the result before continuing.

---

## Related Samples

- **`Samples.WcfService`** — the WCF service this client consumes.
- **`Samples.WcfService.WebClient`** — a browser-based client calling the *other* endpoint (`webEndpoint`, REST/JSON) on the same service, worth comparing directly since both consume the exact same underlying contract two different ways.
