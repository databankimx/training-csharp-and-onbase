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
using System.Windows;
using Microsoft.Extensions.Configuration;
using Serilog;
#endregion

namespace RestApi.TestHarness
{
    #region Training Notes
    /*
     * *Migration Note: identical to Unity.TestHarness's own App.xaml.cs (same Serilog
     * bootstrap: Log.Logger set up before base.OnStartup() creates/shows MainWindow,
     * CloseAndFlush() on exit). See that project's own Training Notes for why. The
     * Serilog.Settings.Configuration + Microsoft.Extensions.Configuration combination
     * that caused a whole cascading FileLoadException saga on Unity.TestHarness.Web (a
     * legacy net48 ASP.NET Web Application project) is NOT expected to recur here: that
     * saga was specifically about AutoGenerateBindingRedirects not reliably covering a
     * deep transitive dependency chain under that particular net48 project type. This
     * project targets net10.0-windows, modern .NET's own dependency resolution doesn't
     * have the same binding-redirect problem net48 Web Application projects did.
     */
    #endregion

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            // Configured BEFORE calling base.OnStartup(): that base call is what actually
            // creates/shows the StartupUri window (MainWindow), Serilog needs to already
            // be ready before that happens, in case anything during window/view model
            // construction tries to log.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("serilog.json", optional: false, reloadOnChange: false)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Information("RestApi Test Harness starting up.");

            base.OnStartup(e);
        }

        /// <inheritdoc />
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("RestApi Test Harness shutting down.");
            Log.CloseAndFlush();

            base.OnExit(e);
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
