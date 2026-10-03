# Samples.InnoSetup

## What This Is

Two [Inno Setup](https://jrsoftware.org/isinfo.php) scripts that package the Windows Service samples as proper Windows installers with a GUI wizard, service registration, uninstall support, and optional prerequisite checks. Not a .NET project - Inno Setup scripts are Pascal-based and compiled by the Inno Setup Compiler (`ISCC.exe`), not MSBuild.

| Script | Packages | Registration |
|---|---|---|
| `Samples.WindowsService.iss` | `Samples.WindowsService` (`net48`) | `installutil.exe` |
| `Samples.WindowsService.NetCore.iss` | `Samples.WindowsService.NetCore` (`net10.0`) | `sc.exe create` |

---

## When to Use Inno Setup

When you need to distribute a Windows application (a service, a desktop app, a CLI tool) with a proper installer experience: a wizard that handles installation directory selection, shortcut creation, service registration, and uninstallation - all without the user needing to run command-line tools. Inno Setup is free, mature, actively maintained, and widely used for internal Windows software distribution.

Alternatives: WiX Toolset (XML-based, integrates with MSBuild, more complex), NSIS (another free option), or Visual Studio's own Setup Project extension (limited, rarely used for serious packaging).

---

## How Inno Setup Scripts Work

An `.iss` script is divided into named sections. The two scripts here mirror the same classic/modern contrast the two services already demonstrate - the installer mechanism changes alongside the service registration mechanism:

**Classic (`Samples.WindowsService.iss`) - uses `installutil.exe`:**

```pascal
[Setup]
AppName=Samples Windows Service
AppVersion=1.0.0
DefaultDirName={autopf}\DataBank\Samples.WindowsService
PrivilegesRequired=admin

[Files]
Source: "..\Samples.WindowsService\bin\Release\*"; DestDir: "{app}"; Flags: recursesubdirs

[Run]
Filename: "{dotnet40}\installutil.exe"; \
    Parameters: """{app}\Samples.WindowsService.exe"""; \
    StatusMsg: "Registering Windows Service..."; \
    Flags: runhidden waituntilterminated

[UninstallRun]
Filename: "sc.exe"; Parameters: "stop Samples.WindowsService"; Flags: runhidden
Filename: "{dotnet40}\installutil.exe"; \
    Parameters: "/u ""{app}\Samples.WindowsService.exe"""; \
    Flags: runhidden waituntilterminated
```

**Modern (`Samples.WindowsService.NetCore.iss`) - uses `sc.exe create`:**

The `[Run]` section calls `sc.exe create` directly instead of `installutil.exe`, and `[UninstallRun]` calls `sc.exe delete`. No separate installer assembly is involved at all - `AddWindowsService()` in `Samples.WindowsService.NetCore`'s `Program.cs` is all that's needed on the code side.

One real `sc.exe` gotcha worth knowing: `binPath=` requires a literal space immediately after the equals sign. `binPath=C:\...` (no space) is silently treated as an unrecognized option and the whole command fails. The scripts handle this correctly; keep it in mind if you ever script service registration by hand.

---

## Building an Installer

### Prerequisites

1. Install [Inno Setup](https://jrsoftware.org/isdl.php) (free).
2. Publish the service being packaged first:

**Classic (`net48`):**
```powershell
dotnet publish ..\Samples.WindowsService\Samples.WindowsService.csproj -c Release
```

**Modern (`net10.0`, self-contained):**
```powershell
dotnet publish ..\Samples.WindowsService.NetCore\Samples.WindowsService.NetCore.csproj `
    -c Release -r win-x64 --self-contained true
```

### Compile the Installer

**In the Inno Setup IDE:** open the `.iss` file, press F9 (or Build > Compile).

**From the command line** (useful for CI):
```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Samples.WindowsService.iss
```

The compiled installer `.exe` appears in the `Output\` folder specified in the script's `[Setup]` section.

### VS Code

Edit `.iss` files in VS Code - the [InnoSetup](https://marketplace.visualstudio.com/items?itemName=idleberg.innosetup) extension provides syntax highlighting and snippets. Compilation still requires `ISCC.exe` to be installed; trigger it via the integrated terminal.

---

## Running the Installer

1. Run the compiled `.exe` as Administrator (both scripts declare `PrivilegesRequired=admin`).
2. Follow the wizard. The `[Run]` section registers the service automatically after files are copied.
3. To uninstall: Control Panel > Programs > Uninstall, or run `unins000.exe` from the installation directory. The `[UninstallRun]` section stops and unregisters the service first.

---

## Key Inno Setup Concepts

| Directive | Meaning |
|---|---|
| `{app}` | The selected installation directory |
| `{autopf}` | `Program Files` or `Program Files (x86)` automatically |
| `{dotnet40}` | The .NET Framework 4.x directory (for `installutil.exe`) |
| `Flags: runhidden` | Run without showing a command window |
| `Flags: waituntilterminated` | Wait for the command to complete before continuing |
| `PrivilegesRequired=admin` | Forces elevation (UAC prompt) at installer startup |

---

## Takeaways

- Inno Setup scripts are Pascal-based and compiled by `ISCC.exe`, not MSBuild. This project appears in the solution as a group of solution items, not a buildable project.
- The two scripts mirror the two services: `installutil.exe` for the classic `net48` service, `sc.exe create` for the modern `net10.0` service.
- `sc.exe create`'s `binPath=` requires a literal space after the equals sign - omitting it fails silently.
- Always publish the service first before compiling the installer. The `[Files]` section copies from the publish output.
- `PrivilegesRequired=admin` forces a UAC prompt at installer launch - required for service registration on any modern Windows system.

---

## Related Projects

- `Samples.WindowsService` -- the classic `net48` service packaged by `Samples.WindowsService.iss`.
- `Samples.WindowsService.NetCore` -- the modern `net10.0` service packaged by `Samples.WindowsService.NetCore.iss`.
