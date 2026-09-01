# Samples.WindowsService.NetCore

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

The modern .NET sibling of `Samples.WindowsService`, built on the Generic Host (`Host.CreateApplicationBuilder`, the same host abstraction `Samples.MvcWebApi.Core`'s `WebApplicationBuilder` sits on top of) with a `BackgroundService`. Like every other sample in this training set, it looks up city/county/state by ZIP code, the ZIP code to look up is read from a plain text file (`C:\Temp\Samples.WindowsService.NetCore\zipcode.txt`) on a five-minute timer, since a Windows Service has no interactive user to supply one on demand.

`Samples.InnoSetup` later packages a Windows Service sample's published output as a real installer.

---

## When to Use This Over the Classic Version

For any new Windows Service being written today. `net48`'s classic `ServiceBase` pattern remains valid for existing services, but offers no advantage over the Generic Host approach for something new, see `LectureNotes.md` for exactly what's different and why.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Host setup: `AddWindowsService`, Serilog, EF Core, the hosted `Worker` |
| `Worker.cs` | The `BackgroundService`, runs the periodic location lookup |
| `Models/ZipCode.cs` | EF Core entity (Code-First) |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |

---

## How to Run

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Create `C:\Temp\Samples.WindowsService.NetCore\zipcode.txt` containing a single ZIP code, e.g. `75067`.

**During development**, just `dotnet run` (or F5), it runs as a normal console app.

**As an installed service** (after `dotnet publish`):
```
sc create Samples.WindowsService.NetCore binPath="C:\path\to\Samples.WindowsService.NetCore.exe"
sc start Samples.WindowsService.NetCore
```

Check the log output for the lookup results. Edit the text file and wait for the next interval to see the service pick up the change.

---

## Related Samples

- **`Samples.WindowsService`** — the classic `net48` sibling of this project, worth comparing directly.
- **`Samples.InnoSetup`** — packages this project's published output as a proper Windows installer.
- **`Samples.GenericHostConsole`** — the same Generic Host pattern applied to a plain console app, without the Windows Service-specific pieces.
