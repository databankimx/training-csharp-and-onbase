# Samples.WindowsService

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

A classic `net48` Windows Service, built on `System.ServiceProcess.ServiceBase`. It periodically checks the `ZipCodes` table for rows with missing data and logs what it finds, a genuine, if simple, example of the recurring background maintenance task a Windows Service is actually used for.

`Samples.WindowsService.NetCore` is the modern sibling of this project, built on the Generic Host + `BackgroundService` instead.

---

## When to Use This Over the Modern Version

For existing `ServiceBase`-based services, or genuine constraints ruling out modern .NET. For any *new* Windows Service, the Generic Host approach (`Samples.WindowsService.NetCore`) is the better default, less boilerplate, dependency injection, and the same executable doubles as a console app during development.

---

## What Makes This Genuinely Different From `Samples.WindowsService.NetCore`

- **`OnStart`/`OnStop`**, event-driven callbacks the Service Control Manager invokes directly, not `BackgroundService.ExecuteAsync`'s continuous loop. `OnStart` must return promptly; the recurring work is handed off to a `System.Timers.Timer` instead.
- **No `PeriodicTimer`** (a modern .NET-only API). The classic, still entirely valid equivalent is `System.Timers.Timer` with an `Elapsed` event handler, started in `OnStart` and disposed in `OnStop`.
- **`InstallUtil.exe`, not `sc create`.** `ProjectInstaller.cs` (a `[RunInstaller(true)]`-decorated class with a `ServiceProcessInstaller`/`ServiceInstaller` pair) is what `installutil.exe Samples.WindowsService.exe` uses to actually register the service. The `.NetCore` sibling has no equivalent file at all.
- **No dependency injection.** `CheckDataHealth()` constructs its own `ExternalDataEntities` directly in a `using` block, no `IServiceScopeFactory`, no container.
- **This executable genuinely cannot run interactively.** Unlike the `.NetCore` sibling's `AddWindowsService()` (which auto-detects console vs. service context), running this `.exe` directly throws immediately, it only runs when started by the Service Control Manager after installation.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | `ServiceBase.Run(new DataHealthCheckService())` |
| `DataHealthCheckService.cs` / `.Designer.cs` | `OnStart`/`OnStop`, the recurring check |
| `ProjectInstaller.cs` / `.Designer.cs` | `InstallUtil.exe` registration |
| `Models/` | EF6 Database-First model (same `ZipCode` entity as `Samples.MvcWebPortal`) |

---

## How to Run

1. Point `App.config`'s `ExternalDataEntities` connection string at a real SQL Server instance.
2. Build the project (`dotnet build`, or F5 in Visual Studio — note F5 will fail with a clear error, since this can't run interactively, see above).
3. From an elevated Command Prompt:
   ```
   installutil.exe C:\path\to\Samples.WindowsService.exe
   sc start Samples.WindowsService
   ```
4. Check the configured log file (`serilog.json`) for the data-health check results.
5. To uninstall: `installutil.exe /u C:\path\to\Samples.WindowsService.exe`

---

## Related Samples

- **`Samples.WindowsService.NetCore`** — the modern sibling of this project, worth comparing directly.
- **`Samples.InnoSetup`** — packages a Windows Service sample as a proper installer.
