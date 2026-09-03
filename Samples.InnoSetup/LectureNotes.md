# Samples.InnoSetup

## What This Is

A fresh addition to `SampleProjects` (not a compilable .NET project), packaging both Windows Service samples as real installers, deliberately chosen as two separate scripts to let the *installer* itself carry forward the same classic/modern contrast the two services already demonstrate. See `README.md` for the build steps.

---

## `InstallUtil.exe` vs. `sc.exe create`: The Installers Mirror the Services

`Samples.WindowsService.iss`'s `[Run]` section invokes `InstallUtil.exe` against the published `.exe`, which is what actually finds and runs `ProjectInstaller.cs`'s `[RunInstaller(true)]`-decorated class (see that project's own `LectureNotes.md`). The installer script itself does almost nothing beyond invoking that mechanism, the real registration logic lives in the compiled assembly.

`Samples.WindowsService.NetCore.iss`'s `[Run]` section calls `sc.exe create` directly, no separate installer assembly involved at all, `AddWindowsService()` in the service's own `Program.cs` is all that's needed on the code side, and `sc.exe` handles registration entirely from the installer script. Genuinely simpler, and worth recognizing as a real, practical benefit of the modern service model, not just a coding-style difference.

---

## A Genuine `sc.exe` Gotcha: The Space After `binPath=`

```
Parameters: "create ""{#ServiceName}"" binPath= ""\""{app}\{#ServiceExe}\"""" start= auto"
```

`sc.exe create`'s `binPath=` parameter **requires a literal space immediately after the equals sign**. `binPath=C:\...` (no space) is silently treated as an unrecognized option, and the whole command fails, often with a confusing error that doesn't obviously point at the missing space. This is a real, well-known `sc.exe` quirk, not specific to Inno Setup, worth knowing if you ever script service registration by hand outside an installer too. The doubled quotes around the path (`\""..\""`) are there because `{app}` (under "Program Files") almost always contains a space itself, and the path needs its own quoting nested inside the already-quoted `Parameters` string.

---

## Why This Isn't a Compilable .NET Project

Inno Setup `.iss` scripts are compiled by the separate Inno Setup Compiler (`ISCC.exe`), a tool entirely outside the .NET/MSBuild ecosystem, there's no `dotnet build` equivalent for this file type. That's why this folder appears in the solution as a group of solution items (the same pattern `Resources`/`Snippets` already use elsewhere in this training set) rather than as a `Project(...)` entry with its own build configuration, there genuinely isn't a build configuration for MSBuild to manage here.

---

## Try It Yourself

Publish both services (see `README.md`), compile both `.iss` scripts, and run both installers on a test machine (or VM). Watch the Services MMC snap-in (`services.msc`) after each install, both services register and start correctly, arrived at through genuinely different mechanisms.
