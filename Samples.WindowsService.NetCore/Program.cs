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

#region Using Directives
using Microsoft.EntityFrameworkCore;
using Samples.WindowsService.NetCore;
using Samples.WindowsService.NetCore.Data;
using Serilog;
#endregion

#region Main Program
var builder = Host.CreateApplicationBuilder(args);

// *Migration Note: UseWindowsService() is what makes this executable behave correctly as an
//   installed Windows Service (responding to Start/Stop/Shutdown control requests) while
//   ALSO running normally as a plain console app during development (dotnet run). It
//   auto-detects the context, no separate build configuration or code path needed. See
//   LectureNotes.md.
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Samples.WindowsService.NetCore";
});

builder.Services.AddSerilog((services, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddDbContext<LocationLookupContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocationLookupDatabase")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
#pragma warning disable S6966
host.Run();
#pragma warning restore S6966
#endregion

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
