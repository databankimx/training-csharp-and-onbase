# Samples.NuGetLibrary

> **Looking for implementation details or the actual publish workflow?** See `LectureNotes.md` in this folder.

## What This Is

A small, framework-agnostic class library — ZIP code validation and formatting utilities, the same domain every other sample in this training set uses — packaged and versioned as a real NuGet package (`DataBank.Samples.LocationLookup`), demonstrating the packaging technique itself rather than a UI or a service.

Unlike most samples here, this one **multi-targets `net48` *and* `net10.0`** in a single project, a genuinely realistic scenario for an internal shared library (like DataBank's own `Databank.*` NuGet suite) that needs to serve both classic and modern .NET consumers from one package.

---

## When to Extract Something Into a Shared NuGet Library

When the same, genuinely stable logic would otherwise be duplicated (or, worse, drift into slightly different implementations) across multiple projects. `ZipCodeValidator`/`LocationFormatter` are a good fit specifically because they're small, dependency-free, and unlikely to need different behavior per consumer. A library that *would* need heavy, framework-specific dependencies (EF Core vs. EF6, ASP.NET Core vs. classic ASP.NET) is usually a sign it shouldn't be one multi-targeted package at all, see `LectureNotes.md` for the fuller discussion.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Location.cs` | A plain, framework-agnostic ZIP code lookup result |
| `ZipCodeValidator.cs` | Validates and normalizes ZIP code input |
| `LocationFormatter.cs` | Formats `Location` values for display |

---

## How to Build the Package

```
dotnet pack --configuration Release
```

`GeneratePackageOnBuild` is also set, so a plain `dotnet build`/F5 already produces a `.nupkg` in `bin\<Configuration>\`, worth knowing since it can be surprising the first time you notice it. See `LectureNotes.md` for the actual `dotnet nuget push` command used to publish this to DataBank's internal GHE NuGet feed.

---

## Related Samples

- Every other `Samples.*` project in this training set implements its own ZIP code lookup, none of them currently reference this package, a deliberate choice, see `LectureNotes.md` for why.
