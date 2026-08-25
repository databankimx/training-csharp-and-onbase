# Ch09 Textbook Code: Northwinds WCF Data Service

## What This Is, and Why It Can't Run Through `LessonRunner`

This exposes the same `Categories`/`Products` data `CSharp.Ch09.TextbookCode.NorthwindsConsole` reads locally, but as a real HTTP/OData web service, `CSharp.Ch09.TextbookCode.NorthwindsClient` then consumes it remotely. This is genuinely different from every other project in this training set: it's hosted through IIS, using `System.Data.Services.DataServiceHostFactory`, which fundamentally requires Visual Studio's own IIS Express integration to run, there is no `dotnet run` equivalent, no standalone `.exe` to launch, this is a `.dll` that IIS Express loads and serves.

**To run it**: open this project in Visual Studio, press F5 (or right-click `NorthwindsService.svc` → "View in Browser"), Visual Studio starts IIS Express and hosts the service automatically. Needs the same `Northwinds` database as `NorthwindsConsole`, see that project's `README.md`.

---

## Why This Stayed a Classic (Non-SDK-Style) `.csproj`

Every other project in this migration uses a modern, SDK-style `.csproj`. This one doesn't, and that's deliberate, not an inconsistency. `UseIIS`/`IISUrl`/IIS Express integration are properties of the classic ASP.NET Web Application project system (`Microsoft.WebApplication.targets`), which SDK-style `.csproj` for classic (non-Core) ASP.NET has no real equivalent for. Forcing this into SDK-style risked producing something that looked plausible but didn't actually host correctly, and there was no way to verify that without a live IIS Express environment to test against. Using the classic format, exactly matching how this technology is actually designed to be built and run, was the safer, more honest choice.

---

## What Changed From the Original Download

Like `CSharp.Ch09.TextbookCode.NorthwindsConsole`, this is an **adapted** port, not byte-for-byte preserved, for the same reason: the original used an EDMX-based, Database First model (a `.edmx` file plus ~35 generated files covering the entire Northwind schema), while `InitializeService()` itself only ever grants access to `Categories`. The model here was rebuilt the same simplified, Code First way as `NorthwindsConsole`, `Category`, `Product`, both pointing at the same `Northwinds` database.

Worth knowing: `DataService<T>` (the class `NorthwindsService` derives from) genuinely supports a Code First `DbContext` the same way it supports an EDMX-based `ObjectContext`, this has been true since WCF Data Services 5.x, so swapping the model type didn't require any changes to `NorthwindsService.svc.cs` itself, it's preserved here completely unchanged from the original download.

---

## A Real Gap in the Original Download: `Web.config`

The original `Web.config` was essentially empty, just XDT-transform-syntax comments, no `connectionStrings` section, no `entityFramework` section at all. Since `NorthwindsEntities` calls `base("name=Northwinds")`, that connection string entry genuinely has to exist somewhere for this service to do anything but throw on first request. This migration's `Web.config` adds a real, working `connectionStrings` and `entityFramework` section (matching `NorthwindsConsole`'s `App.config`), along with the assembly binding redirect Entity Framework 6 needs. Whether the original download's `Web.config` was ever actually completed by hand before use, or whether this service was never fully exercised as originally packaged, isn't something this migration can determine, but the config as downloaded would not have worked.

A smaller detail worth flagging too: the original `NorthwindsService.svc`'s `Factory` attribute hardcoded `Version=5.0.0.0` for `Microsoft.Data.Services`, while the project's own reference (and `packages.config`) pointed at version `5.8.5.0`, a mismatch that would have failed to activate the service at all. Corrected to `5.8.5.0` here, matching the actual referenced package (this project's `EntityFramework` package has since been updated further, to `6.5.2`, see the next section for why that update didn't need a matching change anywhere else).

---

## A Real Gotcha Hit While Testing: EF6's Frozen `AssemblyVersion`

