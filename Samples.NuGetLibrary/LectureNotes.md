# Samples.NuGetLibrary

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port), demonstrating NuGet package authoring rather than a UI or service. See `README.md` for the fuller when-to-extract discussion.

---

## Multi-Targeting: One Project, Two Frameworks, One Package

```xml
<TargetFrameworks>net48;net10.0</TargetFrameworks>
```

`<TargetFrameworks>` (plural), not `<TargetFramework>`, is what triggers multi-targeting: the compiler builds this project **twice**, once for each framework, and `dotnet pack` bundles both resulting assemblies into a single `.nupkg`. A consumer referencing this package via `<PackageReference>` automatically gets whichever build matches their own project's target framework, a `net48` consumer gets the `net48` build, a `net10.0` consumer gets the `net10.0` build, entirely transparent to them.

This is exactly the situation DataBank's own `Databank.*` NuGet library suite is in: internal libraries that need to serve both the classic `net48` codebase and newer `net10.0` projects from one published package, rather than maintaining two separately-versioned packages for the same logic.

---

## Why This Library Stays Dependency-Free

`Location`, `ZipCodeValidator`, and `LocationFormatter` reference nothing beyond the .NET base class library itself, no EF Core, no EF6, no ASP.NET Core, nothing framework-specific. This is deliberate, and not just for simplicity: a library that *needs* different dependencies per target framework (EF Core on `net10.0`, EF6 on `net48`, for instance) usually means the underlying logic genuinely differs per framework too, at which point conditional compilation (`#if NET48` / `#if NET10_0`) creeps in, and the "one library, two targets" story starts costing more than it saves. Worth recognizing as a real design signal: if a shared library needs `#if` blocks to handle framework differences in more than a trivial way, that's often a sign it should be two separate packages (or that the framework-specific parts shouldn't be shared at all), not a reason to reach for more preprocessor directives.

This library was kept intentionally small and general enough that the question never comes up, pure validation and formatting logic has no reason to behave differently on `net48` versus `net10.0`.

---

## A Real Multi-Targeting Gotcha: `record` Needs `IsExternalInit`, and `net48` Doesn't Have It

Building this project for `net48` originally failed outright:

```
CS0518: Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported
```

`Location.cs`'s `record Location(...)` (and, more generally, any `init`-only property) is lowered by the compiler using a marker type, `System.Runtime.CompilerServices.IsExternalInit`, that shipped in the BCL starting with .NET 5. `net48`'s own BCL predates that entirely, it simply isn't there. The compiler doesn't care *where* the type comes from though, only that a type with that exact name and namespace exists somewhere visible to the compilation, so `IsExternalInitPolyfill.cs` defines an empty one:

```csharp
#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
```

`#if NETFRAMEWORK` (a symbol the SDK defines automatically for any .NET Framework target, `net48` included, and *not* for `net10.0`) is what keeps this from colliding with the real type `net10.0` already has natively, this file compiles into the `net48` build only. A genuinely common gotcha for any multi-targeted library reaching for `record`/`init` syntax, worth recognizing on sight rather than re-diagnosing from scratch next time it shows up.

---

## `GeneratePackageOnBuild`: A Package on Every Build, Not Just `dotnet pack`

```xml
<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
```

Without this, a `.nupkg` is only produced when `dotnet pack` is run explicitly. With it, an ordinary `dotnet build` (or pressing F5 in Visual Studio) also drops a `.nupkg` into `bin\<Configuration>\`, convenient during active development of the library itself, but worth turning off (or at least being aware of) for a library where you don't want a new package artifact generated on every single build, some teams prefer `dotnet pack` to remain an explicit, deliberate step tied to an actual release.

---

## Publishing to DataBank's GHE NuGet Feed

This training set doesn't publish anywhere automatically, but the real workflow (matching how DataBank's own `Databank.*` suite gets published) is:

```
dotnet pack --configuration Release --output ./nupkg
dotnet nuget push ./nupkg/DataBank.Samples.LocationLookup.1.0.0.nupkg --source https://databankimx.ghe.com/api/v4/packages/nuget/index.json --api-key <your-GHE-personal-access-token>
```

The `--source` URL is GHE's own NuGet feed endpoint (not nuget.org), and the `--api-key` is a GHE personal access token with package-write permission, not a nuget.org API key. Bumping `<Version>` in the `.csproj` before each `dotnet pack` (or using a CI-driven versioning scheme) is what actually lets consumers pick up a new release via `dotnet restore`/`dotnet add package --version`, pushing the same version number twice is rejected by most NuGet feeds, GHE's included.

---

## Why Nothing in This Solution References the Published Package (Yet)

Every other sample in this training set currently implements its own ZIP code validation/formatting inline, rather than referencing `Samples.NuGetLibrary` via `<PackageReference>`. This is deliberate for a training solution specifically: each sample project's `.csproj` and code should stay a **complete, self-contained illustration** of its own topic (a Windows Service, a gRPC service, a Blazor component), without an extra cross-project dependency a reader would need to chase down to fully understand any single sample. In a real, non-training codebase, replacing each project's own duplicated ZIP code logic with a `<PackageReference Include="DataBank.Samples.LocationLookup" Version="1.0.0" />` referencing this exact package would be the natural next step, and is worth trying yourself as an exercise.

---

## Try It Yourself

Run `dotnet pack --configuration Release`, then inspect the resulting `.nupkg` (it's just a ZIP file, rename it to `.zip` and open it) to see the `lib/net48/` and `lib/net10.0/` folders it actually contains, the multi-targeting made concrete.
