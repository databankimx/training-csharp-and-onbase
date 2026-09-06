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

namespace Unity.TestHarness
{
    #region Training Notes
    /*
     * *Migration Note: Log.Logger is Serilog's own static, process-wide logger, set up
     * once here at startup and used from anywhere (LogViewModel, in this app) without
     * needing to pass a logger instance around. Configuration comes from serilog.json
     * (via Serilog.Settings.Configuration's ReadFrom.Configuration), rather than being
     * hardcoded here in C#, so the logging setup (levels, sinks, rolling behavior) can be
     * changed without a recompile, matching how a real deployed app would want to tune
     * logging in the field. CloseAndFlush() on Exit ensures buffered log entries
     * (especially the File sinks) are actually written before the process ends, Serilog
     * batches writes for performance and doesn't guarantee everything's on disk until
     * flushed.
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

            Log.Information("Unity Test Harness starting up.");

            base.OnStartup(e);
        }

        /// <inheritdoc />
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Unity Test Harness shutting down.");
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
