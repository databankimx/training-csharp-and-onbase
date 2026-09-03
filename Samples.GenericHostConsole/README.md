# Samples.GenericHostConsole

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

The Generic Host (`Host.CreateApplicationBuilder`, the same host abstraction `Samples.MvcWebApi.Core`'s `WebApplicationBuilder` and `Samples.WindowsService.NetCore` both sit on top of) applied to a plain, one-shot console tool. Like every other sample in this training set, it looks up city/county/state by ZIP code, entered as a command-line argument or typed interactively.

**No `AddWindowsService()`, no `BackgroundService`, no `host.Run()`.** This is the key contrast against `Samples.WindowsService.NetCore`: the Host gives this project dependency injection, configuration binding, and structured Serilog logging, exactly the same building blocks a long-running service gets, without ever needing to run continuously.

---

## When to Use This Pattern

For CLI tools, one-off scripts, and scheduled-task-style utilities that would benefit from real DI/configuration/logging (rather than hand-wiring everything in `Main()`) but don't need to run continuously. If the tool needs to run on a recurring schedule unattended, pair this pattern with Windows Task Scheduler (or a cron-equivalent) rather than reaching for `Samples.WindowsService.NetCore`'s always-running service model, a scheduled one-shot process is often simpler to operate and debug than a service that has to manage its own internal timer.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Builds the host, creates one DI scope, runs one lookup, exits |
| `Services/LocationLookupRunner.cs` | The actual lookup logic, a plain injectable class |
| `Models/ZipCode.cs` | EF Core entity (Code-First) |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |

---

## How to Run

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Run with a ZIP code as an argument: `dotnet run -- 75067`, or run with no argument and enter one when prompted.

---

## Related Samples

- **`Samples.WindowsService.NetCore`** — the same Generic Host pattern, extended into a long-running, `BackgroundService`-based service, worth comparing directly.
