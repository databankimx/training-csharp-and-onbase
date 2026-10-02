# Samples.WindowsService.NetCore

## What This Is

The modern .NET sibling of `Samples.WindowsService`, built on the Generic Host (`Host.CreateApplicationBuilder`) with a `BackgroundService`. Same task: look up city/county/state by ZIP code on a five-minute timer, reading the ZIP code from a plain text file. The same executable runs as a normal console app during development and as a Windows Service under the SCM in production -- no code change needed between the two.

---

## When to Use This Over the Classic Version

For **any new Windows Service being written today**. The Generic Host approach gives you:

- DI, `IConfiguration`, and structured logging out of the box.
- The same executable runs interactively during development (`dotnet run`) and as a service in production (`sc create` + `sc start`).
- No `ProjectInstaller.cs`, no `installutil.exe` -- just `sc.exe`.
- `PeriodicTimer` (modern .NET) instead of `System.Timers.Timer` + event handler.

The classic `ServiceBase` pattern remains valid for existing services you're maintaining, but has no advantage for new work.

---

## How It Works

One call in `Program.cs` enables Windows Service behavior:

```csharp
builder.Services.AddWindowsService(options =>
    options.ServiceName = "Samples.WindowsService.NetCore");
```

`AddWindowsService()` auto-detects the runtime context: when running interactively, it behaves like a normal console app; when running under the SCM, it behaves as a service. No conditional code, no separate entry points.

The recurring work lives in a `BackgroundService`:

```csharp
public class Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            string zipCode = await File.ReadAllTextAsync(
                @"C:\Temp\Samples.WindowsService.NetCore\zipcode.txt", stoppingToken);
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LocationLookupContext>();
            var results = await db.ZipCodes.Where(z => z.ZipCode1 == zipCode.Trim()).ToListAsync();
            foreach (var row in results)
                logger.LogInformation("{City}, {County}, {State}", row.City, row.County, row.State);
        }
    }
}
```

`PeriodicTimer` + `await timer.WaitForNextTickAsync(stoppingToken)` is cleaner than `System.Timers.Timer` + `Elapsed` event: cancellation is handled automatically, no risk of overlapping executions if a tick fires before the previous one finished.

`IServiceScopeFactory` creates a new DI scope per tick -- the correct pattern for using a scoped service (`DbContext`) from a singleton (`BackgroundService`). Injecting `DbContext` directly into a `BackgroundService` would make it a singleton-scoped context, which isn't safe.

---

## Creating a Windows Service (Modern)

### Visual Studio

**File > New > Project**, search "Worker Service", click Next, choose .NET version, click Create. The scaffolding creates `Program.cs` and `Worker.cs`.

In `Program.cs`, add:

```csharp
builder.Services.AddWindowsService(options => options.ServiceName = "MyService");
```

Add EF Core and register the `DbContext`:

```csharp
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase")));
```

### VS Code

```powershell
dotnet new worker -n MyWindowsService -f net10.0
dotnet add package Microsoft.Extensions.Hosting.WindowsServices
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Serilog.Extensions.Hosting
dotnet add package Serilog.Settings.Configuration
dotnet add package Serilog.Sinks.File
```

Add `builder.Services.AddWindowsService(...)` in `Program.cs`.

---

## Installing and Running

**During development:** `dotnet run` (or F5). Runs as a normal console app.

**As a Windows Service** (after `dotnet publish`):

```
dotnet publish -c Release -r win-x64 --self-contained true

sc create Samples.WindowsService.NetCore binPath="C:\path\to\Samples.WindowsService.NetCore.exe"
sc start Samples.WindowsService.NetCore
```

To stop and remove:

```
sc stop Samples.WindowsService.NetCore
sc delete Samples.WindowsService.NetCore
```

---

## Classic vs. Modern Side by Side

| Classic (`Samples.WindowsService`) | Modern (`Samples.WindowsService.NetCore`) |
|---|---|
| `ServiceBase.OnStart` / `OnStop` | `BackgroundService.ExecuteAsync` loop |
| `System.Timers.Timer` + `Elapsed` event | `PeriodicTimer` + `await WaitForNextTickAsync` |
| `ProjectInstaller.cs` + `installutil.exe` | No installer class -- `sc.exe create` only |
| Cannot run interactively | `dotnet run` works during development |
| No DI | Full DI, `IConfiguration`, Serilog |
| EF6 Database-First | EF Core Code-First |

---

## Related Projects

- `Samples.WindowsService` -- the classic `ServiceBase` sibling for direct comparison.
- `Samples.GenericHostConsole` -- the same Generic Host pattern without the Windows Service parts.
- `Samples.InnoSetup` -- packages this service as a proper installer.
