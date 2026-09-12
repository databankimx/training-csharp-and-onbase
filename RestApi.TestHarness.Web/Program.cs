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
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestApi.TestHarness.Web.Infrastructure;
using RestApi.TestHarness.Web.Models;
using Serilog;
#endregion

#region Training Notes
/*
 * *Migration Note: replaces Unity.TestHarness.Web's own Global.asax/Global.asax.cs and
 * App_Start/*.cs entirely (RouteConfig, FilterConfig, BundleConfig): ASP.NET Core has no
 * Global.asax at all, and route/filter/middleware registration happens directly here, in
 * one place, rather than across several App_Start classes wired up from Application_Start.
 *
 * Serilog: unlike Unity.TestHarness.Web (which had to abandon Serilog.Settings.Configuration
 * for a custom JSON-parsing workaround, due to a cascading FileLoadException saga
 * specific to being a legacy net48 Web Application project, see that project's own
 * LectureNotes.md), this project uses the standard, officially-supported
 * Serilog.AspNetCore package directly (UseSerilog(), reading appsettings.json's own
 * "Serilog" section via ReadFrom.Configuration), no workaround needed: modern .NET's own
 * dependency resolution doesn't have net48 Web Application projects' binding-redirect
 * problem.
 *
 * Session state: builder.Services.AddSession() + app.UseSession() is ASP.NET Core's own
 * session middleware, used here purely to obtain a stable, per-browser Session.Id (via
 * its own session cookie) to key SessionManagementStore/SessionLogService by, NOT to
 * store SessionManagement instances directly in it (see SessionManagementStore's own
 * Training Notes for why that's not possible). IdleTimeout is set independently of
 * OrphanedSessionCleanupService's own MaxIdle, the two don't need to match exactly, only
 * be in the same neighborhood.
 *
 * DI registrations: SessionManagementStore and the OrphanedSessionCleanupService are
 * singletons (one shared store/sweeper for the whole app, holding many per-session
 * entries internally); SessionLogService is scoped (one per request, matching
 * IHttpContextAccessor's own lifetime expectations). RestApi.02-04's own OnBaseTaxonomy/
 * DocumentRetrieval/DocumentStorage are NOT registered here at all: every method on them
 * is static (see each project's own Training Notes on that), there's no instance to
 * inject, controllers call them directly instead.
 */
#endregion

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<RestApiWebSettings>(builder.Configuration.GetSection("RestApi"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<SessionManagementStore>();
builder.Services.AddHostedService<OrphanedSessionCleanupService>();
builder.Services.AddScoped<SessionLogService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Connect}/{action=Index}/{id?}");

app.Run();

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
