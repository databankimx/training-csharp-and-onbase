# Samples.WindowsService

## What This Is

The `net48` baseline of the Windows Service pair, added alongside `Samples.WindowsService.NetCore` (renamed from a plain `Samples.WindowsService` once this baseline existed, correcting an earlier inconsistency, that project was originally built directly on `net10.0` with no `net48` sibling, which didn't match this solution's actual policy). See `README.md` for the fuller when-to-use discussion.

---

## `OnStart` Must Return Quickly, `System.Timers.Timer` Does the Real Work

```csharp
protected override void OnStart(string[] args)
{
    // ... logging setup ...
    checkTimer = new Timer(CheckInterval.TotalMilliseconds);
    checkTimer.Elapsed += CheckTimer_Elapsed;
    checkTimer.Start();
    CheckDataHealth();
}
```

The Service Control Manager calls `OnStart` once, expects it to return promptly, and will report a start failure if it blocks too long. The actual recurring work (`CheckDataHealth()`) runs on a `System.Timers.Timer`'s `Elapsed` event instead, which fires on a thread-pool thread, **not** the thread `OnStart` ran on. Worth knowing this means `CheckDataHealth()` needs to be safe to run concurrently with itself if a check somehow takes longer than the interval, a real consideration `System.Timers.Timer` leaves to the implementer, unlike `Samples.WindowsService.NetCore`'s `PeriodicTimer.WaitForNextTickAsync` loop, which naturally can't overlap itself (`while (await timer.WaitForNextTickAsync())` doesn't tick again until the previous iteration completes).

---

## `InstallUtil.exe`: The Classic Installation Mechanism

```csharp
[RunInstaller(true)]
public partial class ProjectInstaller : Installer
```

`installutil.exe` (shipped with the .NET Framework, not modern .NET) finds a class decorated with `[RunInstaller(true)]` in the target assembly and runs its `Install()`/`Uninstall()` logic, which in turn runs whatever `Installer`-derived components are attached (`ProjectInstaller.Designer.cs`: a `ServiceProcessInstaller`, controlling the account the service runs as, and a `ServiceInstaller`, controlling its name, display name, and start type). `Samples.WindowsService.NetCore` has no file like this at all, `AddWindowsService()` plus a plain `sc create` command replaces the entire mechanism.

One coupling worth being deliberate about: `ProjectInstaller.Designer.cs`'s `serviceInstaller1.ServiceName` **must** match `DataHealthCheckService.Designer.cs`'s own `this.ServiceName` assignment, the Service Control Manager uses this string to associate an installed service registration with the actual `ServiceBase` implementation that handles start/stop requests for it. Get these out of sync and the service installs but the SCM can't find anything to actually run.

---

## Can't Run Interactively, By Design

Unlike `Samples.WindowsService.NetCore` (where `AddWindowsService()` auto-detects whether it's running as a console app or an installed service), `Program.cs`'s `ServiceBase.Run(...)` here has no such detection. Launching this executable directly (F5, double-click, `dotnet Samples.WindowsService.dll`) throws an `InvalidOperationException` the moment `ServiceBase.Run()` executes, "Cannot start service ... because the process is not running as a Windows service." This is genuinely how classic Windows Services work: they can only be started by the Service Control Manager, after installation, see `README.md` for the actual `installutil.exe`/`sc start` steps.

---

## Try It Yourself

Install both services (see `README.md`), start each, and compare their behavior directly: watch each one's log file populate on the same five-minute interval, then try stopping each one and note how `OnStop()`'s explicit `checkTimer.Stop()`/`Dispose()` compares against `Samples.WindowsService.NetCore`'s automatic `CancellationToken`-based shutdown.
