# README

## DataBank IMX - C# Developer Training Solution

## License and Support

This repository is provided under the [MIT License](LICENSE). You are free to use, copy, modify, merge, publish, distribute, sublicense, and/or sell the materials in this repository, subject to the terms of that license.

This repository is provided for training and reference purposes only. DataBank IMX does not provide warranties, guarantees, maintenance, troubleshooting, implementation assistance, or technical support for this code or its use. Use it in any way permitted by the MIT License, but you are responsible for evaluating, adapting, testing, and supporting any use of it in your own environment.

---

### What is this repository for?

* Modernized, standardized C# developer training curriculum for DataBank IMX
* Chapter-by-chapter console application projects covering C# fundamentals through advanced topics, based on the *MCSD Certification Toolkit (Exam 70-483)* textbook
    * Each chapter's main lesson project is paired with standalone `TextbookCode.*` labs adapted from the textbook's downloadable sample code
* Migrated from the legacy `developer-training-bb` solution, old-style `.csproj` files converted to SDK-style, targeting `net48` with `LangVersion latest`
* End goal is developer readiness for Unity API development, which is pinned to `net48`, so no multi-targeting to `net8.0` or later

---

### Project Information

* Author: [Scott McLean](mailto:smclean@databankimx.com)
* Internal training curriculum, no external customer or project record

---

### Setup/Requirements

