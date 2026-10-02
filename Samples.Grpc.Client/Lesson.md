# Samples.Grpc.Client

## What This Is

A .NET console client consuming `Samples.Grpc`'s server-streaming RPC, printing each matching location as it arrives rather than waiting for a complete response. References `Samples.Grpc`'s own `.proto` file directly -- the exact same contract the server uses, no duplicated or hand-written copy.

---

## When to Use This Pattern

Whenever a .NET application needs to consume a gRPC service. The generated client stub handles all Protocol Buffers serialization, connection management, and streaming mechanics -- the calling code looks very close to a local method call.

---

## How It Works

The `.csproj` references `Samples.Grpc`'s proto file with `GrpcServices="Client"`:

```xml
<ItemGroup>
    <Protobuf Include="..\Samples.Grpc\Protos\locationlookup.proto" GrpcServices="Client" />
</ItemGroup>
```

This generates a client stub class at build time. Calling the service:

```csharp
var channel = GrpcChannel.ForAddress(grpcServerUrl);
var client  = new LocationLookup.LocationLookupClient(channel);

var call = client.LookupLocation(new LocationLookupRequest { ZipCode = zipCode });

await foreach (var location in call.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"  {location.City}, {location.County}, {location.State}");
}
```

`ReadAllAsync()` is an `IAsyncEnumerable<T>` -- results are processed as they stream in, not after all are received. Contrast this against `Samples.MvcWebApi.Core.Client`'s `GetFromJsonAsync`, which waits for the entire response before returning anything.

---

## Creating a gRPC Client

### Visual Studio

**File > New > Project**, "Console App", choose .NET version. Add the gRPC client packages:

```powershell
Install-Package Grpc.Net.Client
Install-Package Google.Protobuf
Install-Package Grpc.Tools
```

Add the `.proto` reference to the `.csproj`:

```xml
<ItemGroup>
    <Protobuf Include="..\MyGrpcService\Protos\myservice.proto" GrpcServices="Client" />
</ItemGroup>
```

Build the project -- the client stub is generated automatically.

### VS Code

```powershell
dotnet new console -n MyGrpcClient -f net10.0
dotnet add package Grpc.Net.Client
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
```

Add the `<Protobuf>` reference to the `.csproj` pointing at the server's `.proto` file, then `dotnet build` to generate the client stub.

---

## Running This Project

1. Start `Samples.Grpc` (F5 or `dotnet run`) and leave it running.
2. If the server isn't on its default port, update `appsettings.json`'s `GrpcServerUrl`.
3. Run this project and enter a ZIP code.

---

## Streaming vs. Single Response

`Samples.Grpc`'s `LookupLocation` is a **server-streaming RPC** -- the server sends multiple `LocationLookupReply` messages and the client receives each one as it arrives. Four RPC patterns exist in gRPC:

| Pattern | Direction | Use when |
|---|---|---|
| Unary | One request → one response | Simple request/reply |
| Server streaming | One request → many responses | Results available incrementally |
| Client streaming | Many requests → one response | Uploading a batch |
| Bidirectional streaming | Many requests ↔ many responses | Real-time, chat-style |

---

## Related Projects

- `Samples.Grpc` -- the gRPC service this client calls.
- `Samples.MvcWebApi.Core.Client` -- a .NET console client for a REST/JSON API, for comparison.
