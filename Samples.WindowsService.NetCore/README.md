# Samples.WindowsService.NetCore

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

The modern .NET sibling of `Samples.WindowsService`, built on the Generic Host (`Host.CreateApplicationBuilder`, the same host abstraction `Samples.MvcWebApi.Core`'s `WebApplicationBuilder` sits on top of) with a `BackgroundService`. Originally this project existed alone, named plainly `Samples.WindowsService`, built directly on `net10.0` with no `net48` baseline; it was renamed to `.NetCore` once a genuine classic sibling was added, giving this project something real to be compared against. See `Samples.WindowsService`'s own `README.md`/`LectureNotes.md` for the classic `net48` version, and this project's `LectureNotes.md` for the specific differences.

It periodically checks the `ZipCodes` table for rows with missing data and logs what it finds, a genuine, if simple, example of the recurring background maintenance task a Windows Service is actually used for.

`Samples.InnoSetup` later packages this project's published output as a real installer.

---

## When to Use This Over the Classic Version

For any new Windows Service being written today. `net48`'s classic `ServiceBase` pattern remains valid for existing services, but offers no advantage over the Generic Host approach for something new, see `LectureNotes.md` for exactly what's different and why.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Host setup: `AddWindowsService`, Serilog, EF Core, the hosted `Worker` |
| `Worker.cs` | The `BackgroundService`, runs the periodic data-health check |
| `Models/ZipCode.cs` | EF Core entity (Code-First), nullable properties (the check is specifically looking for incomplete rows) |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |

---

## How to Run

**During development**, just `dotnet run` (or F5), it runs as a normal console app.

**As an installed service** (after `dotnet publish`):
```
sc create Samples.WindowsService.NetCore binPath="C:\path\to\Samples.WindowsService.NetCore.exe"
sc start Samples.WindowsService.NetCore
```

Either way, point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance first.

---

## Related Samples

- **`Samples.WindowsService`** — the classic `net48` sibling of this project, worth comparing directly.
- **`Samples.InnoSetup`** — packages this project's published output as a proper Windows installer.
- **`Samples.GenericHostConsole`** — the same Generic Host pattern applied to a plain console app, without the Windows Service-specific pieces.