* Visual Studio 2026 or later, with the .NET desktop development workload
* .NET SDK capable of building `net48` (requires the .NET Framework 4.8 targeting pack)
* DLLs / NuGet Packages (by chapter, not every project needs all of these)
    * `Newtonsoft.Json` (Chapter 4)
    * `Microsoft.Office.Interop.Excel` (Chapter 4, COM interop lesson)
    * `Microsoft.CSharp` (any project using the `dynamic` keyword, referenced explicitly since it isn't implicit on `net48`)
    * `NUnit`, `NUnit3TestAdapter`, `Microsoft.NET.Test.Sdk` (`CSharp.SharedLibrary.Tests`)
    * `Hyland.Unity.v25` (Chapter 5 Supplemental, OnBase's proprietary Unity API, resolved from DataBank's internal GHE feed, requires the `DataBank GitHub` source already present in your own user-level `NuGet.config`, do not add a solution-level `NuGet.config` for this, see Known Conflicts)
* OnBase
    * For projects that take advantage of the OnBase Unity API, you will need:
        * A working OnBase system with the "Unity Integration Toolkit" licensed
        * Access to the OnBase Unity API DLLs, which are not included in this repo and must be obtained from Hyland Software
            * Access to the DataBank NuGet feed for the `Hyland.Unity.v25` package, which is only available to DataBank employees and contractors<br>or
            * The following DLLs from your OnBase installation:
                * Hyland.Unity.dll
                * Hyland.Types.dll
                * Hyland.Applications.Web.Security.dll
    * Some projects require access to the DataBank Extensions Library, which is only available to DataBank employees and contractors. If you are a DataBank employee or contractor, please contact the Dev Team for access.

---

### Known Conflicts/Compatibility Notes

* Chapter 4's Excel interop lesson (`ExcelInterop()` in `CSharp.Ch04.UsingTypes`) requires Microsoft Excel to actually be installed on the machine running it, it launches and drives a real Excel instance
* `dynamic` requires an explicit `<Reference Include="Microsoft.CSharp" />` in any `.csproj` that uses it, SDK-style `net48` projects don't pull this in implicitly the way old-style projects with a full `Reference` list did
* `TextbookCode.*` projects intentionally preserve the original textbook download's casing (camelCase fields, lowercase method names in some labs) even where it doesn't match the PascalCase standard used everywhere else, this is deliberate, not an oversight
* `CSharp.Ch04.TextbookCode.ExcelInterop` uses a real `<COMReference>` (`WrapperTool=tlbimp`, generated via Visual Studio's Add > COM Reference dialog against the Excel Object Library registered on the machine), not the `Microsoft.Office.Interop.Excel` NuGet package used everywhere else, to keep its code byte-for-byte identical to the textbook download. The `dotnet` SDK CLI's bundled MSBuild cannot build `<COMReference>` items at all (`MSB4803`, the `ResolveComReference` task isn't implemented there), only the full .NET Framework MSBuild that ships with Visual Studio can, this is a hard tooling limitation, not a missing-PIA problem. `LessonRunner` handles this project specially (see `RequiresFullFrameworkMsBuild` in `LessonRunner\Program.cs`), locating and invoking `MSBuild.exe` via `vswhere.exe` instead of `dotnet run`. CI should still use `DataBank.DeveloperTraining.CI.slnf` (see Usage) to build everything except this one project, since a CI runner won't have Visual Studio's MSBuild available either
* **Never add a solution-level `NuGet.config` with `<clear />` to this repo.** A version of this repo briefly had one to point at the `CSharp.Ch05.Supplemental.ConfigurationClasses` GHE feed, `<clear />` wiped out every source from the real, correctly-configured user-level `NuGet.config` (nuget.org, DataBank's baget feed, and the `DataBank GitHub` GHE source with its credentials), replacing them with a single guessed, wrong URL, which broke restore entirely with a confusing "not a valid JSON object" error. That file has been removed. If a solution-level `NuGet.config` is ever genuinely needed again, do not use `<clear />`, let it merge with the user-level config instead

---

### Usage

* Open `DataBank.DeveloperTraining.sln` in Visual Studio
* Solution structure:

|Folder|Contents|
|-|-|
|`Solution Items`|`.gitignore`, `Directory.Build.props` (shared build settings for every project)|
|`Resources`|Shared reference material carried over from `developer-training-bb\Other Resources`: a quick-reference PDF, an ASCII/Unicode chart workbook, `aspnet_setreg.exe` (referenced by Chapter 5's credential-encryption lesson), and `ExternalData.bak`. DLLs were deliberately left out of this folder, see Known Conflicts for how those are handled instead|
|`CSharpTraining\ChapterNN`|Each chapter's main lesson project plus its `TextbookCode.*` labs|
|`CSharpTraining\SharedCode`|`CSharp.SharedLibrary`, `CSharp.SharedLibrary.Tests`, `LessonRunner`|

* Every project's folder name, `.csproj` file name, and `AssemblyName` are kept identical on purpose, `LessonRunner` and other tooling rely on that convention
* Each chapter's main lesson project includes two markdown files:
    * `LectureNotes.md`, instructor-facing, includes gotchas, bugs found and fixed during the modernization pass, and design notes
    * `{Lesson}.md`, student-facing, clean walkthrough of the lesson content with no discussion of bugs or fixes
        * Within `{Lesson}.md`, any section titled `Bonus: ...` is content added within an existing lesson, beyond what the MCSD Certification Toolkit itself covers
* A project named `CSharp.ChNN.Supplemental.*` is a whole lesson added in its entirety, not adapted from the textbook at all, the `Supplemental` project is to a chapter what a `Bonus:` section is to a single lesson
* To run through the curriculum in order, build and run `LessonRunner`, it presents a chapter menu, then a lesson menu in logical (not alphabetical) teaching order, runs the selected lesson, and returns to the lesson menu when it exits
    * `LessonRunner` launches each lesson via `dotnet run --project`, so lessons build automatically if they're out of date, except `CSharp.Ch04.TextbookCode.ExcelInterop`, which needs the full Visual Studio MSBuild instead (see Known Conflicts)
    * Update `BuildCatalog()` in `LessonRunner\Program.cs` when adding new chapters or lessons, and set `requiresFullFrameworkMsBuild: true` on any new lesson that uses a `<COMReference>`
* CI should build against `DataBank.DeveloperTraining.CI.slnf` (a solution filter at the repo root) instead of the full `.sln`, e.g. `dotnet build DataBank.DeveloperTraining.CI.slnf`. It excludes `CSharp.Ch04.TextbookCode.ExcelInterop`, the one project that needs the full Visual Studio MSBuild to build (see Known Conflicts), which a CI runner won't have. Keep this filter's project list in sync whenever a new project is added to the solution, unless that new project has the same `<COMReference>` limitation

---

### Version History

* 08/12/2026 - Migrated Chapters 1-4 (`HelloWorld`, `BasicProgramStructure`, `WorkingWithTheTypeSystem`, `UsingTypes`), `CSharp.SharedLibrary` (plus test project), and `LessonRunner` from `developer-training-bb` to SDK-style projects targeting `net48`
* 08/20/2026 - Migrated Chapters 5-6 (`WorkingWithData`, `DebuggingAndErrorHandling`), added `CSharp.Ch05.Supplemental.ConfigurationClasses` ..., plus `Resources` folder with reference material, ASCII/Unicode chart, and `aspnet_setreg.exe`. Partial migration of Chapter 7

---

### Roadmap

1. Add remaining textbook chapters
1. Add supplemental lessons for advanced topics not covered in the textbook
1. Add a net8.0 or net10.0 target to the solution for developers who want to learn the latest C# features, while keeping the `net48` target for Unity API development

### Who do I talk to?

* Any questions can be addressed to the following
    * [Scott McLean](mailto:smclean@databankimx.com)
    * [Dev Team](mailto:development@databankimx.com)
