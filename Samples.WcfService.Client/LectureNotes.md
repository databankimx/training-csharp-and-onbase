# Samples.WcfService.Client

## What This Is

A console client consuming `Samples.WcfService` through its `appEndpoint` (`basicHttpBinding`, SOAP), using `ChannelFactory<IExampleWebService>` rather than a wrapper class, a genuinely common, arguably cleaner WCF client pattern than ASMX's generated-proxy-class approach: the channel factory produces a live proxy implementing the contract interface directly, method calls on it are indistinguishable from calling the interface locally.

---

## Verifying the WSDL Before Reuse

Given the mismatch found in `Samples.AsmxWebService.Client` (a stale proxy generated against an earlier, five-operation version of a service that had since been trimmed to three), the same check was applied here before reusing anything: `WebService\ExampleWebService.cs` (the `svcutil`-generated proxy, defining `IExampleWebService` and every DTO) and `WebService\ExampleWebService.wsdl` were compared operation-by-operation against `Samples.WcfService`'s actual `IExampleWebService.cs`.

Result: **all five operations match exactly** (`Ping`, `TestService`, `TestServiceRest`, `LookupLocation`, `LookupLocationRest`), on both sides. Since `Samples.WcfService` ports that interface unchanged, there's no possibility of the drift that happened with ASMX, the operations in this WSDL are genuinely correct to reuse.

**The address itself was wrong, though.** A real WSDL pulled directly from the running service reported `http://localhost:39417/ExampleWebService.svc`, plain HTTP on IIS Express's auto-assigned HTTP port, not the `https://localhost:44357` (the SSL port) originally assumed here based on the `.csproj`'s `<IISUrl>` setting. IIS Express serves both protocols, but evidently the service's own metadata endpoint reports whichever address it was actually reached through, and the HTTP port is what's genuinely live by default. **Fixed**: `App.config`'s `webServiceUrl` and the WSDL's own `<soap:address>` were both updated to `http://localhost:39417/ExampleWebService.svc`, and the WSDL file itself was replaced wholesale with the exact bytes pulled from the real service rather than hand-edited, to guarantee accuracy. `Program.cs`'s existing `if (settings.WebServiceUrl.ToLower().StartsWith("https"))` check already handles a plain-HTTP URL correctly (it simply skips setting `BasicHttpSecurityMode.Transport`), so no code changes were needed, only configuration.

---

## Porting Notes

SDK-style console project (a console app has no reason to stay legacy-style the way a web project does). `System.ServiceModel` (for `ChannelFactory`/`BasicHttpBinding`) referenced as a plain Framework assembly, works fine on net48 without any package. No bugs found in `Program.cs` itself.

Every `throw new ApplicationException(...)` in `Program.cs` was replaced with `CSharp.SharedLibrary.Models.DatabankException`, matching the same swap made in `Samples.WcfService` and now a standard for every `Samples.*` project going forward. `WebService\ExampleWebService.cs`, being genuine `svcutil`-generated auto-generated code (not something DataBank wrote by hand), was deliberately left untouched, it wouldn't reference `DatabankException` in real generated output either.

---

## Try It Yourself

Run `Samples.WcfService` first (F5, IIS Express), then run this client. It walks through `Ping()`, `TestService()`, and `LookupLocation()` interactively, calling each through the `appEndpoint`'s genuine SOAP binding.
