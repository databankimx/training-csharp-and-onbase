# Samples.WcfService

## What This Is

A real, deployable WCF (Windows Communication Foundation) service exposing the *same* contract (`IExampleWebService`, five operations) through **two different endpoints simultaneously**: `appEndpoint` (`basicHttpBinding`, a genuine SOAP/WSDL endpoint for .NET clients) and `webEndpoint` (`webHttpBinding`, a REST/JSON endpoint for browser AJAX calls). This dual-endpoint pattern is worth studying closely, it's the single biggest thing distinguishing WCF from ASMX: one service definition, multiple ways to reach it.

---

## Porting Notes

Same treatment as `Samples.AsmxWebService`: kept as a legacy-style Web Application Project (required for the `.svc` handler and IIS Express), `packages.config` replaced with `PackageReference` (eliminating the same class of unnecessary transitive-dependency bloat, BouncyCastle, Google.Protobuf, K4os, ZstdNet, etc.), log4net replaced with Serilog (sink configuration in `serilog.json`, minimum level still driven from `Web.config`'s `debugMode` setting via a `LoggingLevelSwitch`), and hardcoded test credentials genericized in `Web.config`.

**Binding redirects handled proactively this time.** This project's package list and exact versions are identical to `Samples.AsmxWebService`'s, so rather than waiting to hit the same `FileLoadException` and regenerate the answer from scratch, the exact redirects already validated there were copied directly into this project's `Web.config` up front. Worth double-checking against the generated `Samples.WcfService.dll.config` after the first build regardless, dependency resolution can differ slightly even between projects with nominally identical package references.

---

## A Real Bug Fixed: Unreachable Code After `Response.End()`

```csharp
protected void Application_BeginRequest(object sender, EventArgs e)
{
    ...
    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
    HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
    HttpContext.Current.Response.End();
    // Needed if service will be called from OnBase Foundation EP3+
    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, obtoken");
}
```

`Response.End()` throws internally (`ThreadAbortException`) to halt request processing immediately, a well-documented, deliberate ASP.NET behavior. That means the final `AddHeader` call, explicitly commented as "needed if service will be called from OnBase Foundation EP3+", was genuine dead code, it could never execute. Anyone actually integrating this service with OnBase Foundation EP3+ and relying on that comment would have found the `obtoken` header silently missing from every OPTIONS response. **Fixed** by moving that header value above `Response.End()` (and consolidating it with the earlier, now-redundant `AddHeader` call for the same header name, rather than sending the header twice with different values).

---

## The WSDL, Verified Against the Actual Ported Contract

Unlike `Samples.AsmxWebService.Client`'s original proxy (which turned out to be stale, generated against an earlier version of that service with two extra operations that no longer existed), this service's `IExampleWebService.cs` contract and `Samples.WcfService.Client`'s generated proxy/WSDL were checked operation-by-operation against each other before reuse: both list exactly five operations (`Ping`, `TestService`, `TestServiceRest`, `LookupLocation`, `LookupLocationRest`), and since this project ports the contract unchanged, there's no drift, unlike the ASMX case. See `Samples.WcfService.Client`'s own `LectureNotes.md` for the full comparison.

---

## `DatabankException` Replaces `ApplicationException`

Every `throw new ApplicationException(...)` in this project (`ExampleWebService.svc.cs`, `HelperClasses/Database.cs`, `Models/Configuration/DatabaseSettings.cs`, `Models/Objects/LocationLookupRequest.cs`) was replaced with `CSharp.SharedLibrary.Models.DatabankException`, DataBank's own exception type, and now a standard for every `Samples.*` project going forward (already applied to `Samples.AsmxWebService` as well). `DatabankException(string message, Exception innerException = null)` matches `ApplicationException`'s own constructor exactly, so this was a direct, safe swap, no call-site logic changed. Required adding a `<ProjectReference>` to `CSharp.SharedLibrary` in the `.csproj`.

---

## Try It Yourself

Point `Web.config`'s `<database>` element at a real SQL Server instance with a `ZipCodes` table, then run the project (F5, IIS Express). Browse to `ExampleWebService.svc` to see WCF's own service description page, then `ExampleWebService.svc?singlewsdl` for the full WSDL (including all data contracts, unlike plain `?wsdl`).
