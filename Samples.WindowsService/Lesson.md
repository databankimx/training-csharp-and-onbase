# Samples.WindowsService

## What This Is

A classic `net48` Windows Service built on `System.ServiceProcess.ServiceBase`. The Service Control Manager (SCM) starts and stops it, it runs with no interactive user, and it reads its input (the ZIP code to look up) from a plain text file on a five-minute timer.

`Samples.WindowsService.NetCore` is the modern sibling -- same task, Generic Host + `BackgroundService` instead.

---

## When to Use This Over the Modern Version

Only for **existing `ServiceBase`-based services** you're maintaining, or genuine constraints ruling out modern .NET. For any new Windows Service, the Generic Host approach (`Samples.WindowsService.NetCore`) is the better default: less boilerplate, dependency injection, and the same executable doubles as a console app during development.

---

## How `ServiceBase` Works

The SCM calls `OnStart` when the service starts and `OnStop` when it stops. `OnStart` must return quickly -- long-running work is handed off to a `System.Timers.Timer`:

```csharp
public partial class LocationLookupService : ServiceBase
{
    private Timer _timer;

    protected override void OnStart(string[] args)
    {
        _timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    protected override void OnStop()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        string zipCode = File.ReadAllText(@"C:\Temp\Samples.WindowsService\zipcode.txt").Trim();
        // EF6 query, log result
    }
}
```

`Program.cs` is one line: `ServiceBase.Run(new LocationLookupService())`.

**This executable cannot run interactively.** Running it directly throws immediately -- it only runs when started by the SCM after installation.

---

## `ProjectInstaller.cs` and `InstallUtil.exe`

Registration with the SCM is handled by `ProjectInstaller.cs`, a `[RunInstaller(true)]`-decorated class with a `ServiceProcessInstaller`/`ServiceInstaller` pair. This is what `installutil.exe` reads:

```
installutil.exe C:\path\to\Samples.WindowsService.exe
sc start Samples.WindowsService
```

The modern sibling has no `ProjectInstaller.cs` at all -- it uses `sc.exe create` directly.

---

## Creating a Windows Service (Classic)

### Visual Studio

**File > New > Project**, search "Windows Service (.NET Framework)", click Create. The scaffolding generates `Program.cs`, `Service1.cs`, `Service1.Designer.cs`, and `ProjectInstaller.cs` with `ProjectInstaller.Designer.cs`.

Rename `Service1` to your service name in both `.cs` files and in `ProjectInstaller`'s `serviceInstaller1.ServiceName` property.

To add the EF6 model: right-click the project > **Add > New Item > ADO.NET Entity Data Model**, connect to your database, select the `ZipCodes` table.

### VS Code

```powershell
dotnet new worker -n MyWindowsService -f net48
dotnet add package System.ServiceProcess.ServiceController
```

The `worker` template gives a starting structure, but adapting it to classic `ServiceBase` (rather than the modern `BackgroundService`) requires replacing the generated `Worker.cs` with a `ServiceBase`-derived class and adding a `ProjectInstaller.cs` by hand. Easier to create in Visual Studio and edit in VS Code.

---

## Installing and Running

1. Point `App.config`'s `ExternalDataEntities` connection string at a SQL Server instance.
2. Create `C:\Temp\Samples.WindowsService\zipcode.txt` containing a single ZIP code.
3. Build the project.
4. From an **elevated** Command Prompt:

```
installutil.exe C:\path\to\Samples.WindowsService.exe
sc start Samples.WindowsService
```

5. Check the configured log file for results. To uninstall:

```
sc stop Samples.WindowsService
installutil.exe /u C:\path\to\Samples.WindowsService.exe
```

---

## Related Projects

- `Samples.WindowsService.NetCore` -- the modern Generic Host sibling, worth comparing directly.
- `Samples.InnoSetup` -- packages this service as a proper installer.
