# Samples.WindowsService

## What This Is

The `net48` baseline of the Windows Service pair, added alongside `Samples.WindowsService.NetCore`. See `README.md` for the fuller when-to-use discussion.

---

## Same Task as Every Other Sample, Deliberately

Every other project in this training set looks up city/county/state by ZIP code. This project (and `Samples.WindowsService.NetCore`) do the exact same lookup, so the only thing that varies across the whole training set is the coding pattern (postback, MVC action, Razor Page handler, MVVM binding, WinForms event, `ServiceBase`/`BackgroundService`), never the underlying task. An earlier version of this service did something else entirely (a "data health check" scanning the whole `ZipCodes` table for incomplete rows), a reasonable-sounding idea on its own, but a real inconsistency against the rest of the training set, corrected here.

**Where does the ZIP code come from, though?** Every interactive sample gets one from a user (a form field, a search box). A Windows Service has no interactive caller. `LocationLookupService.LookupLocation()` reads it from a plain text file instead, `C:\Temp\Samples.WindowsService\zipcode.txt`, re-read fresh on every timer tick, so changing the file's contents changes what the *next* scheduled lookup searches for, no restart needed. A registry value (`HKEY_LOCAL_MACHINE\...`) is another common, legitimate way to configure an unattended service like this; a file was used here specifically because it needs no elevated permissions to read or write, and works identically on both this project and `Samples.WindowsService.NetCore`.

---

## `OnStart` Must Return Quickly, `System.Timers.Timer` Does the Real Work

```csharp
protected override void OnStart(string[] args)
{
    // ... logging setup ...
    lookupTimer = new Timer(LookupInterval.TotalMilliseconds);
    lookupTimer.Elapsed += LookupTimer_Elapsed;
    lookupTimer.Start();
    LookupLocation();
}
```

The Service Control Manager calls `OnStart` once, expects it to return promptly, and will report a start failure if it blocks too long. The actual recurring work (`LookupLocation()`) runs on a `System.Timers.Timer`'s `Elapsed` event instead, which fires on a thread-pool thread, **not** the thread `OnStart` ran on. Worth knowing this means `LookupLocation()` needs to be safe to run concurrently with itself if a lookup somehow takes longer than the interval, a real consideration `System.Timers.Timer` leaves to the implementer, unlike `Samples.WindowsService.NetCore`'s `PeriodicTimer.WaitForNextTickAsync` loop, which naturally can't overlap itself.

---

## `InstallUtil.exe`: The Classic Installation Mechanism

```csharp
[RunInstaller(true)]
public partial class ProjectInstaller : Installer
```

`installutil.exe` (shipped with the .NET Framework, not modern .NET) finds a class decorated with `[RunInstaller(true)]` in the target assembly and runs its `Install()`/`Uninstall()` logic, which in turn runs whatever `Installer`-derived components are attached (`ProjectInstaller.Designer.cs`: a `ServiceProcessInstaller`, controlling the account the service runs as, and a `ServiceInstaller`, controlling its name, display name, and start type). `Samples.WindowsService.NetCore` has no file like this at all, `AddWindowsService()` plus a plain `sc create` command replaces the entire mechanism.

One coupling worth being deliberate about: `ProjectInstaller.Designer.cs`'s `serviceInstaller1.ServiceName` **must** match `LocationLookupService.Designer.cs`'s own `this.ServiceName` assignment, the Service Control Manager uses this string to associate an installed service registration with the actual `ServiceBase` implementation that handles it. Get these out of sync and the service installs but the SCM can't find anything to actually run.

---

## Can't Run Interactively, By Design

Unlike `Samples.WindowsService.NetCore` (where `AddWindowsService()` auto-detects whether it's running as a console app or an installed service), `Program.cs`'s `ServiceBase.Run(...)` here has no such detection. Launching this executable directly (F5, double-click, `dotnet Samples.WindowsService.dll`) throws an `InvalidOperationException` the moment `ServiceBase.Run()` executes, "Cannot start service ... because the process is not running as a Windows service." This is genuinely how classic Windows Services work: they can only be started by the Service Control Manager, after installation, see `README.md` for the actual `installutil.exe`/`sc start` steps.

---

## Try It Yourself

Install both services (see `README.md`), start each, and compare their behavior directly: create the input file for one, watch its log pick up the result on the next interval, then edit the file and watch the *next* result change without restarting anything. Then try stopping each service and note how `OnStop()`'s explicit `lookupTimer.Stop()`/`Dispose()` compares against `Samples.WindowsService.NetCore`'s automatic `CancellationToken`-based shutdown.
