# Samples.WcfService

## What This Is

WCF (Windows Communication Foundation), Microsoft's second-generation .NET web service technology (2006), succeeding ASMX. The key WCF idea: separate a service's *contract* (what it does) from its *bindings* (how you reach it), so the same service code can be exposed multiple ways at once.

This project demonstrates exactly that: one contract (`IExampleWebService`), exposed through **two endpoints simultaneously**:

- `appEndpoint` (`basicHttpBinding`) -- a SOAP/WSDL endpoint consumed by `Samples.WcfService.Client` via `ChannelFactory<IExampleWebService>`.
- `webEndpoint` (`webHttpBinding`) -- a REST/JSON endpoint consumed by `Samples.WcfService.WebClient` via plain AJAX.

Same ZIP code lookup as every other Samples project, backed by the same `ZipCodes` table.

---

## When to Use WCF

More defensible than ASMX, but still not the first choice for new work. ASP.NET Core Web API (or gRPC for service-to-service) is the modern default. WCF remains reasonable for **classic .NET Framework applications that genuinely need multiple binding types from one contract** -- SOAP for a legacy integration, REST for a modern browser client, from the same service code. You'll encounter it in still-active classic .NET work more often than ASMX.

---

## How WCF Works

The contract is an interface decorated with `[ServiceContract]`/`[OperationContract]`. REST-friendly operations add `[WebGet]` (for GET) or `[WebInvoke]` (for POST/PUT/DELETE):

```csharp
[ServiceContract]
public interface IExampleWebService
{
    [OperationContract]
    PingResponse Ping();

    [OperationContract]
    [WebGet(UriTemplate = "/LookupLocationRest/{zipCode}", ResponseFormat = WebMessageFormat.Json)]
    LocationLookupResponse LookupLocationRest(string zipCode);
}
```

Bindings, endpoints, and behaviors are all configured in `Web.config`'s `<system.serviceModel>` section -- no code changes needed to add a new binding or change how the service is reached.

---

## Creating a WCF Service

### Visual Studio

**File > New > Project**, search "WCF Service Application". This template creates the `.svc` file, a starter interface, and the implementation class. The binding/endpoint configuration is generated into `Web.config` automatically. To add a second endpoint, edit the `<services>` section in `<system.serviceModel>` by hand.

### VS Code

WCF Service projects require the classic ASP.NET hosting model. Create the project skeleton in Visual Studio. VS Code can edit the implementation files, but the WCF-specific project type and `<system.serviceModel>` structure are most easily managed in Visual Studio.

---

## Running This Project

1. Point `Web.config`'s `<database>` element at a SQL Server instance with a `ZipCodes` table.
2. Press F5 (IIS Express).
3. Browse to `ExampleWebService.svc` for the service description page, or append `?singlewsdl` for the full WSDL.

---

## Pros and Cons

**Pros:** One contract, multiple bindings. Strongly-typed, contract-first design. Genuine SOAP interoperability alongside a REST option. Rich, externalized configuration.

**Cons:** Configuration-heavy (`<system.serviceModel>` is substantial). No longer actively developed. Doesn't run on modern .NET without CoreWCF, a community-maintained partial reimplementation.

---

## Related Projects

- `Samples.WcfService.Client` -- .NET console client via `ChannelFactory<T>` (SOAP endpoint).
- `Samples.WcfService.WebClient` -- browser client (REST/JSON endpoint).
- `Samples.AsmxWebService` -- the technology WCF succeeded.
