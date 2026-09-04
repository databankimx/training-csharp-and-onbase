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
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Samples.Blazor.WebAssembly;
#endregion

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// *Migration Note: WebAssemblyHostBuilder already fetches wwwroot/appsettings.json over
//   HTTP automatically during startup (the browser downloads it like any other static
//   asset), no manual ConfigurationBuilder wiring needed, unlike Samples.MvcWebApi.Core.Client
//   (a plain, non-hosted console app), this host already provides that. ApiBaseUrl is read
//   from it directly here. See LectureNotes.md.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is not configured in wwwroot/appsettings.json!");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

await builder.Build().RunAsync();
