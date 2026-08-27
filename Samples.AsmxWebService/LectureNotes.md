# Samples.AsmxWebService

## What This Is

A real, deployable ASMX (SOAP) web service demonstrating the original .NET web service technology, kept in this training set specifically because you'll encounter ASMX services in support and maintenance work even though you should never build new ones. Exposes three methods (`Ping`, `TestService`, `LookupLocation`) over a switchable multi-database backend (SQL Server, Oracle, MySQL, or ODBC).

---

## What Modernized, and What Deliberately Didn't

Classic ASP.NET (`System.Web`-based) projects have no official SDK-style support, `Microsoft.NET.Sdk.Web` only targets ASP.NET Core, so this stays a legacy-style Web Application project, that's what makes the `.asmx` handler and IIS Express integration work at all.

What *did* modernize: `packages.config` is gone. Every package is now a `<PackageReference>` instead, and MSBuild resolves transitive dependencies automatically. The original `packages.config` had to hand-list BouncyCastle, Google.Protobuf, three separate K4os packages, ZstdNet, Ubiety.Dns.Core, System.Text.Json, System.Memory, System.Buffers, and several more, none of which this project's own code ever references directly, they're all transitive dependencies of `MySql.Data` that the legacy package-management format forces you to enumerate by hand. With `PackageReference`, none of that needs to appear in the project file at all; only the packages actually used directly (Serilog, MySql.Data, Oracle.ManagedDataAccess, the Roslyn compiler package) are listed now.

The multi-database switch itself (`Database.cs` branches on `SqlServer`/`Oracle`/`MySql`/`Odbc`) is genuine, intentional functionality, not bloat, and was kept as-is.

**A real gotcha hit while testing, in two parts**: running the service threw `FileLoadException` ("The located assembly's manifest definition does not match the assembly reference") for Serilog specifically.

**Part one**: `<OutputType>Library</OutputType>` (correct for a web project) normally *suppresses* MSBuild's binding-redirect generation, even with `AutoGenerateBindingRedirects=true` (already set solution-wide in `Directory.Build.props`), on the assumption that a class library doesn't have its own config file to redirect in. Adding `<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>` forces MSBuild to compute the redirects anyway.

**Part two, the one that actually mattered**: for a *web application* project specifically, MSBuild writes those computed redirects into a companion `Samples.AsmxWebService.dll.config` file in `\bin`, not into `Web.config`. ASP.NET never reads a per-assembly `.dll.config` file at runtime, only `Web.config` itself, so that generated file was silently useless, the exception persisted even after a full clean rebuild. **Actually fixed** by copying the exact redirects MSBuild had already correctly computed (visible in that generated `.dll.config`) into `Web.config`'s own `<runtime><assemblyBinding>` section by hand. Worth remembering for every other web-hosted project in this training set (`Samples.WcfService`, `Samples.MvcWebApi`, `Samples.MvcWebPortal`): `GenerateBindingRedirectsOutputType` is a useful *calculator*, not a complete fix, for anything hosted under `System.Web`, the answer it computes still has to be copied into `Web.config` manually.

---

## Logging: log4net Replaced With Serilog

DataBank has standardized on Serilog going forward, so this project's logging was swapped, not just modernized. The shape of the code barely changed, `ErrorHandling.Logger` and `Database.Logger` are still static properties any part of the app can log through, only the underlying type changed (`log4net.ILog` → `Serilog.ILogger`), and the method calls (`Logger?.Debug(...)`, `Logger?.Error(...)`) are identical, Serilog's `ILogger` happens to expose the same method names.

The real difference is *configuration*. log4net needed a `<log4net>` section in `Web.config`, parsed by a custom `ConfigurationSectionHandler`, an appender, a layout pattern, all in XML. Serilog's sink configuration (where and in what format logs are written) lives in a dedicated `serilog.json` file instead, so it can change without a recompile:

