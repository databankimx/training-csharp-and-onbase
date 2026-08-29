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
using Samples.MvcWebApi.Core;
using Samples.MvcWebApi.Core.Data;
using Serilog;
#endregion

#region Main Program
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

// *Migration Note: the classic projects (Samples.AsmxWebService, Samples.WcfService,
//   Samples.MvcWebApi) all handle CORS by hand in Global.asax's Application_BeginRequest,
//   manually adding Access-Control-Allow-Origin/-Methods/-Headers response headers. ASP.NET
//   Core has no Global.asax at all, and its own CORS middleware (AddCors/UseCors) is the
//   correct, built-in replacement, added here specifically so
//   Samples.MvcWebApi.Core.WebClient (a different origin/port) can call this API from the
//   browser.
//
//   *Fixed*: the original version of this used AllowAnyOrigin(), a genuine static-analysis
//   finding (csharpsquid:S5122, "Restrict this CORS policy to trusted origins"), not a false
//   positive, wide-open CORS lets ANY website's JavaScript read this API's responses, not
//   just the one WebClient that's actually supposed to call it. Restricted to the specific
//   origin(s) that genuinely need access instead, read from configuration rather than
//   hardcoded, since a hardcoded port here would be exactly the kind of thing that breaks
//   again the next time a port has to change (see LectureNotes.md for the port-conflict saga
//   that motivated this). See LectureNotes.md.
var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(corsAllowedOrigins)
    .AllowAnyMethod()
    .AllowAnyHeader()));

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
app.UseCors();
app.UseAuthorization();
app.MapControllers();

#pragma warning disable S6966 // Generated Program.cs is the entry point, so app.Run() is required
app.Run();
#pragma warning restore S6966
#endregion

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
