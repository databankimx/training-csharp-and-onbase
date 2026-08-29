# Samples.MvcWebApi.Core.Common

## What This Is

Shared request/response record types for `Samples.MvcWebApi.Core` (the API) and `Samples.MvcWebApi.Core.Client` (a .NET console client). ASP.NET Core Web API still has no built-in WSDL or data contract format, the exact same limitation classic Web API 2 has, so the exact same solution applies: a small shared library both the server and any .NET client reference directly, so the shapes can't silently drift apart. See `Samples.MvcWebApi.Common`'s own `README.md` for the fuller discussion of why this pattern exists.

---

## What's in This Project

Four `record` types (`Location`, `LocationLookupResponse`, `TestRequest`, `TestResponse`), immutable, value-equality data holders, the modern C# idiom for exactly this kind of shape, a real contrast worth noting against the classic `Samples.MvcWebApi.Common`'s plain mutable classes.
