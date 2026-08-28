# Samples.MvcWebApi.Common

## What This Is

The shared request/response DTOs used by `Samples.MvcWebApi` (the API), `Samples.MvcWebApi.Client` (a .NET console client), and `Samples.MvcWebApi.WebClient` (a browser client). This project exists specifically because of Web API's biggest limitation relative to WCF or ASMX: **it has no built-in data contract or WSDL**, nothing tells a consumer what shape a request or response actually is. The convention that fills that gap is a shared assembly like this one, referenced by both the API and every .NET client, so the shapes genuinely can't drift apart.

---

## Porting Notes

SDK-style class library, no bugs found, no changes beyond the standard modernization (copyright header year, `#region` conventions matching this training set). Kept as a plain shared library, no `DatabankException` usage here since these are pure data-holding classes with no logic to throw from.

---

## How This Fits Together

- **`ApiRequestBase`** / **`ApiResponseBase`** — the common `Id` (request correlation) and `Errors` shape every request/response follows.
- **`TestRequest`** / **`TestResponse`**, **`LocationRequest`** / **`LocationResponse`**, **`Location`** — the concrete shapes for this sample API's two real operations.

Every project in this group (server and both clients) references this assembly directly rather than each redefining its own copy of these classes, worth contrasting against `Samples.WcfService.Client`'s `svcutil`-generated proxy, which duplicates the service's data contracts into a separate, generated file instead. Web API's lack of a contract format makes that generation-based approach impossible, a shared library is the only real option.
