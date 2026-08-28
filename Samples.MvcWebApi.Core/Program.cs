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
using Samples.MvcWebApi.Core;
using Samples.MvcWebApi.Core.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// *Migration Note: builder.Host.UseSerilog() reads Serilog's own configuration directly from
//   appsettings.json's "Serilog" section, no manual ConfigurationBuilder/AddJsonFile wiring
//   needed the way Samples.MvcWebApi's Global.asax.cs required, WebApplicationBuilder already
//   loads appsettings.json (and appsettings.{Environment}.json, and environment variables)
//   before this line even runs. See LectureNotes.md.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core Code-First, registered for dependency injection, contrast against
//   Samples.MvcWebApi's LocationLookupController, which constructs "new LocationLookupDatabase()"
//   directly rather than receiving it through DI. See LectureNotes.md.
builder.Services.AddDbContext<LocationLookupContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocationLookupDatabase")));

// IExceptionHandler (added in .NET 8) is the modern, built-in replacement for
//   Samples.MvcWebApi's [ExceptionFilter] attribute + DatabankException combination. See
//   GlobalExceptionHandler.cs and LectureNotes.md.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
