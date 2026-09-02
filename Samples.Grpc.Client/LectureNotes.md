# Samples.Grpc.Client

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port, `net10.0`, matching `Samples.Grpc`). See `README.md` for the fuller how-to-run discussion.

---

## One Contract, Referenced Directly, Not Duplicated

```xml
<Protobuf Include="..\Samples.Grpc\Protos\locationlookup.proto" GrpcServices="Client" Link="Protos\locationlookup.proto" />
```

This project's `.csproj` points directly at `Samples.Grpc`'s own `.proto` file, rather than a copy. `GrpcServices="Client"` tells the code generator to produce only the client stub (`LocationLookupService.LocationLookupServiceClient`) from it, not the server base class `Samples.Grpc` itself generates from the identical file. There is exactly one `.proto` file in the entire solution, both projects build against it directly, genuinely stronger than `Samples.MvcWebApi`'s shared-library convention (`Samples.MvcWebApi.Common`), which still depends on both sides remembering to reference the shared project, not on there being only one file to begin with.

---

## Consuming a Server Stream

```csharp
using var call = client.LookupLocation(new ZipCodeRequest { ZipCode = zipCode });

await foreach (var location in call.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"  - City: {location.City}");
}
```

`client.LookupLocation(...)` returns immediately with an open stream, it does **not** block until the server has finished sending every result. `call.ResponseStream.ReadAllAsync()` is an `IAsyncEnumerable<LocationReply>`, and the `await foreach` loop processes each `LocationReply` as it arrives, genuinely different from `Samples.MvcWebApi.Core.Client`'s `HttpClient.GetFromJsonAsync<T>()`, which can't return anything at all until the complete JSON response has been received and deserialized. For this sample's typically single-row results, the practical difference is negligible, the same code would behave identically if the server were streaming thousands of rows, nothing here would need to change to handle that.

---

## Try It Yourself

Run `Samples.Grpc` first, then this project, and search a ZIP code. Then open `Samples.Grpc/Services/LocationLookupServiceImpl.cs` alongside this project's `Program.cs`, one writes to `responseStream`, the other reads from `call.ResponseStream`, the same generated contract on both ends.
