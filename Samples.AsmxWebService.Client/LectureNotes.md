# Samples.AsmxWebService.Client

## What This Is

A console client consuming `Samples.AsmxWebService` via a genuinely generated proxy class (`WebService/ExampleWebService.cs`), produced by `wsdl.exe` reading the service's own WSDL document, not hand-written. This demonstrates the standard way .NET code has always consumed an ASMX (or any WSDL-publishing SOAP) service: generate a strongly-typed client class once, then call it like any other object.

---

## Porting Notes

Ported as an SDK-style console project (unlike `Samples.AsmxWebService` itself, a console app has no reason to stay legacy-style, SDK-style auto-includes files by wildcard and needs far less boilerplate). `System.Web.Services` (for `SoapHttpClientProtocol`, the generated proxy's base class) is referenced as a plain Framework assembly reference, works fine on net48 without any package.

`WebService/ExampleWebService.cs` and `WebService/ExampleWebService.wsdl` are both kept exactly as originally generated/downloaded, including the WSDL's hardcoded `https://localhost:44355/ExampleWebService.asmx` service address baked into `ExampleWebService()`'s constructor, this is genuinely how `wsdl.exe` output looks, worth seeing unmodified. The actual URL the client connects to at runtime is overridden immediately after construction (`new ExampleWebService { Url = settings.WebServiceUrl }`), reading from `App.config`, so the hardcoded constructor default only matters if that override is ever accidentally skipped.

No bugs found in the original code.

---

## Try It Yourself

Run `Samples.AsmxWebService` first (F5 in Visual Studio, IIS Express), then run this client, it walks through `Ping()`, `TestService()`, and `LookupLocation()` interactively, pausing after each so you can read the result before continuing.
