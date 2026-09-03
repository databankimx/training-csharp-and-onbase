# Samples.GenericHostConsole

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port). See `README.md` for the fuller when-to-use discussion.

---

## The Generic Host Isn't Just for Services

`Samples.WindowsService.NetCore`'s `Program.cs` and this project's `Program.cs` start identically:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog(...);
builder.Services.AddDbContext<LocationLookupContext>(...);
```

They diverge from there. `Samples.WindowsService.NetCore` adds `AddWindowsService()` and `AddHostedService<Worker>()`, then calls `host.Run()`, which blocks forever, repeatedly invoking `Worker.ExecuteAsync` until the service is stopped. This project does none of that: it resolves `LocationLookupRunner` directly, awaits `RunAsync()` once, and lets the process exit normally. Both are legitimate, idiomatic uses of the exact same host, "run forever as a background service" is one thing you can build on top of the Generic Host, not a defining requirement of using it. Any CLI tool, migration script, or scheduled batch job that wants real DI, configuration binding, and structured logging (instead of hand-wiring all of that in `Main()`) is a reasonable candidate for this pattern.

---

## A Real Bug, Fixed: Resolving a Scoped Service From the Root Provider

The first version of this project's `Program.cs` resolved `LocationLookupRunner` directly from `host.Services`:

```csharp
var runner = host.Services.GetRequiredService<LocationLookupRunner>();
```

`host.Services` is the **root** service provider. `LocationLookupContext` is registered scoped (`AddDbContext`'s default lifetime), and `LocationLookupRunner` depends on it directly. Resolving a scoped service from the root provider throws `InvalidOperationException: Cannot resolve scope service '...' from root provider` whenever scope validation is enabled, which it is by default in the `Development` environment specifically to catch exactly this class of bug before it reaches production. **Fixed** by creating one explicit scope for the single run:

```csharp
using (var scope = host.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<LocationLookupRunner>();
    await runner.RunAsync(zipCode);
}
```

This is the exact same underlying fix `Samples.WindowsService.NetCore`'s `Worker` applies via `IServiceScopeFactory.CreateScope()`, just invoked once here instead of once per timer tick, since this project only ever does one unit of work per process run.

---

## Try It Yourself

Run `dotnet run -- 75067` (or any other ZIP code) and watch the console output alongside the Serilog file sink. Then compare `Program.cs` directly against `Samples.WindowsService.NetCore`'s own, same host setup, genuinely different lifetime.
