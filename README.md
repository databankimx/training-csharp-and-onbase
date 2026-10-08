# DataBank IMX - Developer Training Solution

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

## About

- **Author:** [Scott McLean](mailto:smclean@databankimx.com)
- **Purpose:** Internal developer training curriculum for DataBank IMX

### Textbook reference

*MCSD Certification Toolkit (Exam 70-483) Programming in C#* - Covaci, O'Brien, Stephens, Varallo - Wrox/Wiley  
ISBN: 978-1118612095 - [Amazon](https://www.amazon.com/dp/1118612094)

### Why am I using this old, out-of-print textbook?

Although the 70-483 exam is retired, the material in this textbook is still valid for learning C# fundamentals and even advanced topics.

The lessons and labs in this repository are based on the content of the textbook, but have been modernized and standardized for use in our internal training curriculum.

While there are newer concepts and features in C# that are not covered in this textbook, the fundamentals are still relevant and important for any C# developer to understand.

### Will this be updated to use a newer textbook?

Probably.

At some point I plan to add a dedicated .NET 10 path in the training. However, as of this writing, the Hyland Unity API is pegged to .NET Framework 4.8, and Unity scripting is pegged to C# 6, so I am not planning to replace the existing curriculum.

~Scott McLean, 2026

---

## Running the training

The solution supports two complementary training modes and one web-based track.

### C# Lesson Walkthrough - LessonRunner.Wpf

In solution folder: `Training/CSharp/Utilities/LessonRunner.Wpf`

The primary way to work through the C# curriculum. A WPF desktop application that presents each chapter's guided lesson steps in order, with an interactive code viewer, syntax-highlighted source, rendered Lesson.md notes, and a live output pane that runs each step in-process using Roslyn.

**To use it:** Set `LessonRunner.Wpf` as the startup project and run it, or launch `LessonRunner.Wpf.exe` from the build output. Select a chapter in the left panel, select a step, and click Run. See `LessonRunner.Wpf/README.md` for full documentation.

### Complete Chapter Projects - LessonRunner (console) or Visual Studio

Each chapter's main project (e.g. `CSharp.Ch05.ImplementingClassHierarchies`) and its supplemental and textbook projects are complete, runnable console (or WinForms) applications containing the full working code for that chapter. These are reference implementations - browse them in Visual Studio alongside the Lesson.md notes, or run them to see the full program output in sequence.

> All of the projects in the Visual Studio Solution are organized into solution folders, so following through the lesson path is straightforward, even if you choose to run the projects manually.

#### LessonRunner

In solution folder: `Training/CSharp/Utilities/LessonRunner`

**LessonRunner** is the easiest way to run these. It is a console-based menu launcher that lets you select a chapter, then a lesson within it, run it, and land back on the same lesson menu when it finishes - so working through all the lessons in a chapter is a series of keypresses rather than repeatedly changing the startup project.

To use it: set `LessonRunner` (the console project, not `LessonRunner.Wpf`) as the startup project and run it, or:
```
dotnet run --project LessonRunner
```

Select a chapter number, then a lesson number. The lesson builds automatically (via `dotnet run --project`) if it's out of date and runs in the same console window. When it exits you're returned to the lesson menu, one keypress away from the next lesson.

**To add a new chapter or lesson to the menu:** update `BuildCatalog()` in `LessonRunner/Program.cs`. Each entry needs a display name and the project's folder name. Lessons flagged `requiresFullFrameworkMsBuild: true` are built via Visual Studio's MSBuild rather than the `dotnet` CLI (required for COM reference projects - see Known Conflicts). Lessons flagged `requiresVisualStudio: true` cannot be launched from LessonRunner at all and display instructions instead.

**To run a project directly** without LessonRunner: set the desired project as the startup project in Visual Studio and press F5, or:
```
dotnet run --project <ProjectFolder>
```

### EForms Training - EForms.TrainingNavigator

In solution folder: `Training/OnBase/E-Forms/EForms.TrainingNavigator`

A self-hosted web application for the HTML/CSS/JavaScript/OnBase Forms curriculum. It serves the lesson files from `EForms.TrainingNavigator/wwwroot/` and provides a browser-based preview/code/console environment.

**To use it:**
1. Restore `Resources/ExternalData.bak` into a SQL Server instance and update the connection string in `EForms.TrainingNavigator/appsettings.json`
2. Set `EForms.TrainingNavigator` as the startup project and run it, or: `dotnet run --project EForms.TrainingNavigator`
3. Open `http://localhost:5000` in a browser

See `EForms.TrainingNavigator/README.md` for full setup instructions including database restoration.

---

### Unity API and REST API tracks

Each track includes a collection of class library projects demonstrating aspects of the API solutions.

Each also includes two test harness projects (WPF and Web) that provide a UI to test all of the functionality in the track. The WPF harness is a desktop application that runs against a local OnBase system, while the Web harness is a browser-based application that runs against a remote OnBase API server.

### Sample Projects

Each of these must be run from Visual Studio or the `dotnet` CLI. They are not integrated into either Lesson Runner.

These are a collection of project types, implementing the same functionality on top of different technologies.

All of these require a live SQL Server instance for the database-backed samples. The server must have the ExternalData database restored from `Resources/ExternalData.bak`. Update the connection string in each project's `appsettings.json` or `App.config` as needed.

## Setup and requirements

* Visual Studio 2026 or later, with the .NET desktop development workload
* .NET SDK capable of building `net48` (requires the .NET Framework 4.8 targeting pack)

### NuGet packages (by track - not every project needs all of these)

| Package | Used by |
|---|---|
| `Newtonsoft.Json` | Chapter 4 |
| `Microsoft.Office.Interop.Excel` | Chapter 4 COM interop (see Known Conflicts) |
| `Microsoft.CSharp` | Any project using `dynamic` (not implicit on net48) |
| `NUnit`, `NUnit3TestAdapter`, `Microsoft.NET.Test.Sdk` | `CSharp.SharedLibrary.Tests`, `Samples.NUnitTests` |
| `Hyland.Unity` v26.1.2 | Unity API track (DataBank GitHub NuGet feed, net48) |
| `Hyland.Unity.netstandard` v26.1.2 | Unity API track (net10 illustrative project) |
| `DBIMX.Extensions.v25` 1.0.64 | `Unity.07` (until a v26 package is published) |
| `Databank.Logging`, `Databank.Models` | Internal DataBank packages (DataBank GHE NuGet feed) |
| `Microsoft.EntityFrameworkCore.SqlServer` | `EForms.TrainingNavigator` |
| `Serilog`, `Serilog.AspNetCore` | `EForms.TrainingNavigator` |

> **Hyland.Unity** is resolved from the `DataBank GitHub` NuGet source. This source must already be present in your user-level `NuGet.config` with valid credentials. Do not add a solution-level `NuGet.config` with `<clear />` to satisfy this - see Known Conflicts.

### OnBase requirements (Unity and REST API tracks)

- A working OnBase system with the Unity Integration Toolkit licensed
- The `Hyland.Unity` NuGet package from the DataBank GitHub feed, or Hyland.Unity.dll, Hyland.Types.dll, and Hyland.Applications.Web.Security.dll from your OnBase installation
- `Unity.07` additionally requires `DBIMX.Extensions_unsigned.v25` from the DataBank extensions library (contact the Dev Team)
- REST API track: access to an OnBase API Server and a configured Hyland Identity Provider

---

## Known conflicts and compatibility notes

- **Excel interop (Ch04):** `CSharp.Ch04.TextbookCode.ExcelInterop` uses a `<COMReference>` (not the NuGet package), requires the Excel Object Library registered on the machine, and can only be built by Visual Studio's full MSBuild - not the `dotnet` CLI. CI should use `DataBank.DeveloperTraining.CI.slnf` which excludes it.
- **`dynamic` keyword:** Requires an explicit `<Reference Include="Microsoft.CSharp" />` in any net48 SDK-style project that uses it.
- **TextbookCode formatting:** `TextbookCode.*` projects intentionally preserve the original textbook's casing (camelCase fields, lowercase method names in some labs). This is deliberate, not an oversight.
- **No solution-level NuGet.config with `<clear />`:** A version of this repo briefly had one, which wiped every source from the user-level config and broke restore entirely. It has been removed. If a solution-level config is ever needed again, do not use `<clear />` - let it merge with the user-level config.

---

## Solution structure

### CSharpTraining - C# chapter projects

#### Chapter lesson projects

Each chapter has a main project and may have supplemental and textbook code projects. The main project is a complete, runnable console application. Supplemental projects cover topics beyond the textbook. TextbookCode projects are adapted from the textbook's downloadable samples.

| Project | Description |
|---|---|
| `CSharp.Ch01.HelloWorld` | Console I/O, string interpolation, basic types |
| `CSharp.Ch02.BasicProgramStructure` | Control flow, loops, methods, arrays |
| `CSharp.Ch03.WorkingWithTheTypeSystem` | Classes, structs, enums, properties, value vs. reference types |
| `CSharp.Ch04.UsingTypes` | Arrays, generics, delegates, dynamic, COM interop (Excel) |
| `CSharp.Ch05.ImplementingClassHierarchies` | Inheritance, interfaces, abstract classes, IComparable, IDisposable |
| `CSharp.Ch05.Supplemental.Cloning` | ICloneable, deep vs. shallow copy |
| `CSharp.Ch05.Supplemental.ConfigurationClasses` | Layered configuration pattern |
| `CSharp.Ch05.Supplemental.ImplementingClassHierarchies` | Extended hierarchy examples |
| `CSharp.Ch06.DelegatesEventsAndExceptions` | Delegates, events, lambda expressions, exception handling |
| `CSharp.Ch06.Supplemental.01` through `.09` | Named/anonymous delegates, lambdas, callbacks, multicast, exception handling, parameterized thread start, events, assertions, closures |
| `CSharp.Ch07.MultithreadingAndAsynchronousProcessing` | Threads, Tasks, async/await, synchronization primitives |
| `CSharp.Ch07.Supplemental.01` through `.09` | Thread pool, unblocking UI, TPL, async, race conditions, barriers, locking, lock-free alternatives, concurrent collections |
| `CSharp.Ch08.Reflection` | Assembly inspection, dynamic invocation, attributes, CodeDOM |
| `CSharp.Ch08.Supplemental.01` through `.04` | Custom attributes, dynamic invocation, CodeDOM compile-and-run, reflection performance |
| `CSharp.Ch09.WorkingWithDataCollections` | Arrays, generic and non-generic collections, HashSet, SortedList, LinkedList |
| `CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework` | ADO.NET direct access, Entity Framework 6 (requires SQL Server) |
| `CSharp.Ch09.Supplemental.02.SqlInjection` | SQL injection demonstration and parameterized query fix (requires SQL Server) |
| `CSharp.Ch09.Supplemental.03.ConnectingToOtherDatabases` | ADO.NET provider pattern: SQLite, MySQL, PostgreSQL, Oracle, ODBC, MongoDB |
| `CSharp.Ch09.Supplemental.04.FileIO` | Files, directories, streams, async file I/O |
| `CSharp.Ch09.Supplemental.05.Serialization` | XML serialization, System.Text.Json, PBKDF2, BinaryFormatter (reference only) |
| `CSharp.Ch10.WorkingWithLinq` | Query and method syntax, joins, grouping, LINQ to XML |
| `CSharp.Ch10.Supplemental.01.DeferredExecution` | Deferred execution consequences: live view, double enumeration, snapshots, modify-during-enum |
| `CSharp.Ch10.Supplemental.02.LinqToXmlDeepDive` | Parse, query, transform, mutate, namespaces, save/load |
| `CSharp.Ch10.Supplemental.03.CustomLinqExtensionMethods` | WhereCustom, DistinctBy, Chunk, Median, eager-validation gotcha |
| `CSharp.Ch10.Supplemental.04.IQueryableVsIEnumerable` | IQueryable translation vs. in-process filtering (requires SQL Server) |
| `CSharp.Ch11.InputValidationDebuggingAndInstrumentation` | TryParse, Regex, sanity checks, Debug.Assert, preprocessor, Trace, EventLog, Stopwatch |
| `CSharp.Ch11.Supplemental.01.RegularExpressionsDeepDive` | Named groups, Matches, Replace, RegexOptions, greedy/lazy, compiled perf |
| `CSharp.Ch11.Supplemental.02.PreprocessorDirectivesDeepDive` | #define, #region, #pragma warning, caller info attributes |
| `CSharp.Ch11.Supplemental.03.TraceListeners` | TextWriterTraceListener, custom listener, multiple listeners, indentation, TraceSwitch |
| `CSharp.Ch11.Supplemental.04.PerformanceCountersAndProfiling` | PerformanceCounter, custom counter, JIT warm-up, GC memory, when to use a real profiler |
| `CSharp.Ch12.UsingEncryptionAndManagingAssemblies` | AES, RSA, SHA-256, X.509 certificates, strong naming, GAC |
| `CSharp.Ch12.Supplemental.01.DigitalSignaturesDeepDive` | RSA signing vs. encryption, HMAC |
| `CSharp.Ch12.Supplemental.02.PasswordHashingDoneRight` | PBKDF2, salting, constant-time comparison |
| `CSharp.Ch12.Supplemental.03.CertificatesDeepDive` | Extensions, PFX/CER export, Windows certificate store, chain validation |
| `CSharp.Ch12.Supplemental.04.StrongNamingAndTheGacDeepDive` | Real assembly inspection, GlobalAssemblyCache, side-by-side versioning, binding redirects |

#### TextbookCode projects

Adapted from the textbook's downloadable sample code. Intentionally preserve original formatting. Grouped under each chapter in the solution.

| Chapter | TextbookCode projects |
|---|---|
| Ch02 | AverageGrades, LotteryProgram, UsingIfStatements, WorkingWithForLoops |
| Ch03 | AccessingProperties, OverloadingConstructors, StudentClass, StudentClassWithMethods, UsingEnums, UsingProperties, UsingValueTypes, ValueTypeAlias, ValueTypePassing |
| Ch04 | CastingArrays, Ch04RealWorldScenario01-04, CloneArray, ExcelInterop, Permutations, ShortPathNames |
| Ch05 | Ch05RealWorldScenario01-02, ComparablePerson, EllipsesAndCircles, ICloneablePerson, IComparableCars, IComparerCars, IDisposableClass, IEnumerableTree, IEquatablePerson, PersonHierarchy, ThisAndBase, TreeEnumerator, UniversityClasses |
| Ch06 | AnonymousGraph, ArithmeticExceptions, AsyncLambdas, BankAccount, Ch06RealWorldScenario01-02, CovarianceAndContravariance, Events, ExceptionHandling, GraphFunction, MoneyMarketAccount, StaticAndInstanceDelegates |
| Ch07 | BarrierSample, BarrierWithCancellationSample, BarrierWithTasks, ContinuationsApp, Locking, MethodSyncronization, SimpleApp, TPLApp, Utils, WinFormApp, WpfApp, WPFAsyncApp |
| Ch08 | Chapter8 |
| Ch09 | Chapter9, FileIOAsync, NorthwindsClient, NorthwindsConsole, NorthwindsWCFDataService, Serialization |
| Ch10 | LINQSamples |
| Ch11 | Ch11RealWorldScenario01, WriteToEventLog |
| Ch12 | Chapter12 |

#### Supplementary lessons

Standalone topics not tied to a specific chapter.

| Project | Description |
|---|---|
| `CSharp.Supplemental.Algorithms.BigOConcepts` | Big-O notation and complexity analysis |
| `CSharp.Supplemental.Algorithms.Recursion` | Recursive algorithms |
| `CSharp.Supplemental.Algorithms.ReducingComplexity` | Complexity reduction techniques |
| `CSharp.Supplemental.Algorithms.Search` | Linear and binary search |
| `CSharp.Supplemental.Algorithms.Sort` | Bubble, insertion, selection, merge, quick sort |
| `CSharp.Supplemental.Algorithms.Shared` | Shared helpers for algorithm projects |
| `CSharp.Supplemental.Algorithms.Visualizations` | Algorithm step visualizations |
| `CSharp.Supplemental.BitwiseOperations` | Bitwise operators and common patterns |
| `CSharp.Supplemental.DataStructureFundamentals` | Stack, queue, linked list, tree, graph from scratch |
| `CSharp.Supplemental.FactoryPattern.01.NoFactory` | Baseline: direct instantiation without a factory |
| `CSharp.Supplemental.FactoryPattern.02.BasicFactory` | Simple factory method |
| `CSharp.Supplemental.FactoryPattern.03.ImprovingPattern` | Abstract factory and further refinement |
| `CSharp.Supplemental.LoggingWithDatabankLogging` | Internal DataBank logging library |
| `CSharp.Supplemental.LoggingWithLog4Net` | Log4Net configuration and usage |
| `CSharp.Supplemental.LoggingWithSerilog` | Serilog structured logging |
| `CSharp.Supplemental.StringPerformance` | String concatenation vs. StringBuilder benchmarks |
| `CSharp.Supplemental.TrieExamples` | Trie data structure implementation and use cases |

#### Shared code

| Project | Description |
|---|---|
| `CSharp.SharedLibrary` | `GenericFunctions` (Pause, Clear), `DatabankException`, shared models used across all chapter projects |
| `CSharp.SharedLibrary.Tests` | NUnit tests for SharedLibrary |

### LessonRunner

| Project | Description |
|---|---|
| `LessonRunner` | Console menu launcher: select a chapter and lesson, runs the project via `dotnet run`, returns to the menu on exit. Update `BuildCatalog()` in `Program.cs` when adding chapters. |
| `LessonRunner.Core` | Platform-agnostic library: lesson step parser, Roslyn in-process runner, external process runner, models. See `LessonRunner.Core/README.md`. |
| `LessonRunner.Wpf` | WPF desktop application: chapter/step navigation, source viewer, rendered Lesson.md, output pane, theme switching. See `LessonRunner.Wpf/README.md`. |

### EForms Training

| Project | Description |
|---|---|
| `EForms.TrainingNavigator` | ASP.NET Core web app serving the HTML/CSS/JavaScript/OnBase Forms curriculum with a browser-based lesson navigator. See `EForms.TrainingNavigator/README.md`. |

### OnBase Unity API track

End-to-end training for the OnBase Unity API. Requires an OnBase system with the Unity Integration Toolkit licensed and access to the DataBank GitHub NuGet feed.

| Project | Description |
|---|---|
| `Unity.00.CommonFunctionality` | Connection, authentication, shared utilities |
| `Unity.01.ConnectingToOnBase` | Connection patterns, IdP token login |
| `Unity.02.AccessingTaxonomy` | Document types, keyword types, item types |
| `Unity.03.DocumentRetrieval` | Query, retrieve, keyword access |
| `Unity.04.DocumentArchiving` | Archive, re-index, keyword update |
| `Unity.05.UnityScripts` | Unity scripting fundamentals |
| `Unity.06.UnityFormDefaultValues` | Setting form default values |
| `Unity.07.UsingDataBankExtensionsLibrary` | DBIMX.Extensions library usage |
| `Unity.SimpleButBadExample` | Anti-pattern reference: how not to write Unity code |
| `Unity.TestHarness` | WPF test harness for interactive Unity API testing |
| `Unity.TestHarness.Web` | Web-based test harness |

### OnBase REST API track

End-to-end training for the OnBase REST API. Requires an OnBase API Server and a configured Hyland Identity Provider.

| Project | Description |
|---|---|
| `RestApi.00.CommonFunctionality` | HTTP client setup, authentication, shared utilities |
| `RestApi.01.ConnectingToOnBase` | REST connection patterns |
| `RestApi.02.AccessingTaxonomy` | Taxonomy via REST |
| `RestApi.03.DocumentRetrieval` | Document query and retrieval via REST |
| `RestApi.04.DocumentArchiving` | Document archiving via REST |
| `RestApi.TestHarness` | Console test harness |
| `RestApi.TestHarness.Web` | Web-based test harness |

### Sample Projects

Technology survey samples demonstrating specific .NET and web technologies. Each includes a Lesson.md with how-to-create steps for both Visual Studio and VS Code.

#### Desktop

| Project | Description |
|---|---|
| `Samples.WinForms` | Windows Forms: event-driven UI, data binding, dialogs |
| `Samples.Wpf` | WPF: MVVM pattern, data binding, commands, styles |

#### Services

| Project | Description |
|---|---|
| `Samples.WindowsService` | .NET Framework Windows Service using `ServiceBase` |
| `Samples.WindowsService.NetCore` | .NET 10 background service using Generic Host and `BackgroundService` |
| `Samples.GenericHostConsole` | Generic Host in a console application |

#### Web Applications

| Project | Description |
|---|---|
| `Samples.WebForms` | ASP.NET Web Forms (legacy reference) |
| `Samples.MvcWebPortal` | ASP.NET MVC 5 web portal |
| `Samples.MvcWebPortal.Core` | ASP.NET Core MVC web portal |
| `Samples.RazorPages` | ASP.NET Core Razor Pages |
| `Samples.Blazor.Server` | Blazor Server |
| `Samples.Blazor.WebAssembly` | Blazor WebAssembly |

#### Web APIs

| Project | Description |
|---|---|
| `Samples.MvcWebApi` | ASP.NET Web API 2 |
| `Samples.MvcWebApi.Client` | .NET client for the classic Web API |
| `Samples.MvcWebApi.Common` | Shared models for classic Web API |
| `Samples.MvcWebApi.WebClient` | Browser client for classic Web API |
| `Samples.MvcWebApi.Core` | ASP.NET Core Web API |
| `Samples.MvcWebApi.Core.Client` | .NET client for Core Web API |
| `Samples.MvcWebApi.Core.Common` | Shared models for Core Web API |
| `Samples.MvcWebApi.Core.WebClient` | Browser client for Core Web API |
| `Samples.Grpc` | gRPC service |
| `Samples.Grpc.Client` | gRPC client |

#### Web Services (legacy)

| Project | Description |
|---|---|
| `Samples.AsmxWebService` | ASMX/SOAP web service |
| `Samples.AsmxWebService.Client` | .NET client for ASMX service |
| `Samples.AsmxWebService.WebClient` | Browser client for ASMX service |
| `Samples.WcfService` | WCF service |
| `Samples.WcfService.Client` | .NET client for WCF service |
| `Samples.WcfService.WebClient` | Browser client for WCF service |

#### Testing and Utilities

| Project | Description |
|---|---|
| `Samples.NUnitTests` | NUnit unit tests for `Samples.NuGetLibrary` |
| `Samples.NuGetLibrary` | Multi-targeted (net48/net10) NuGet package authoring sample |
| `Samples.InnoSetup` | Inno Setup installer scripts for the Windows Service samples |

---

## CI

Build against `DataBank.DeveloperTraining.CI.slnf` (the solution filter at the repo root) instead of the full `.sln`:

```
dotnet build DataBank.DeveloperTraining.CI.slnf
```

This excludes `CSharp.Ch04.TextbookCode.ExcelInterop`, which requires the full Visual Studio MSBuild and cannot be built by the `dotnet` CLI. Keep this filter in sync when adding new projects that have the same `<COMReference>` limitation.

SonarQube and Snyk scans are configured to exclude all `TextbookCode` projects. See `sonar-project.properties` and `.snyk` at the repo root.

---

## Authorship note

Since you may be wondering, yes, I did use generative AI in some places: specifically for code review and cleanup and to convert my copious (but largely unreadable) notes into meaningful documentation.

Having originally written this training curriculum in 2013, I had a lot of notes and code that were not in a state that could be shared with others. I used AI to help me clean up the code, remove unnecessary comments, and make the documentation more readable. I also used AI to help me identify areas where the code could be improved or simplified.

Both human-only purists and vibe-coding enthusiasts have my apologies for the hybrid approach, but I found it to be a very effective way to get this repository into a state where it could be shared with others. I hope you find it useful and informative.

~ Scott McLean, 2026

---

## Version history

| Date | Changes |
|---|---|
| 08/12/2026 | Migrated Ch01-Ch04, SharedLibrary, LessonRunner from `developer-training-bb` to SDK-style net48 projects |
| 08/20/2026 | Migrated Ch05-Ch06, supplemental lessons, Resources folder |
| 08/22/2026 | Completed Ch07-Ch09 migration |
| 08/24/2026 | Completed Ch10-Ch12 migration |
| 08/30/2026 | Added SampleProjects solution folder |
| 09/01/2026 | Added OnBase Unity API track |
| 09/11/2026 | Added OnBase REST API track |
| 10/03/2026 | Completed Lesson.md sweep. Added SupplementaryLessons track. Added SonarQube/Snyk CI exclusions. Removed LectureNotes.md files. Added LessonRunner.Wpf guided walkthrough mode with Roslyn in-process execution, theme switching, and pop-out lesson window. |

---

## Contact

- [Scott McLean](mailto:smclean@databankimx.com)
- [Dev Team](mailto:development@databankimx.com)
