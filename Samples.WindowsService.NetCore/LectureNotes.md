# Samples.WindowsService.NetCore

## What This Is

The modern .NET sibling of `Samples.WindowsService`, originally the ONLY Windows Service sample in this training set (named plainly `Samples.WindowsService`, built directly on `net10.0`). Renamed to `.NetCore` once a genuine `net48` baseline was added alongside it. See `README.md` for the fuller when-to-use discussion.

---

## Same Task as Every Other Sample, Deliberately

Every other project in this training set looks up city/county/state by ZIP code, and so does this one, and its `net48` sibling, `Samples.WindowsService`. An earlier version of both projects did something else entirely (a "data health check" scanning the whole `ZipCodes` table for incomplete rows), a reasonable-sounding idea on its own, but a real inconsistency against the rest of the training set: the point of having classic and modern siblings side by side is to illustrate *coding* differences, not different underlying tasks. Corrected here.

**Where does the ZIP code come from?** Every interactive sample gets one from a user (a form field, a search box). A Windows Service has no interactive caller, so `Worker.LookupLocationAsync` reads the ZIP code from a plain text file instead, re-read fresh on every timer tick, so editing the file changes what the *next* scheduled lookup searches for, no restart needed. A registry value is another legitimate way to configure an unattended service like this; a file was used here specifically because it needs no elevated permissions and behaves identically on both this project and its `net48` sibling.

The file's *location* itself is read from `appsettings.json`'s `ZipCodeFilePath` setting, not hardcoded, `Worker`'s constructor now takes an `IConfiguration` and reads it once, at construction time, matching the equivalent fix already applied to `Samples.WindowsService`'s own `App.config`.

---

## What's Actually Different From the Classic `net48` Version

- **Generic Host + `BackgroundService`**, not `System.ServiceProcess.ServiceBase`. `Worker.ExecuteAsync` runs continuously with a `PeriodicTimer`; the classic version's `OnStart`/`OnStop` are event-driven callbacks the Service Control Manager invokes directly.
- **`AddWindowsService()`** lets the *same executable* run as a normal console app during development and as an installed service in production, auto-detecting the context. The classic version genuinely behaves differently depending on how it's launched (see that project's own notes).
- **EF Core Code-First** (`Models/ZipCode.cs` is the source of truth) vs. EF6 Database-First (an `.edmx` reverse-engineered from an existing table).
- **Dependency injection throughout.** `Worker` receives `IServiceScopeFactory` and creates a fresh DI scope (and therefore a fresh `DbContext`) on every lookup, contrast this against the classic version's direct `new ExternalDataEntities()` construction.
- **`LoggerMessage.Define`**, precompiled logging delegates, instead of a `Logger` field written to directly (see the `CA1873` discussion below).

---

## Avoiding `CA1873`: `LoggerMessage.Define`, Not `[LoggerMessage]`

Direct calls to `logger.LogInformation(...)`/`logger.LogWarning(...)` parse their message template and box every argument on **every** call, even when that log level is disabled, real overhead on a service that runs a check every five minutes for its entire lifetime, correctly flagged by static analysis (`CA1873`). The newer `[LoggerMessage]` attribute + `partial` method source-generator pattern was tried first here and turned out to be a dead end in this project (the generator never actually produced an implementation, for reasons that weren't reliably diagnosable without an actual build to inspect). `LoggerMessage.Define` (available since .NET Core 2.0, no source generator involved) was used instead, achieving the identical performance characteristic through an older, simpler, unconditionally reliable mechanism. Five precompiled delegates cover every log message this worker writes: file-not-found, file-empty, no-results, location-found, and lookup-failed.

---

## Try It Yourself

Run this project with `dotnet run`, create the ZIP code input file, and watch the log output. Then compare it directly against `Samples.WindowsService` (F5 in Visual Studio, or install both as actual services), same underlying task, genuinely different hosting model.
