# Samples.Grpc

## What This Is

A gRPC service, ASP.NET Core-hosted, defined by a `.proto` contract file that generates C# code at build time. It looks up city/county/state by ZIP code -- the same task every other Samples project performs -- but **streams** each matching result back to the caller as it's found rather than collecting them all first.

gRPC is the closest thing in this training set to WCF's contract-first approach: the `.proto` file is the authoritative schema, and both server and client code are generated from it. No hand-written DTOs, no shared library convention.

**No `net48` sibling.** gRPC server hosting requires HTTP/2, and classic ASP.NET/IIS on `net48` has no well-supported path for that.

---

## When to Use gRPC

For **service-to-service communication** (internal APIs, microservices) where:

- A strongly-typed, schema-enforced contract matters.
- Performance is a priority -- Protocol Buffers binary serialization is faster and smaller than JSON.
- Streaming (server-to-client, client-to-server, or bidirectional) is needed.

Less suitable for:
- Public APIs where REST/JSON's human-readability and universal tooling matter.
- Browser clients -- browsers can't call raw gRPC directly without `grpc-web`, a separate compatibility layer.

---

## How gRPC Works

The contract lives in `Protos/locationlookup.proto`:

```protobuf
syntax = "proto3";

service LocationLookup {
    rpc LookupLocation (LocationLookupRequest) returns (stream LocationLookupReply);
}

message LocationLookupRequest { string zip_code = 1; }
message LocationLookupReply { string city = 1; string county = 2; string state = 3; }
```

The `.csproj` references the `.proto` file and MSBuild generates both server base classes and client stub code from it automatically at build time:

```xml
<ItemGroup>
    <Protobuf Include="Protos\locationlookup.proto" GrpcServices="Server" />
</ItemGroup>
```

The service implementation overrides the generated base class:

```csharp
public class LocationLookupServiceImpl(LocationLookupContext db)
    : LocationLookup.LocationLookupBase
{
    public override async Task LookupLocation(
        LocationLookupRequest request,
        IServerStreamWriter<LocationLookupReply> responseStream,
        ServerCallContext context)
    {
        var results = db.ZipCodes.Where(z => z.ZipCode1 == request.ZipCode);
        await foreach (var row in results.AsAsyncEnumerable())
        {
            await responseStream.WriteAsync(new LocationLookupReply
            {
                City = row.City, County = row.County, State = row.State
            });
        }
    }
}
```

Each matching row is streamed individually. The client receives and prints each one as it arrives -- it doesn't wait for all results before seeing any.

---

## Creating a gRPC Service

### Visual Studio

**File > New > Project**, search "ASP.NET Core gRPC Service", click Next, choose .NET version, click Create. The scaffolding creates `Protos/greet.proto`, a `GreeterService.cs`, and `Program.cs` with `app.MapGrpcService<GreeterService>()` wired.

Replace `greet.proto` with your own contract. Add EF Core via Manage NuGet Packages, register the `DbContext` in `Program.cs`.

### VS Code

```powershell
dotnet new grpc -n MyGrpcService -f net10.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Grpc.AspNetCore
```

Edit `Protos/greet.proto` to define your contract. The `<Protobuf>` MSBuild item in the `.csproj` triggers code generation on build. Run with `dotnet run`.

---

## Running This Project

1. Point `appsettings.json`'s `LocationLookupDatabase` at a SQL Server instance.
2. Press F5 or `dotnet run`. No browser opens -- gRPC services aren't callable from a browser directly.
3. Run `Samples.Grpc.Client` to make a real call.

---

## gRPC vs. REST

| | gRPC | REST/JSON |
|---|---|---|
| Contract | `.proto` file (authoritative schema) | Shared library convention or OpenAPI spec |
| Serialization | Protocol Buffers (binary) | JSON (text) |
| Streaming | First-class (4 patterns) | Requires SSE or WebSockets |
| Browser support | Needs `grpc-web` layer | Native |
| Tooling | Specialized (`grpcurl`, Postman with plugin) | Universal |

---

## Related Projects

- `Samples.Grpc.Client` -- the .NET console client for this service.
- `Samples.MvcWebApi.Core` -- a REST/JSON API performing the same lookup, for direct comparison.
- `Samples.WcfService` -- WCF's WSDL-based contract-first approach, the closest classic equivalent.
