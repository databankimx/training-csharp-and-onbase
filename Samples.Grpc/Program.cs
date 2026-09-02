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

using Microsoft.EntityFrameworkCore;
using Samples.Grpc.Data;
using Samples.Grpc.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// *Migration Note: AddGrpc() adds the framework that dispatches incoming HTTP/2 requests
//   to the generated LocationLookupServiceBase methods. This genuinely requires HTTP/2,
//   see Samples.Grpc.csproj for why that rules out a net48 baseline for this sample.
builder.Services.AddGrpc();

builder.Services.AddDbContextFactory<LocationLookupContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocationLookupDatabase")));

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapGrpcService<LocationLookupServiceImpl>();
app.MapGet("/", () => "This is a gRPC endpoint, it must be accessed through a gRPC client, not a browser. See Samples.Grpc.Client.");

app.Run();
