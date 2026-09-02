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
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

using Microsoft.EntityFrameworkCore;
using Samples.Blazor.Server.Components;
using Samples.Blazor.Server.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// *Migration Note: AddDbContextFactory, not AddDbContext, this is the Microsoft-recommended
//   registration for Blazor Server specifically. A Blazor Server "circuit" (one connected
//   browser tab) is long-lived, potentially for an entire user session, so injecting a
//   scoped DbContext directly into a component would keep ONE instance alive for that
//   whole circuit, the same class of problem as holding a scoped service inside a
//   singleton (see Samples.WindowsService.NetCore's own IServiceScopeFactory usage for the
//   analogous fix in a different context). IDbContextFactory<T> lets Home.razor create a
//   genuinely fresh, short-lived context for each individual search instead. See
//   LectureNotes.md.
builder.Services.AddDbContextFactory<LocationLookupContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocationLookupDatabase")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
