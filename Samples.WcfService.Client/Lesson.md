# Samples.WcfService.Client

## What This Is

A .NET console client consuming `Samples.WcfService`'s SOAP endpoint (`appEndpoint`) via `ChannelFactory<IExampleWebService>` -- a common, clean WCF client pattern that avoids needing a separately generated wrapper class.

---

## When to Use This Pattern

Whenever consuming a WCF SOAP endpoint from another .NET application. `ChannelFactory<T>` works when you have access to the contract interface (either from a shared assembly or from a `svcutil`-generated file). It's arguably cleaner than ASMX's approach because the same interface describes both what the server offers and what the client calls.

---

## How It Works

```csharp
var binding = new BasicHttpBinding();
var address = new EndpointAddress(settings.WebServiceUrl);
var factory  = new ChannelFactory<IExampleWebService>(binding, address);
var channel  = factory.CreateChannel();

var result = channel.Ping();
var location = channel.LookupLocation(new LocationLookupRequest { ZipCode = "75067" });
```

`CreateChannel()` returns a live proxy that implements `IExampleWebService` directly. No wrapper class needed. The contract interface (and its DTOs) can come from a shared assembly reference or from a `svcutil`-generated file when you don't control the server's source.

`WebService/ExampleWebService.cs` contains a `svcutil`-generated equivalent, kept for reference. The project itself uses `ChannelFactory` directly.

---

## Creating a Similar Project

### Visual Studio

**File > New > Project**, select "Console App (.NET Framework)". To generate a `svcutil` proxy from a live service: **Project > Add Service Reference**, point at the `.svc?wsdl` URL, and let Visual Studio generate the proxy. To use `ChannelFactory` instead: reference a shared assembly containing the contract interface, add `System.ServiceModel` to the references, and write the `ChannelFactory` code directly.

### VS Code

```powershell
dotnet new console -n MyWcfClient -f net48
```

Add `System.ServiceModel` via NuGet:

```powershell
dotnet add package System.ServiceModel.Http
```

Reference the shared contract assembly (or copy in the `svcutil`-generated file), then write the `ChannelFactory` code.

---

## Running This Project

1. Start `Samples.WcfService` (F5 in Visual Studio) and leave it running.
2. Run this project. It calls each operation through the SOAP endpoint.

---

## Related Projects

- `Samples.WcfService` -- the service this client consumes.
- `Samples.WcfService.WebClient` -- browser client calling the REST endpoint on the same service.
