# Samples.Grpc

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

A gRPC service, ASP.NET Core-hosted, defined by a `.proto` file (`Protos/locationlookup.proto`), the closest thing gRPC has to `Samples.WcfService`'s WSDL, a strongly-typed contract generated into C# at build time rather than hand-written DTOs. It looks up city/county/state by ZIP code, the same task every other sample in this training set performs, but **streams** each matching result back to the caller as it's found, rather than collecting them into one response first.

**No `net48` sibling exists.** gRPC server hosting genuinely requires HTTP/2, and classic ASP.NET/IIS on `net48` has no well-supported story for that (the old `Grpc.Core` native-library implementation that once made this possible is itself deprecated), the same class of exception as `Samples.RazorPages`/`Samples.Blazor`.

`Samples.Grpc.Client` is a .NET console client for this service.

---

## When to Use gRPC

For service-to-service communication (internal APIs, microservices) where performance, a strongly-typed contract, and/or streaming matter more than broad client compatibility. Less suitable for anything a browser needs to call directly (browsers can't easily speak raw gRPC without `grpc-web`, a separate compatibility layer) or for public APIs where REST/JSON's ubiquity and human-readability matter more.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Protos/locationlookup.proto` | The service contract, generates both server and client code |
| `Services/LocationLookupServiceImpl.cs` | The actual implementation, a server-streaming RPC |
| `Models/ZipCode.cs` | EF Core entity (Code-First) |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |

---

## How to Run

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Press F5 (or `dotnet run`). No browser opens, a gRPC service isn't something a browser can call directly, use `Samples.Grpc.Client` instead.

---

## Related Samples

- **`Samples.Grpc.Client`** — a .NET console client for this service.
- **`Samples.MvcWebApi.Core`** — a REST/JSON API performing the same lookup, worth comparing the contract-first, streaming-capable gRPC approach against a conventional HTTP API directly.
