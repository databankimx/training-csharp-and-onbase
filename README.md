# README

## DataBank IMX - C# Developer Training Solution

## License and Support

This repository is provided under the [MIT License](LICENSE). You are free to use, copy, modify, merge, publish, distribute, sublicense, and/or sell the materials in this repository, subject to the terms of that license.

This repository is provided for training and reference purposes only. DataBank IMX does not provide warranties, guarantees, maintenance, troubleshooting, implementation assistance, or technical support for this code or its use. Use it in any way permitted by the MIT License, but you are responsible for evaluating, adapting, testing, and supporting any use of it in your own environment.

---

### Project Information

* Author: [Scott McLean](mailto:smclean@databankimx.com)
* Internal training curriculum, no external customer or project record

---

## Textbook Reference

* Title: MCSD Certification Toolkit (Exam 70-483) Programming in C#
    * Note: This certification is retired, but the material is still valid for our training
* Publisher: Wrox
    * Note: Wrox is now an imprint of [Wiley](https://www.wiley.com/), and this book is no longer in print, but used copies are available from online resellers
* Authors:
    * Tiberiu Covaci
    * Gerry O'Brien
    * Rod Stephens
    * Vincent Varallo
* ISBN: 978-1118612095
* Links:
    * [Amazon](https://www.amazon.com/MCSD-Certification-Toolkit-Exam-70-483/dp/1118612094/ref=sr_1_1?crid=O0WYMAZRQFXK&dib=eyJ2IjoiMSJ9.3WmnuReSYDk9393MvxIf201kT6N0TGrEQE_HGF9Ny-SlTnmSiP6IAmMbAo7FeKXZnUcaIwcN5L8TbRf6TCKWp8pXHpmmizht4nwXieDStR4DAxN68YbMvnCIZGznt1aVAsYIo2QkXxOPCIoLRGfnIoqZdRGuUcY2cYTEkb4BVBbWiCPDJUqKOuT68-tJ33Qna5p7POK8rPZN494tF9P5nh_IbM2nZ6UHwz13SKi9-9M.P6mUzw3YypplZni3C0zgRz2dOa42PkTK_wwE_hIIupY&dib_tag=se&keywords=MCSD+Certification+Toolkit&qid=1788625712&sprefix=mcsd+certification+toolkit%2Caps%2C322&sr=8-1)

### Why am I using this old, out-of-print textbook?

Although the 70-483 exam is retired, the material in this textbook is still valid for learning C# fundamentals and even advanced topics.

The lessons and labs in this repository are based on the content of the textbook, but have been modernized and standardized for use in our internal training curriculum.

While there are newer concepts and features in C# that are not covered in this textbook, the fundamentals are still relevant and important for any C# developer to understand.

### Will this be updated to use a newer textbook?

Probably.

At some point I plan to add a dedicated .NET 10 path in the training. However, as of this writing, the Hyland Unity API is pegged to .NET Framework 4.8, and Unity scripting is pegged to C# 6, so I am not planning to replace the existing curriculum.

~Scott McLean, 2026

---

## Authorship

Since you may be wondering, yes, I did use generative AI in some places: specifically for code review and cleanup and to convert my copious (but largely unreadable) notes into meaningful documentation.

Having originally written this training curriculum in 2013, I had a lot of notes and code that were not in a state that could be shared with others. I used AI to help me clean up the code, remove unnecessary comments, and make the documentation more readable. I also used AI to help me identify areas where the code could be improved or simplified.

Both human-only purists and vibe-coding enthusiasts have my apologies for the hybrid approach, but I found it to be a very effective way to get this repository into a state where it could be shared with others. I hope you find it useful and informative.

~ Scott McLean, 2026

---

### What is this repository for?

* Modernized, standardized C# developer training curriculum for DataBank IMX
* Chapter-by-chapter console application projects covering C# fundamentals through advanced topics, based on the *MCSD Certification Toolkit (Exam 70-483)* textbook
    * Each chapter's main lesson project is paired with standalone `TextbookCode.*` labs adapted from the textbook's downloadable sample code
* Migrated from the legacy `developer-training-bb` solution, old-style `.csproj` files converted to SDK-style, targeting `net48` with `LangVersion latest`
* End goal is developer readiness for Unity API development, which is pinned to `net48`, so no multi-targeting to `net8.0` or later

---

### Setup/Requirements

* Visual Studio 2026 or later, with the .NET desktop development workload
* .NET SDK capable of building `net48` (requires the .NET Framework 4.8 targeting pack)
* DLLs / NuGet Packages (by chapter, not every project needs all of these)
    * `Newtonsoft.Json` (Chapter 4)
    * `Microsoft.Office.Interop.Excel` (Chapter 4, COM interop lesson)
    * `Microsoft.CSharp` (any project using the `dynamic` keyword, referenced explicitly since it isn't implicit on `net48`)
    * `NUnit`, `NUnit3TestAdapter`, `Microsoft.NET.Test.Sdk` (`CSharp.SharedLibrary.Tests`, `Samples.NUnitTests`)
    * `Hyland.Unity` (Unity API track, OnBase's proprietary Unity API, resolved from DataBank's internal GHE feed, requires the `DataBank GitHub` source already present in your own user-level `NuGet.config`, do not add a solution-level `NuGet.config` for this, see Known Conflicts)
    * `Databank.Logging`, `Databank.Models` (internal DataBank NuGet packages, resolved from DataBank's GHE NuGet feed)
* OnBase
    * For projects in the Unity API track, you will need:
        * A working OnBase system with the "Unity Integration Toolkit" licensed
        * Access to the OnBase Unity API DLLs, which are not included in this repo and must be obtained from Hyland Software
            * Access to the DataBank NuGet feed for the `Hyland.Unity` package, which is only available to DataBank employees and contractors<br>or
            * The following DLLs from your OnBase installation:
                * Hyland.Unity.dll
                * Hyland.Types.dll
                * Hyland.Applications.Web.Security.dll
    * Some projects require access to the DataBank Extensions Library (`DBIMX.Extensions_unsigned.v25`), which is only available to DataBank employees and contractors. If you are a DataBank employee or contractor, please contact the Dev Team for access.
    * For projects in the REST API track, you will need access to an OnBase API Server and a configured Hyland Identity Provider

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

| Folder | Contents |
|-|-|
| `Solution Items` | `.gitignore`, `Directory.Build.props` (shared build settings for every project) |
| `Resources` | Shared reference material: a quick-reference PDF, an ASCII/Unicode chart workbook, `aspnet_setreg.exe` (referenced by Chapter 5's credential-encryption lesson), and `ExternalData.bak`. DLLs were deliberately left out of this folder, see Known Conflicts |
| `CSharpTraining\ChapterNN` | Each chapter's main lesson project plus its `TextbookCode.*` labs and `Supplemental.*` projects |
| `CSharpTraining\SharedCode` | `CSharp.SharedLibrary`, `CSharp.SharedLibrary.Tests`, `LessonRunner` |
| `CSharpTraining\SupplementaryLessons` | Standalone supplemental lesson projects covering algorithms, data structures, design patterns, logging, and other topics not tied to a specific chapter |
| `OnBase Unity API` | `Unity.00` through `Unity.07`, `Unity.SimpleButBadExample`, `Unity.TestHarness`, `Unity.TestHarness.Web` - end-to-end Unity API training track |
| `OnBase REST API` | `RestApi.00` through `RestApi.04`, `RestApi.TestHarness`, `RestApi.TestHarness.Web` - end-to-end REST API training track |
| `SampleProjects` | Technology survey samples organized by category (see below) |

* The `SampleProjects` solution folder contains standalone samples demonstrating specific technologies, organized into sub-folders:

| Sub-folder | Projects |
|-|-|
| `Deployment` | `Samples.InnoSetup` - Inno Setup installer scripts for the Windows Service samples |
| `Desktop` | `Samples.WinForms`, `Samples.Wpf` - WinForms (event-driven) and WPF (MVVM) desktop applications |
| `Services` | `Samples.WindowsService` (`net48`, `ServiceBase`), `Samples.WindowsService.NetCore` (Generic Host + `BackgroundService`), `Samples.GenericHostConsole` |
| `Testing` | `Samples.NUnitTests` - NUnit unit tests for `Samples.NuGetLibrary` |
| `Utilities` | `Samples.NuGetLibrary` - multi-targeted (`net48`/`net10.0`) NuGet package authoring |
| `WebApplications` | `Samples.Blazor.Server`, `Samples.Blazor.WebAssembly`, `Samples.MvcWebPortal` (classic MVC 5), `Samples.MvcWebPortal.Core` (ASP.NET Core MVC), `Samples.RazorPages`, `Samples.WebForms` |
| `WebApis` | `Samples.MvcWebApi` + `.Client` + `.Common` + `.WebClient` (classic Web API 2), `Samples.MvcWebApi.Core` + `.Client` + `.Common` + `.WebClient` (ASP.NET Core), `Samples.Grpc` + `.Client` |
| `WebServices` | `Samples.AsmxWebService` + `.Client` + `.WebClient` (ASMX/SOAP), `Samples.WcfService` + `.Client` + `.WebClient` (WCF) |

* Every project's folder name, `.csproj` file name, and `AssemblyName` are kept identical on purpose, `LessonRunner` and other tooling rely on that convention
* Each project includes a `Lesson.md` file: a student-facing walkthrough of the lesson content covering what the project demonstrates, how to run it, and how to create something similar from scratch. Supplemental and TextbookCode projects include a brief "What This Is" orientation. SampleProjects include how-to-create steps for both Visual Studio and VS Code
    * A project named `CSharp.ChNN.Supplemental.*` is a whole lesson added in its entirety beyond the textbook content
    * Within a `Lesson.md`, any section titled `Bonus: ...` covers content beyond what the textbook itself includes
* To run through the curriculum in order, build and run `LessonRunner`, it presents a chapter menu, then a lesson menu in logical (not alphabetical) teaching order, runs the selected lesson, and returns to the lesson menu when it exits
    * `LessonRunner` launches each lesson via `dotnet run --project`, so lessons build automatically if they're out of date, except `CSharp.Ch04.TextbookCode.ExcelInterop`, which needs the full Visual Studio MSBuild instead (see Known Conflicts)
    * Update `BuildCatalog()` in `LessonRunner\Program.cs` when adding new chapters or lessons, and set `requiresFullFrameworkMsBuild: true` on any new lesson that uses a `<COMReference>`
* CI should build against `DataBank.DeveloperTraining.CI.slnf` (a solution filter at the repo root) instead of the full `.sln`, e.g. `dotnet build DataBank.DeveloperTraining.CI.slnf`. It excludes `CSharp.Ch04.TextbookCode.ExcelInterop`, the one project that needs the full Visual Studio MSBuild to build (see Known Conflicts), which a CI runner won't have. Keep this filter's project list in sync whenever a new project is added to the solution, unless that new project has the same `<COMReference>` limitation
* CI scans (SonarQube and Snyk) are configured to exclude all `TextbookCode` projects from analysis. See `sonar-project.properties` and `.snyk` at the repo root

---

### Version History

* 08/12/2026 - Migrated Chapters 1-4 (`HelloWorld`, `BasicProgramStructure`, `WorkingWithTheTypeSystem`, `UsingTypes`), `CSharp.SharedLibrary` (plus test project), and `LessonRunner` from `developer-training-bb` to SDK-style projects targeting `net48`
* 08/20/2026 - Migrated Chapters 5-6, added supplemental lessons, `Resources` folder with reference material and `aspnet_setreg.exe`. Partial migration of Chapter 7
* 08/22/2026 - Completed migration of Chapters 7-9
* 08/24/2026 - Completed migration of Chapters 10-12
* 08/30/2026 - Added `SampleProjects` solution folder with technology survey samples across deployment, desktop, services, testing, utilities, web applications, web APIs, and web services categories
* 09/01/2026 - Added OnBase Unity API track (`Unity.00` through `Unity.07`, `Unity.SimpleButBadExample`, `Unity.TestHarness`, `Unity.TestHarness.Web`)
* 09/11/2026 - Added OnBase REST API track (`RestApi.00` through `RestApi.04`, `RestApi.TestHarness`, `RestApi.TestHarness.Web`)
* 10/03/2026 - Completed `Lesson.md` sweep across all projects. Added `CSharpTraining\SupplementaryLessons` track (algorithms, data structures, design patterns, logging, string performance, and more). Added SonarQube (`sonar-project.properties`) and Snyk (`.snyk`) CI scan exclusions for `TextbookCode` projects. Removed all `LectureNotes.md` files (porting/migration notes folded into `Lesson.md` where relevant, remainder retired)

---

### Roadmap

1. Add more supplemental lessons for advanced topics not covered in the textbook
1. Add a `net10.0` target to the solution for developers who want to learn the latest C# features, while keeping the `net48` target for Unity API development

### Who do I talk to?

* Any questions can be addressed to the following
    * [Scott McLean](mailto:smclean@databankimx.com)
    * [Dev Team](mailto:development@databankimx.com)
