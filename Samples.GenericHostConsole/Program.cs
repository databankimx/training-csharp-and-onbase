#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using Microsoft.EntityFrameworkCore;
using Samples.GenericHostConsole.Data;
using Samples.GenericHostConsole.Services;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
#endregion

#region Training Notes
/*
 * Compare this file directly against Samples.WindowsService.NetCore's own Program.cs:
 *
 * - Both call Host.CreateApplicationBuilder(args), both register Serilog and an EF Core
 *   DbContext the same way.
 * - Samples.WindowsService.NetCore ALSO calls AddWindowsService() and
 *   AddHostedService<Worker>(), then host.Run(), which blocks forever running Worker's
 *   ExecuteAsync loop until the service is stopped.
 * - THIS project does neither. host.Services.GetRequiredService<...>() resolves
 *   LocationLookupRunner directly, RunAsync() is awaited ONCE, and the process exits
 *   normally afterward, no AddHostedService, no host.Run()/RunAsync() at all.
 *
 * Both are equally legitimate uses of the Generic Host, it's a general-purpose
 * application host providing DI, configuration, and logging, "run forever as a service"
 * is one thing you can build on top of it, not a requirement for using it at all.
 *
 * *Fixed*: LocationLookupContext is registered SCOPED (AddDbContext's default), but
 * host.Services is the ROOT provider, resolving a scoped service directly from it throws
 * "Cannot resolve scope service ... from root provider" whenever scope validation is
 * enabled (the default in the Development environment). host.Services.CreateScope()
 * below creates one explicit scope for this single run, exactly the same fix
 * Samples.WindowsService.NetCore's Worker applies via IServiceScopeFactory, just called
 * once here instead of once per timer tick. See LectureNotes.md.
 */
#endregion

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddDbContext<LocationLookupContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocationLookupDatabase")));

builder.Services.AddTransient<LocationLookupRunner>();

using var host = builder.Build();

string? zipCode = args.Length > 0 ? args[0] : null;
if (string.IsNullOrWhiteSpace(zipCode))
{
    Console.WriteLine("Enter a zip code to search...");
    zipCode = Console.ReadLine();
}

if (string.IsNullOrWhiteSpace(zipCode)) zipCode = "75067";

using (var scope = host.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<LocationLookupRunner>();
    await runner.RunAsync(zipCode);
}

await Log.CloseAndFlushAsync();
