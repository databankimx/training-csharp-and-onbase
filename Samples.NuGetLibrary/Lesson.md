# Samples.NuGetLibrary

## What This Is

A small, framework-agnostic class library -- ZIP code validation and formatting utilities, the same domain every other Samples project uses -- packaged and versioned as a real NuGet package (`DataBank.Samples.LocationLookup`). The lesson here is the packaging technique itself, not the validation logic.

This project **multi-targets `net48` and `net10.0`** in a single `.csproj`, a realistic scenario for an internal shared library (like DataBank's own `Databank.*` suite) that needs to serve both classic and modern .NET consumers from one package.

---

## When to Extract Something Into a Shared NuGet Library

When the same, genuinely stable logic would otherwise be duplicated -- or drift into slightly different implementations -- across multiple projects. `ZipCodeValidator` and `LocationFormatter` are a good fit specifically because they're:

- Small and focused.
- Dependency-free (no EF, no ASP.NET, no framework-specific code).
- Unlikely to need different behavior per consumer.

A library that *would* need heavy framework-specific dependencies (EF Core vs. EF6, ASP.NET Core vs. classic ASP.NET) usually shouldn't be one multi-targeted package -- those concerns belong in separate packages or in the consuming project.

Note: the other Samples projects in this training set don't reference this package -- they each implement their own ZIP lookup directly. That's a deliberate teaching choice (each sample demonstrates a standalone, self-contained project type), not a recommendation.

---

## How Multi-Targeting Works

```xml
<!-- Samples.NuGetLibrary.csproj -->
<PropertyGroup>
    <TargetFrameworks>net48;net10.0</TargetFrameworks>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>DataBank.Samples.LocationLookup</PackageId>
    <Version>1.0.0</Version>
</PropertyGroup>
```

`TargetFrameworks` (plural) builds the library for both targets. The resulting `.nupkg` contains both `net48/` and `net10.0/` subfolders under `lib/`. NuGet selects the appropriate one automatically when the package is installed.

`GeneratePackageOnBuild` means every build produces a `.nupkg` in `bin\<Configuration>\` -- a plain `dotnet build` or F5 already packages it.

---

## Creating a NuGet Library Project

### Visual Studio

**File > New > Project**, "Class Library", choose .NET version, click Create.

To add multi-targeting: open the `.csproj`, change `<TargetFramework>` to `<TargetFrameworks>net48;net10.0</TargetFrameworks>`.

To configure NuGet metadata: right-click the project > Properties > Package tab. Set Package ID, Version, Authors, Description. Or add them directly to the `.csproj`:

```xml
<PropertyGroup>
    <PackageId>Company.MyLibrary</PackageId>
    <Version>1.0.0</Version>
    <Authors>DataBank IMX</Authors>
    <Description>Shared utilities for...</Description>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
</PropertyGroup>
```

### VS Code

```powershell
dotnet new classlib -n MyLibrary -f net10.0
```

Edit the `.csproj` directly to add multi-targeting and NuGet metadata. Build:

```powershell
dotnet build
dotnet pack --configuration Release
```

---

## Building and Publishing the Package

```powershell
# Build the package
dotnet pack --configuration Release

# Publish to DataBank's internal GHE NuGet feed
dotnet nuget push bin\Release\DataBank.Samples.LocationLookup.1.0.0.nupkg `
    --source "https://nuget.pkg.github.com/databankimx/index.json" `
    --api-key <your-GHE-PAT>
```

See `LectureNotes.md` for the full publish workflow and PAT configuration.

---

## Related Projects

- `Samples.NUnitTests` -- the unit test project for this library, and its first real consumer.
