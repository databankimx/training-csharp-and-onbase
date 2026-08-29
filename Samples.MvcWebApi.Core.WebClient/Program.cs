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

// *Migration Note: the entire hosting setup for this project, three lines. The classic web
//   clients each needed a full legacy Web Application Project (a .csproj with IIS Express
//   settings, project type GUIDs, a Web.config) purely to get static files served at all.
//   ASP.NET Core's minimal hosting model does the same job directly. See LectureNotes.md.
var app = WebApplication.CreateBuilder(args).Build();

app.UseDefaultFiles();
app.UseStaticFiles();

#pragma warning disable S6966 // Using app.Run() here is acceptable.
app.Run();
#pragma warning restore S6966

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
