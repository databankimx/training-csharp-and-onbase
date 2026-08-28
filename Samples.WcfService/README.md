# Samples.WcfService

> **Looking for implementation details, bugs found, or migration notes?** See `LectureNotes.md` in this folder.

## What This Is

WCF (Windows Communication Foundation), Microsoft's second-generation .NET web service technology (2006), succeeding ASMX. Where ASMX is SOAP-only, WCF separates a service's *contract* (what it does) from its *bindings* (how you reach it), letting the same service definition be exposed multiple ways at once.

This sample demonstrates exactly that: one contract (`IExampleWebService`, three real operations plus two REST-friendly variants), exposed through **two endpoints simultaneously**:

- **`appEndpoint`** (`basicHttpBinding`) — a genuine SOAP/WSDL endpoint, consumed by `Samples.WcfService.Client` (a .NET console app) via `ChannelFactory<IExampleWebService>`.
- **`webEndpoint`** (`webHttpBinding`) — a REST/JSON endpoint, consumed by `Samples.WcfService.WebClient` (a browser page) via plain AJAX.

---

## When to Use WCF

More defensible than ASMX, but still not the first choice for new work. ASP.NET Core Web API (or gRPC for service-to-service calls) is the modern default. That said, WCF remains genuinely reasonable for classic .NET Framework applications that need **multiple binding types from one contract** (SOAP for legacy integrations, REST for a modern front-end, all from the same service code), something ASP.NET Core doesn't replicate quite as directly. You'll encounter WCF far more often than ASMX in still-active (not just legacy-maintenance) classic .NET Framework work.

---

## Pros

- **One contract, multiple bindings.** Add a new binding in `Web.config` (say, a message-queue transport) without touching the service implementation at all.
- **Strongly-typed, contract-first design.** `[ServiceContract]`/`[OperationContract]` make the shape of the service explicit and enforceable at compile time, not just convention.
- **Genuine SOAP interoperability where you need it**, alongside a REST/JSON option where you don't, from the same codebase.
- **Rich configuration**: message size limits, timeouts, security modes, all externalized to `Web.config` rather than baked into code.

## Cons

- **Configuration-heavy.** The `<system.serviceModel>` section in this project's `Web.config` is substantial, bindings, behaviors, services, and endpoints all need to line up correctly, and small mismatches produce unhelpful runtime errors.
- **No longer actively developed.** WCF has been in maintenance mode for years; ASP.NET Core's own service model (Web API, gRPC, minimal APIs) is where Microsoft's investment goes now.
- **Doesn't run on modern .NET** without CoreWCF (a community-maintained, partial reimplementation), classic WCF is tied to .NET Framework.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `IExampleWebService.cs` | The service contract, `[ServiceContract]`/`[OperationContract]`/`[WebGet]`/`[WebInvoke]` |
| `ExampleWebService.svc` / `.svc.cs` | The service implementation |
| `Global.asax` / `.cs` | CORS headers for the REST endpoint, TLS 1.2 enforcement |
| `Models/Objects/` | Request/response data contracts |
| `HelperClasses/Database.cs` | Multi-backend database query logic (same pattern as `Samples.AsmxWebService`) |
| `serilog.json` | Log sink configuration |
| `Web.config` | Bindings, behaviors, endpoints, database settings, binding redirects |

---

## How to Run

1. Point `Web.config`'s `<database>` element at a real SQL Server instance with a `ZipCodes` table.
2. Press F5 (IIS Express).
3. Browse to `ExampleWebService.svc` for WCF's own service description page, or `ExampleWebService.svc?singlewsdl` for the full WSDL.

---

## Related Samples

- **`Samples.WcfService.Client`** — .NET console client using `ChannelFactory<IExampleWebService>` against the SOAP endpoint.
- **`Samples.WcfService.WebClient`** — browser-based client calling the REST/JSON endpoint directly.
- **`Samples.AsmxWebService`** — the technology WCF succeeded, worth comparing directly.
