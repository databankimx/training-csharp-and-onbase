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
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using SysConfig = System.Configuration;
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
     *
     * *Migration Note: this service performs the SAME core task as every other sample in
     * this training set, look up city/county/state by ZIP code, deliberately, so the only
     * thing that varies across samples is the coding pattern, not the underlying logic. A
     * Windows Service has no interactive caller to supply a ZIP code on demand, so the ZIP
     * code to look up is instead read from a plain text file at ZipCodeInputPath (see
     * Fields below) on every timer tick, a genuine, realistic pattern for unattended
     * services (an HKEY_LOCAL_MACHINE registry value is another common choice for this
     * same purpose, a file was used here specifically because it needs no elevated
     * permissions and behaves identically on both net48 and net10.0). See LectureNotes.md.
     */
    #endregion

    /// <summary>
    /// Periodically reads a ZIP code from a text file and looks up its city/county/state,
    /// the same task as <see cref="Samples.WindowsService.NetCore.Worker"/>, expressed in
    /// the classic ServiceBase model instead.
    /// </summary>
    public partial class LocationLookupService : ServiceBase
    {
        #region Fields
        // How often to check the input file and perform a lookup. A real service would
        // likely read this from configuration rather than hardcoding it; kept simple here
        // for the sample.
        private static readonly TimeSpan LookupInterval = TimeSpan.FromMinutes(5);

        // Where the ZIP code to look up is read from, one plain line of text, re-read on
        // every timer tick so an operator can change it without restarting the service.
        private static readonly string ZipCodeInputPath = SysConfig.ConfigurationManager.AppSettings["zipCodeFilePath"];

        // System.Timers.Timer, not PeriodicTimer, see Training Notes above.
        private Timer lookupTimer;

        // Serilog logging utility
        private ILogger logger;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationLookupService"/> class.
        /// </summary>
        public LocationLookupService()
        {
            InitializeComponent();
        }
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Called by the Service Control Manager when the service is started. Sets up
        /// logging and starts the recurring lookup timer; does NOT run a lookup itself,
        /// this method must return promptly.
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

            logger.Information("Samples.WindowsService starting, reading ZIP code from {Path}...", ZipCodeInputPath);

            lookupTimer = new Timer(LookupInterval.TotalMilliseconds);
            lookupTimer.Elapsed += LookupTimer_Elapsed;
            lookupTimer.AutoReset = true;
            lookupTimer.Start();

            // Run one lookup immediately on startup too, rather than waiting a full
            // interval for the first result.
            LookupLocation();
        }

        /// <summary>
        /// Called by the Service Control Manager when the service is asked to stop.
        /// Cleanly halts the recurring lookup timer.
        /// </summary>
        protected override void OnStop()
        {
            lookupTimer?.Stop();
            lookupTimer?.Dispose();
            logger?.Information("Samples.WindowsService stopped.");
            Log.CloseAndFlush();
        }
        #endregion

        #region Event Handlers
        // Timer.Elapsed fires on a thread pool thread, NOT the same thread OnStart ran on,
        // worth knowing since it means LookupLocation() must be safe to run concurrently
        // with itself if a lookup somehow takes longer than LookupInterval, a real
        // consideration System.Timers.Timer leaves to the implementer, unlike
        // PeriodicTimer's WaitForNextTickAsync loop, which naturally can't overlap itself.
        private void LookupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            LookupLocation();
        }
        #endregion

        #region Helper Functions
        // Reads the configured ZIP code from ZipCodeInputPath and looks it up.
        private void LookupLocation()
        {
            try
            {
                if (!File.Exists(ZipCodeInputPath))
                {
                    logger.Warning("ZIP code input file not found at {Path}, skipping this lookup.", ZipCodeInputPath);
                    return;
                }

                string zipCode = File.ReadLines(ZipCodeInputPath).FirstOrDefault()?.Trim();
                if (string.IsNullOrEmpty(zipCode))
                {
                    logger.Warning("ZIP code input file at {Path} is empty, skipping this lookup.", ZipCodeInputPath);
                    return;
                }

                using (var db = new ExternalDataEntities())
                {
                    var results = db.ZipCodes.Where(z => z.ZipCode1 == zipCode).ToList();

                    if (results.Count == 0)
                    {
                        logger.Information("Location lookup: no results found for ZIP code {ZipCode}.", zipCode);
                        return;
                    }

                    foreach (var location in results)
                        logger.Information("Location lookup for {ZipCode}: {City}, {County} County, {State}", zipCode, location.City, location.County, location.State);
                }
            }
            catch (Exception ex)
            {
                var wrapped = new DatabankException("Location lookup failed.", ex);
#pragma warning disable S6667
                logger.Error(wrapped, "Location lookup failed.");
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
