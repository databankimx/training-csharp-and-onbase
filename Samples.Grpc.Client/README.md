# Samples.Grpc.Client

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

A console application consuming `Samples.Grpc`'s server-streaming RPC, printing each matching location as it arrives rather than waiting for a complete response.

```csharp
await foreach (var location in call.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"  - City: {location.City}");
}
```

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Connects to the server, calls `LookupLocation`, streams results to the console |
| `appsettings.json` | `GrpcServerUrl`, editable without a recompile |

References `Samples.Grpc`'s own `Protos/locationlookup.proto` directly (see this project's `.csproj`), the exact same contract file the server uses, no duplicated or hand-written copy.

---

## How to Run

1. Run `Samples.Grpc` first (F5 or `dotnet run`), and leave it running.
2. If it isn't running on its default port, update `appsettings.json`'s `GrpcServerUrl` to match.
3. Run this project, and enter a ZIP code to search.

---

## Related Samples

- **`Samples.Grpc`** — the gRPC service this client calls.
- **`Samples.MvcWebApi.Core.Client`** — a .NET console client for a conventional REST/JSON API, worth comparing directly.