Running this service for the first time threw `FileLoadException: Could not load file or assembly 'EntityFramework'... manifest definition does not match the assembly reference`, worth documenting since the fix is counterintuitive if you haven't hit it before.

Entity Framework 6's **assembly version** has stayed frozen at `6.0.0.0` across every single 6.x NuGet package release, by design, for binary compatibility, `6.4.4`, `6.5.2`, and every other 6.x release are *package* version numbers, not *assembly* version numbers. The actual `EntityFramework.dll` file's identity, the thing `.csproj` references, `Web.config`'s `<configSections>` entry, and any binding redirect all need to agree on, is always `Version=6.0.0.0`, no matter which 6.x package is physically installed.

The failure happened because an earlier edit here "corrected" both the `<configSections>` entry and the binding redirect's `newVersion` to `6.4.4.0` (matching the package version at the time), which broke the exact identity match IIS needs, `6.4.4.0` is simply never the real assembly version, for any EF6 release. The actual fix was reverting both back to `6.0.0.0`:

```xml
<section name="entityFramework" type="...EntityFramework, Version=6.0.0.0, ..." requirePermission="false" />
...
<bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
```

Worth internalizing as a general pattern, not just an EF6-specific fact: a library's *package* version and its *assembly* version are not guaranteed to move together, and NuGet-managed classic (`packages.config`-style) projects can genuinely re-derive the correct assembly version automatically when a package updates (Visual Studio's own package-update process re-corrected the `<configSections>` entry back to `6.0.0.0` here after the package was bumped to `6.5.2`), even when a manual edit elsewhere assumed otherwise.

---

## A Second Round of the Same Problem: `NorthwindsService.svc`'s Hardcoded Version

After fixing the `EntityFramework` mismatch above, the service failed again with the same class of error, this time for `Microsoft.Data.Services`. The cause was almost identical, but the opposite direction: `NorthwindsService.svc`'s `Factory` attribute hardcodes a specific assembly version:

```
Factory="System.Data.Services.DataServiceHostFactory, Microsoft.Data.Services, Version=5.8.4.0, ..."
```

Unlike `EntityFramework`, the `Microsoft.Data.Services` family of packages (`Microsoft.Data.Services`, `Microsoft.Data.Services.Client`, `Microsoft.Data.OData`, `Microsoft.Data.Edm`, `System.Spatial`) genuinely *do* track their assembly version to their package version, installing package `5.8.4` really does produce assembly version `5.8.4.0`. The `.csproj`'s own `<Reference>` elements self-corrected to `5.8.4.0` automatically when the packages were updated (NuGet's install/update process rewrites `.csproj` references), but the `.svc` file's `Factory` string is a plain text attribute NuGet has no reason to know about or touch, so it stayed at whatever version it was manually set to previously, `5.8.5.0`, no longer matching anything actually installed.

**Fixed** by updating `NorthwindsService.svc` to `Version=5.8.4.0`, matching the `.csproj`'s already-correct references. **Also added** binding redirects in `Web.config` for all five `Microsoft.Data.*`/`System.Spatial` assemblies, the same defensive pattern already in place for `EntityFramework`, so that a future package update doesn't require remembering to also hand-edit this one text attribute in a `.svc` file that nothing else touches automatically. The general lesson from both rounds of this: any place a specific assembly version is spelled out by hand (a config section, a `.svc` directive, anywhere outside the actual `<Reference>` elements NuGet manages) is a place that can silently go stale the next time a package updates, worth a habit of checking for exactly this after any dependency bump on a classic, `packages.config`-style project.

---

## Worth Reading Alongside the Other Two Projects

`NorthwindsConsole` shows the identical `Categories`/`Products` data accessed directly through Entity Framework, no network involved. `NorthwindsClient` shows the same data consumed as JSON over HTTP, through this service. Reading all three together is the most complete picture of what this piece of the chapter ("Creating WCF Data Services") is actually teaching: the same underlying data, reachable three different ways, each with real, different tradeoffs.
