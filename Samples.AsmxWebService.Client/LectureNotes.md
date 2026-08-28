# Samples.AsmxWebService.Client

## What This Is

A console client consuming `Samples.AsmxWebService` via a genuinely generated proxy class (`WebService/ExampleWebService.cs`), produced by `wsdl.exe` reading the service's own WSDL document, not hand-written. This demonstrates the standard way .NET code has always consumed an ASMX (or any WSDL-publishing SOAP) service: generate a strongly-typed client class once, then call it like any other object.

---

## Porting Notes

Ported as an SDK-style console project (unlike `Samples.AsmxWebService` itself, a console app has no reason to stay legacy-style, SDK-style auto-includes files by wildcard and needs far less boilerplate). `System.Web.Services` (for `SoapHttpClientProtocol`, the generated proxy's base class) is referenced as a plain Framework assembly reference, works fine on net48 without any package.

`WebService/ExampleWebService.cs` and `WebService/ExampleWebService.wsdl` are both kept exactly as originally generated/downloaded, including the WSDL's hardcoded `https://localhost:44355/ExampleWebService.asmx` service address baked into `ExampleWebService()`'s constructor, this is genuinely how `wsdl.exe` output looks, worth seeing unmodified. The actual URL the client connects to at runtime is overridden immediately after construction (`new ExampleWebService { Url = settings.WebServiceUrl }`), reading from `App.config`, so the hardcoded constructor default only matters if that override is ever accidentally skipped.

---

## A Real Mismatch Found: the Proxy Was Stale Against the Actual Service

The original downloaded proxy (`WebService/ExampleWebService.cs`, generated from `WebService/ExampleWebService.wsdl`) exposed **five** operations: `Ping`, `TestService`, `TestServiceRest`, `LookupLocation`, and `LookupLocationRest`. The actual `Samples.AsmxWebService` project this client targets, the version genuinely migrated and running, only implements **three**: `Ping`, `TestService`, and `LookupLocation`. The two REST-suffixed operations must have existed in an earlier version of the service the original proxy was generated against, and were simply never regenerated when that service changed.

This didn't break anything at compile time (the extra methods just sat there, unused, since `Program.cs` never called them) or even necessarily at runtime for the methods this client actually calls, but it meant the "genuine `wsdl.exe` output" this project is meant to demonstrate was quietly describing a service that no longer exists. **Fixed** by regenerating both `WebService/ExampleWebService.wsdl` and `WebService/ExampleWebService.cs` against the real WSDL pulled directly from the running service (`https://localhost:44355/ExampleWebService.asmx?wsdl`), removing `TestServiceRest`/`LookupLocationRest` and their supporting types, delegates, and event-arg classes entirely. Worth taking as a general lesson: a generated proxy is a snapshot of the service's contract at generation time, not a live reflection of it, if the service changes, the proxy has to be regenerated, `wsdl.exe` won't warn you that it's gone stale.

---

## Try It Yourself

Run `Samples.AsmxWebService` first (F5 in Visual Studio, IIS Express), then run this client, it walks through `Ping()`, `TestService()`, and `LookupLocation()` interactively, pausing after each so you can read the result before continuing.
