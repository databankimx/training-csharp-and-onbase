# Samples.Grpc

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port, gRPC server hosting has no `net48` equivalent). See `README.md` for the fuller when-to-use discussion.

---

## The `.proto` File Is the Contract, Generated at Build Time

```protobuf
service LocationLookupService {
  rpc LookupLocation (ZipCodeRequest) returns (stream LocationReply);
}
```

`Protos/locationlookup.proto` is the single source of truth for this service's shape, the closest thing gRPC has to `Samples.WcfService`'s WSDL. `LocationLookupService.LocationLookupServiceBase` (the class `Services/LocationLookupServiceImpl.cs` overrides), `ZipCodeRequest`, and `LocationReply` are all **generated at build time** from this file, not written by hand, the same general idea as an EF6 EDMX generating `LocationLookupModel.Context.cs` elsewhere in this training set, code generation from a declarative source rather than hand-maintained code. `Samples.Grpc.Client` references this exact same `.proto` file directly (see its own `.csproj`), so there is only ever one copy of the contract to keep in sync, genuinely stronger than `Samples.MvcWebApi`'s shared-library convention (`Samples.MvcWebApi.Common`), which still relies on both sides remembering to actually reference it.

---

## Server Streaming: gRPC's Actual Signature Capability

```csharp
public override async Task LookupLocation(ZipCodeRequest request, IServerStreamWriter<LocationReply> responseStream, ServerCallContext context)
{
    await foreach (var zipCode in query.WithCancellation(context.CancellationToken))
    {
        await responseStream.WriteAsync(new LocationReply { ... });
    }
}
```

The `stream` keyword in the `.proto` file is what makes this a **server-streaming** RPC rather than a plain request/response. Each `LocationReply` is written to `responseStream` and sent to the client **as soon as it's ready**, over the same HTTP/2 connection, rather than being collected into one list and returned all at once the way `Samples.MvcWebApi.Core`'s `LocationLookupController` (which returns a single `LocationLookupResponse` containing every match) has to. For a single ZIP code lookup this distinction barely matters in practice (there's usually only one or two matching rows), but the pattern itself scales to genuinely large result sets without ever buffering the whole response in memory on either side, worth understanding even though this particular sample's dataset is small. gRPC also supports client-streaming and full bidirectional streaming, this sample demonstrates only the server-streaming direction, since it maps naturally onto a "look up and return matches" task.

---

## `IDbContextFactory<T>`, Same Reasoning as `Samples.Blazor.Server`

`LocationLookupServiceImpl` is registered per-call by the gRPC framework, but its dependencies come from the same DI container as everything else in this project, and `AddDbContextFactory` + `CreateDbContextAsync()` is used here for the same reason it's used in `Samples.Blazor.Server`: a fresh, short-lived `DbContext` per unit of work, rather than one shared across however long the underlying registration's lifetime happens to be.

---

## No Browser, No Swagger

Unlike `Samples.MvcWebApi.Core`, there's no interactive documentation UI to browse to here, `Program.cs`'s root `/` route just returns a plain string explaining that this endpoint needs a real gRPC client. gRPC's binary, HTTP/2-framed protocol isn't something a browser (or a tool like Postman, without gRPC-specific support) can casually poke at the way a REST/JSON endpoint can, worth knowing as a genuine trade-off of the technology, not a limitation of this sample.

---

## Try It Yourself

Run this project, then run `Samples.Grpc.Client` and watch it print each matching location as it streams in. Then compare `Services/LocationLookupServiceImpl.cs` directly against `Samples.MvcWebApi.Core`'s `LocationLookupController`, same underlying EF Core query, genuinely different response shape and delivery mechanism.
