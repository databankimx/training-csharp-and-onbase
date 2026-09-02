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
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Samples.WindowsService.NetCore.Data;
#endregion

namespace Samples.WindowsService.NetCore
{
    #region Training Notes
    /*
     * BackgroundService is the standard base class for long-running work hosted by the
     * Generic Host (the same host abstraction ASP.NET Core's WebApplicationBuilder is
     * built on, see Samples.MvcWebApi.Core). It's registered as a SINGLETON by
     * AddHostedService<Worker>() in Program.cs, and its ExecuteAsync method runs for the
     * entire lifetime of the application.
     *
     * That singleton lifetime is exactly why this class injects IServiceScopeFactory
     * rather than LocationLookupContext directly: EF Core DbContexts are registered
     * SCOPED (a new instance per unit of work), and a singleton can't safely hold a
     * scoped dependency, it would either fail DI validation outright, or, worse, silently
     * capture ONE DbContext instance for the service's entire lifetime, a genuine bug
     * (stale cached data, thread-safety issues, a connection held open indefinitely).
     * CreateScope() below creates a fresh scope, and therefore a fresh DbContext, on
     * every single timer tick, exactly the same lifetime a real request would get in
     * Samples.MvcWebApi.Core.
     *
     * Logging uses LoggerMessage.Define, precompiled logging delegates (available since
     * .NET Core 2.0, no source generator or "partial" anything involved) that check
     * IsEnabled() FIRST and only format/box arguments if the message will actually be
     * written, avoiding CA1873. See LectureNotes.md for why this ended up being the more
     * reliable choice over the newer [LoggerMessage] attribute/source-generator approach.
     *
     * *Migration Note: this service performs the SAME core task as every other sample in
     * this training set, look up city/county/state by ZIP code, deliberately, so the only
     * thing that varies across samples is the coding pattern, not the underlying logic. A
     * Windows Service has no interactive caller to supply a ZIP code on demand, so the ZIP
     * code to look up is instead read from a plain text file at zipCodeInputPath (see
     * Fields below), whose location is itself read from appsettings.json's
     * "ZipCodeFilePath" setting rather than hardcoded, matching the same fix applied to
     * Samples.WindowsService's own App.config, on every timer tick, a genuine, realistic
     * pattern for unattended services (an HKEY_LOCAL_MACHINE registry value is another
     * common choice for this same purpose, a file was used here specifically because it
     * needs no elevated permissions and behaves identically on both net48 and net10.0).
     * See Samples.WindowsService's own LocationLookupService.cs for the classic
     * ServiceBase equivalent of this exact task.
     */
    #endregion

    /// <summary>
    /// Periodically reads a ZIP code from a text file and looks up its city/county/state,
    /// the same task as <see cref="Samples.WindowsService.LocationLookupService"/>,
    /// expressed in the Generic Host + BackgroundService model instead.
    /// </summary>
    /// <param name="scopeFactory">Used to create a fresh dependency injection scope (and therefore a fresh <see cref="LocationLookupContext"/>) on every lookup.</param>
    /// <param name="logger">The logger this worker writes its findings to, via the precompiled delegates below.</param>
    /// <param name="configuration">Used to read the "ZipCodeFilePath" setting from appsettings.json.</param>
    public class Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
    {
        #region Fields
        // How often to check the input file and perform a lookup. A real service would
        // likely read this from configuration rather than hardcoding it; kept simple here
        // for the sample.
        private static readonly TimeSpan LookupInterval = TimeSpan.FromMinutes(5);

        // Where the ZIP code to look up is read from, one plain line of text, re-read on
        // every timer tick so an operator can change it without restarting the service.
        // Read from appsettings.json rather than hardcoded, matching Samples.WindowsService's
        // own App.config-based fix.
        private readonly string zipCodeInputPath = configuration["ZipCodeFilePath"]
            ?? throw new InvalidOperationException("ZipCodeFilePath is not configured in appsettings.json!");

        // Precompiled logging delegates (see Training Notes above). Each is built once,
        // statically, the FIRST time this type is used, not on every log call.
        private static readonly Action<ILogger, string, Exception?> LogInputFileNotFoundDelegate =
            Microsoft.Extensions.Logging.LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(1, nameof(LogInputFileNotFound)),
                "ZIP code input file not found at {Path}, skipping this lookup.");

        private static readonly Action<ILogger, string, Exception?> LogInputFileEmptyDelegate =
            Microsoft.Extensions.Logging.LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(2, nameof(LogInputFileEmpty)),
                "ZIP code input file at {Path} is empty, skipping this lookup.");

        private static readonly Action<ILogger, string, Exception?> LogNoResultsDelegate =
            Microsoft.Extensions.Logging.LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(3, nameof(LogNoResults)),
                "Location lookup: no results found for ZIP code {ZipCode}.");

        private static readonly Action<ILogger, string, string, string, string, Exception?> LogLocationFoundDelegate =
            Microsoft.Extensions.Logging.LoggerMessage.Define<string, string, string, string>(
                LogLevel.Information,
                new EventId(4, nameof(LogLocationFound)),
                "Location lookup for {ZipCode}: {City}, {County} County, {State}");

        private static readonly Action<ILogger, Exception?> LogLookupFailedDelegate =
            Microsoft.Extensions.Logging.LoggerMessage.Define(
                LogLevel.Error,
                new EventId(5, nameof(LogLookupFailed)),
                "Location lookup failed.");
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Runs for the entire lifetime of the service, performing a location lookup on a
        /// fixed interval until cancellation is requested (service stop/shutdown).
        /// </summary>
        /// <param name="stoppingToken">Signaled when the host is shutting down.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(LookupInterval);

            do
            {
                await LookupLocationAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        #endregion

        #region Helper Functions
        // Reads the configured ZIP code from ZipCodeInputPath and looks it up, using a
        // fresh DI scope (and therefore a fresh LocationLookupContext) for this one
        // lookup only, see Training Notes above.
        private async Task LookupLocationAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!File.Exists(zipCodeInputPath))
                {
                    LogInputFileNotFound(zipCodeInputPath);
                    return;
                }

                var lines = await File.ReadAllLinesAsync(zipCodeInputPath, cancellationToken);
                var zipCode = lines.FirstOrDefault()?.Trim();
                if (string.IsNullOrEmpty(zipCode))
                {
                    LogInputFileEmpty(zipCodeInputPath);
                    return;
                }

                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LocationLookupContext>();

                var results = await db.ZipCodes.Where(z => z.ZipCode1 == zipCode).ToListAsync(cancellationToken);

                if (results.Count == 0)
                {
                    LogNoResults(zipCode);
                    return;
                }

                foreach (var location in results)
                    LogLocationFound(zipCode, location.City ?? string.Empty, location.County ?? string.Empty, location.State ?? string.Empty);
            }
            catch (Exception ex)
            {
                LogLookupFailed(ex);
            }
        }

        // Thin wrappers around the precompiled delegates above, so the rest of the class
        // reads exactly like a normal method call rather than reaching for the delegate
        // fields directly everywhere they're needed.
        private void LogInputFileNotFound(string path) =>
            LogInputFileNotFoundDelegate(logger, path, null);

        private void LogInputFileEmpty(string path) =>
            LogInputFileEmptyDelegate(logger, path, null);

        private void LogNoResults(string zipCode) =>
            LogNoResultsDelegate(logger, zipCode, null);

        private void LogLocationFound(string zipCode, string city, string county, string state) =>
            LogLocationFoundDelegate(logger, zipCode, city, county, state, null);

        private void LogLookupFailed(Exception ex) =>
            LogLookupFailedDelegate(logger, ex);
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
