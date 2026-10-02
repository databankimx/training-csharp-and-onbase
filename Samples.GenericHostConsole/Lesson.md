# Samples.GenericHostConsole

## What This Is

The Generic Host (`Host.CreateApplicationBuilder`) applied to a plain, one-shot console tool. The same host abstraction that `Samples.MvcWebApi.Core`'s `WebApplicationBuilder` and `Samples.WindowsService.NetCore` both sit on top of -- giving this project dependency injection, configuration binding, and structured Serilog logging -- without any web server, service control manager, or always-on loop involved.

Same ZIP code lookup as every other Samples project. The ZIP code comes from a command-line argument or interactive prompt, and the program exits when done.

---

## When to Use This Pattern

For CLI tools, one-off scripts, and scheduled-task-style utilities that would benefit from real DI, configuration, and logging -- but don't need to run continuously. Specifically:

- **Use Generic Host console** when: you want DI and `IConfiguration` without the overhead of a full web framework, and the tool runs once and exits.
- **Use `Samples.WindowsService.NetCore`** when: the tool needs to run continuously, unattended, restarting after failure.
- **Pair with Windows Task Scheduler** for recurring execution: a one-shot process run on a schedule is often simpler to operate and debug than a service managing its own internal timer.

---

## How It Works

```csharp
// Program.cs
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<LocationLookupContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocationLookupDatabase")));
builder.Services.AddTransient<LocationLookupRunner>();

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

var host = builder.Build();

// Create one DI scope, resolve the service, run it, exit
using var scope = host.Services.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<LocationLookupRunner>();
await runner.RunAsync(args);
```

The key difference from `Samples.WindowsService.NetCore`: no `host.Run()`, no `BackgroundService`, no loop. Build the host, create one scope, do the work, exit. The host infrastructure (DI, config, logging) is available without any of the always-on machinery.

`LocationLookupRunner` is a plain injectable class:

```csharp
public class LocationLookupRunner(LocationLookupContext db, ILogger<LocationLookupRunner> logger)
{
    public async Task RunAsync(string[] args)
    {
        string zipCode = args.Length > 0 ? args[0] : PromptForZipCode();
        var results = await db.ZipCodes.Where(z => z.ZipCode1 == zipCode).ToListAsync();
        foreach (var row in results)
            logger.LogInformation("{City}, {County}, {State}", row.City, row.County, row.State);
    }
}
```

---

## Creating a Generic Host Console Project

### Visual Studio

**File > New > Project**, "Console App", choose .NET version, click Create. Then add the host:

```powershell
Install-Package Microsoft.Extensions.Hosting
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Serilog.Extensions.Hosting
Install-Package Serilog.Settings.Configuration
```

Replace `Program.cs` with the `Host.CreateApplicationBuilder` pattern above.

### VS Code

```powershell
dotnet new console -n MyConsoleTool -f net10.0
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Serilog.Extensions.Hosting
dotnet add package Serilog.Settings.Configuration
dotnet add package Serilog.Sinks.File
dotnet run -- 75067
```

---

## Running This Project

1. Point `appsettings.json`'s `LocationLookupDatabase` at a SQL Server instance.
2. Run with a ZIP code argument: `dotnet run -- 75067`
   Or with no argument and enter one when prompted.

---

## Contrast With `Samples.WindowsService.NetCore`

Both projects use `Host.CreateApplicationBuilder` and EF Core. The difference is one method call and one class:

| Generic Host Console | Windows Service |
|---|---|
| `host.Build()`, then scope + run + exit | `host.Run()` -- runs until stopped |
| No `BackgroundService` | `Worker : BackgroundService` with `ExecuteAsync` loop |
| Run on demand or via Task Scheduler | Always running, managed by SCM |

---

## Related Projects

- `Samples.WindowsService.NetCore` -- the same Generic Host pattern extended into a long-running service.
