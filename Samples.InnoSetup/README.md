# Samples.InnoSetup

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Two [Inno Setup](https://jrsoftware.org/isinfo.php) scripts, each packaging one of the Windows Service samples in this training set as a proper Windows installer:

| Script | Packages | Registration mechanism |
|---|---|---|
| `Samples.WindowsService.iss` | `Samples.WindowsService` (classic, `net48`) | `InstallUtil.exe` |
| `Samples.WindowsService.NetCore.iss` | `Samples.WindowsService.NetCore` (modern, `net10.0`) | `sc.exe create` |

Not a compilable .NET project, Inno Setup scripts are compiled by the separate Inno Setup Compiler (`ISCC.exe`), not `dotnet build`/MSBuild, so this folder is represented in the solution as a group of solution items rather than a project.

---

## How to Build an Installer

1. Install [Inno Setup](https://jrsoftware.org/isdl.php) (free) if you haven't already.
2. Publish the service you want to package first:
   - Classic: `dotnet publish ..\Samples.WindowsService\Samples.WindowsService.csproj -c Release`
   - .NET Core: `dotnet publish ..\Samples.WindowsService.NetCore\Samples.WindowsService.NetCore.csproj -c Release -r win-x64 --self-contained true`
3. Open the matching `.iss` file in the Inno Setup Compiler (or run `ISCC.exe Samples.WindowsService.iss` from the command line) and build.
4. Run the resulting installer (as Administrator, both scripts require elevation) to install and register the service, or its uninstaller to stop and unregister it.

---

## Related Samples

- **`Samples.WindowsService`** / **`Samples.WindowsService.NetCore`** — the two services these scripts package.
