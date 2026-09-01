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
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Serilog;
using CSharp.SharedLibrary.Models;
using Samples.WindowsService.Models;
#endregion

namespace Samples.WindowsService
{
    #region Training Notes
    /*
     * Classic System.ServiceProcess.ServiceBase is an EVENT-DRIVEN model, genuinely
     * different from Samples.WindowsService.NetCore's BackgroundService:
     *
     * - OnStart(string[] args) is called ONCE by the Service Control Manager when the
     *   service starts (sc start, or the Services MMC snap-in), and must return quickly,
     *   the SCM expects a prompt response and will report a start failure if OnStart
     *   blocks too long. Long-running/recurring work has to be handed off to something
     *   else, a Timer here, a background Thread in other real services, OnStart itself
     *   is not where that work runs.
     *
     * - OnStop() is called when the service is asked to stop, and is responsible for
     *   cleanly halting whatever OnStart started, there's no CancellationToken flowing
     *   through automatically the way BackgroundService.ExecuteAsync gets one.
     *
     * - There's no PeriodicTimer here (a modern .NET-only API), the classic, still
     *   entirely valid net48 equivalent is System.Timers.Timer with an Elapsed event
     *   handler, wired up in OnStart and disposed in OnStop.
     *
     * - No dependency injection, no scoped services, no IServiceScopeFactory. Each timer
     *   tick constructs its own ExternalDataEntities directly (a using block), the same
     *   "one DbContext per unit of work" LIFETIME as the .NetCore sibling's DI-scoped
     *   context achieves, just without a container managing it.
     */
    #endregion

    /// <summary>
    /// Periodically checks the <c>ZipCodes</c> table for rows with missing data (a null or
    /// empty <c>State</c>, <c>County</c>, <c>City</c>, or ZIP code) and logs what it finds,
    /// the same task as <see cref="Samples.WindowsService.NetCore.Worker"/>, expressed in
    /// the classic ServiceBase model instead.
    /// </summary>
    public partial class DataHealthCheckService : ServiceBase
    {
        #region Fields
        // How often to run the check. A real service would likely read this from
        // configuration rather than hardcoding it; kept simple here for the sample.
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

        // System.Timers.Timer, not PeriodicTimer, see Training Notes above.
        private Timer checkTimer;

        // Serilog logging utility
        private ILogger logger;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DataHealthCheckService"/> class.
        /// </summary>
        public DataHealthCheckService()
        {
            InitializeComponent();
        }
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Called by the Service Control Manager when the service is started. Sets up
        /// logging and starts the recurring data-health check timer; does NOT run the
        /// check itself, this method must return promptly.
        /// </summary>
        /// <param name="args">Any command-line arguments passed to the service.</param>
        protected override void OnStart(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("serilog.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            logger = Log.Logger;

            logger.Information("Samples.WindowsService starting...");

            checkTimer = new Timer(CheckInterval.TotalMilliseconds);
            checkTimer.Elapsed += CheckTimer_Elapsed;
            checkTimer.AutoReset = true;
            checkTimer.Start();

            // Run one check immediately on startup too, rather than waiting a full
            // interval for the first result.
            CheckDataHealth();
        }

        /// <summary>
        /// Called by the Service Control Manager when the service is asked to stop.
        /// Cleanly halts the recurring check timer.
        /// </summary>
        protected override void OnStop()
        {
            checkTimer?.Stop();
            checkTimer?.Dispose();
            logger?.Information("Samples.WindowsService stopped.");
            Log.CloseAndFlush();
        }
        #endregion

        #region Event Handlers
        // Timer.Elapsed fires on a thread pool thread, NOT the same thread OnStart ran on,
        // worth knowing since it means CheckDataHealth() must be safe to run concurrently
        // with itself if a check somehow takes longer than CheckInterval, a real
        // consideration System.Timers.Timer leaves to the implementer, unlike
        // PeriodicTimer's WaitForNextTickAsync loop, which naturally can't overlap itself.
        private void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckDataHealth();
        }
        #endregion

        #region Helper Functions
        // Runs a single data-health check.
        private void CheckDataHealth()
        {
            try
            {
                using (var db = new ExternalDataEntities())
                {
                    var zipCodes = db.ZipCodes.ToList();
                    var incompleteCount = zipCodes.Count(z =>
                        string.IsNullOrWhiteSpace(z.State)
                        || string.IsNullOrWhiteSpace(z.County)
                        || string.IsNullOrWhiteSpace(z.City)
                        || string.IsNullOrWhiteSpace(z.ZipCode1));

                    if (incompleteCount > 0)
                        logger.Warning("Data health check: {IncompleteCount} of {TotalCount} ZipCodes rows have missing data.", incompleteCount, zipCodes.Count);
                    else
                        logger.Information("Data health check: all {TotalCount} ZipCodes rows are complete.", zipCodes.Count);
                }
            }
            catch (Exception ex)
            {
                var wrapped = new DatabankException("Data health check failed.", ex);
#pragma warning disable S6667
                logger.Error(wrapped, "Data health check failed.");
#pragma warning restore S6667
            }
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
