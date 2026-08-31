# Samples.WindowsService.NetCore

## What This Is

The modern .NET sibling of `Samples.WindowsService`, originally the ONLY Windows Service sample in this training set (named plainly `Samples.WindowsService`, built directly on `net10.0`). Renamed to `.NetCore` once a genuine `net48` baseline was added alongside it, correcting an earlier inconsistency (see `Samples.WindowsService`'s own `LectureNotes.md` for the fuller story of why that correction happened).

---

## What's Actually Different From the Classic `net48` Version

- **Generic Host + `BackgroundService`**, not `System.ServiceProcess.ServiceBase`. `Worker.ExecuteAsync` runs continuously with a `PeriodicTimer`; the classic version's `OnStart`/`OnStop` are event-driven callbacks the Service Control Manager invokes directly.
- **`AddWindowsService()`** lets the *same executable* run as a normal console app during development and as an installed service in production, auto-detecting the context. The classic version genuinely behaves differently depending on how it's launched (see that project's own notes).
- **EF Core Code-First** (`Models/ZipCode.cs` is the source of truth) vs. EF6 Database-First (an `.edmx` reverse-engineered from an existing table).
- **Dependency injection throughout.** `Worker` receives `IServiceScopeFactory` and creates a fresh DI scope (and therefore a fresh `DbContext`) on every check, contrast this against the classic version's direct `new ExternalDataEntities()` construction.
- **`LoggerMessage.Define`**, precompiled logging delegates, instead of a `Logger` field written to directly (see the `CA1873` discussion below).

---

## Avoiding `CA1873`: `LoggerMessage.Define`, Not `[LoggerMessage]`

Direct calls to `logger.LogInformation(...)`/`logger.LogWarning(...)` parse their message template and box every argument on **every** call, even when that log level is disabled, real overhead on a service that runs its check every five minutes for its entire lifetime, correctly flagged by static analysis (`CA1873`). The newer `[LoggerMessage]` attribute + `partial` method source-generator pattern was tried first here and turned out to be a dead end in this project (the generator never actually produced an implementation, for reasons that weren't reliably diagnosable without an actual build to inspect). `LoggerMessage.Define` (available since .NET Core 2.0, no source generator involved) was used instead, achieving the identical performance characteristic through an older, simpler, unconditionally reliable mechanism.

---

## Try It Yourself

Run this project with `dotnet run`, and compare it directly against `Samples.WindowsService` (F5 in Visual Studio, or install both as actual services), same underlying task, genuinely different hosting model.
