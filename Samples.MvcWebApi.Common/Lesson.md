# Samples.MvcWebApi.Common

## What This Is

The shared request/response DTOs for `Samples.MvcWebApi`, `Samples.MvcWebApi.Client`, and `Samples.MvcWebApi.WebClient`. A small library that exists specifically because of Web API's biggest limitation relative to WCF/ASMX: **no built-in contract format**. Nothing in Web API itself describes what shape a request or response takes. A shared assembly referenced by both server and client is the convention that fills that gap.

---

## When to Use This Pattern

Whenever a .NET client needs to consume a Web API (classic or ASP.NET Core) in a type-safe way. The shared library prevents the server and client from silently drifting into different shapes for the same model. Compare against `Samples.WcfService.Client`'s `svcutil`-generated proxy, which duplicates the service's contracts into a separate generated file instead -- Web API's lack of a contract format makes generation impossible, a shared library is the only real option.

---

## What's in This Project

- `ApiRequestBase` / `ApiResponseBase` -- common `Id` (request correlation) and `Errors` shape.
- `TestRequest` / `TestResponse`, `LocationRequest` / `LocationResponse`, `Location` -- the concrete shapes for this sample's two real operations.

Plain mutable classes with no logic -- worth contrasting against `Samples.MvcWebApi.Core.Common`, which uses immutable C# `record` types for the same purpose.

---

## Creating a Similar Project

### Visual Studio

**File > New > Project**, select "Class Library (.NET Framework)". Add a project reference to it from both the API project and any .NET client. Only `record`/class definitions go here -- no EF, no DI, no service code.

### VS Code

```powershell
dotnet new classlib -n MyApi.Common -f net48
```

Add a project reference from each consumer:

```powershell
dotnet add ../MyApi/MyApi.csproj reference MyApi.Common.csproj
dotnet add ../MyApiClient/MyApiClient.csproj reference MyApi.Common.csproj
```

---

## Related Projects

- `Samples.MvcWebApi` -- the API that uses these types.
- `Samples.MvcWebApi.Client` -- the .NET client that references this library.
- `Samples.MvcWebApi.Core.Common` -- the modern equivalent using `record` types.