```json
{
  "Serilog": {
    "WriteTo": [
      { "Name": "File", "Args": { "path": "C:\\Temp\\Logs\\ExampleWebService.log", "outputTemplate": "..." } }
    ]
  }
}
```

`serilog.json` deliberately does *not* set a `MinimumLevel`. That's still driven from code, in `ExampleWebService`'s static constructor, directly from `Web.config`'s existing `debugMode` setting, via a `LoggingLevelSwitch`:

```csharp
var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("serilog.json", optional: false, reloadOnChange: true)
    .Build();

var levelSwitch = new LoggingLevelSwitch(Settings.DebugMode ? LogEventLevel.Debug : LogEventLevel.Error);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

This keeps the exact same runtime behavior the original hand-coded version had, toggling `debugMode` in `Web.config` alone (no recompile) still switches verbosity, while moving the actually-likely-to-change-per-environment bit (where logs land, what they look like) into a plain JSON file a non-developer could edit. Requires three more packages beyond core Serilog: `Serilog.Settings.Configuration` (the `.ReadFrom.Configuration()` extension), `Microsoft.Extensions.Configuration` and `Microsoft.Extensions.Configuration.Json` (the `ConfigurationBuilder`/`AddJsonFile` machinery). `Web.config` no longer needs a `<log4net>` block or a log4net config-section declaration, both were removed.

**Worth checking after adding these packages**: any time new packages are added to this project, re-check `Samples.AsmxWebService.dll.config` (generated fresh on every build, see the binding-redirect discussion above) for any new or changed entries, `Microsoft.Extensions.Configuration.*` and `Serilog.Settings.Configuration` bring in their own transitive dependencies (`Microsoft.Extensions.Primitives`, `Microsoft.Extensions.FileProviders.*`, possibly newer versions of things already redirected like `System.Memory`), any of which could need a matching entry copied into `Web.config`'s `<runtime><assemblyBinding>` section, exactly the same way the original Serilog redirect was added.

**Another real gotcha hit while testing**: `Microsoft.Extensions.Configuration` defines its own `ConfigurationManager` and, less expectedly, its own `ConfigurationBuilder`, both genuinely ambiguous against `System.Configuration`'s classes of the same names once both namespaces are `using`-imported in the same file (yes, `System.Configuration` really does have its own `ConfigurationBuilder` too, the abstract base for the `configBuilders` feature, easy to not know it exists until it collides with something). **Fixed** by fully qualifying both call sites: `System.Configuration.ConfigurationManager.GetSection(...)` for the existing `serviceSettings` read, `Microsoft.Extensions.Configuration.ConfigurationBuilder` for the new `serilog.json` loading.

---

## A Real Issue Fixed: Hardcoded Test Credentials

The original `Web.config` pointed at a real internal hostname and a real (if low-value) credential pair:

```xml
<database architecture="SqlServer" server="OnBaseTestVM" ... username="HSI" password="wstinol" />
```

Genericized to placeholder values (`YourSqlServerHostname` / `YourUsername` / `YourPassword`), with a comment pointing at where to fill in a real SQL Server instance to actually run this sample. Not appropriate to ship real internal server details in a training samples repo, regardless of whether that particular server is still in service.

---

## A Correction: The ASMX/TLS Training Note

The original training note in `ExampleWebService.asmx.cs` claimed "ASMX only supports TLS 1.1". That's not accurate, TLS version is negotiated by the OS and .NET Framework itself (via settings like `SchUseStrongCrypto`), not restricted by ASMX as a technology. Removed from the note; the two limitations that *are* genuinely specific to ASMX (no RESTful URI support, inherently SOAP-based even when forced to accept/return JSON) are kept.

---

## Try It Yourself

Point `Web.config`'s `<database>` element at a real SQL Server instance with a `ZipCodes` table (`State`, `County`, `City`, `ZipCode` columns), then run the project (F5 in Visual Studio, IIS Express). Browse to `ExampleWebService.asmx` directly to see the auto-generated test page ASP.NET provides for every `[WebMethod]`, a genuinely useful feature this technology still has going for it.
