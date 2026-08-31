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
     * *Fixed*: logging originally called logger.LogInformation()/LogWarning() directly,
     * which static analysis correctly flagged (CA1873, "Evaluation of this argument may
     * be expensive and unnecessary if logging is disabled"), the message template gets
     * parsed and every argument gets boxed on EVERY call, even when that log level is
     * disabled and the message will never actually be written anywhere. Replaced below
     * with LoggerMessage.Define, precompiled logging delegates (available since .NET Core
     * 2.0, no source generator or "partial" anything involved) that check IsEnabled()
     * FIRST and only format/box arguments if the message will actually be written. See
     * LectureNotes.md for why this ended up being the more reliable choice over the
     * newer [LoggerMessage] attribute/source-generator approach.
     */
    #endregion

    /// <summary>
    /// Periodically checks the <c>ZipCodes</c> table for rows with missing data (a null or
    /// empty <c>State</c>, <c>County</c>, <c>City</c>, or ZIP code) and logs what it finds,
    /// a genuine, if simple, example of the kind of recurring background maintenance task
    /// a Windows Service is actually used for in practice.
    /// </summary>
    /// <param name="scopeFactory">Used to create a fresh dependency injection scope (and therefore a fresh <see cref="LocationLookupContext"/>) on every check.</param>
    /// <param name="logger">The logger this worker writes its findings to, via the precompiled delegates below.</param>
    public class Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger) : BackgroundService
    {
        #region Fields
        // How often to run the check. A real service would likely read this from
        // configuration rather than hardcoding it; kept simple here for the sample.
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

        // Precompiled logging delegates (see Training Notes above). Each is built once,
        // statically, the FIRST time this type is used, not on every log call.
        private static readonly Action<ILogger, int, int, Exception?> LogIncompleteDataDelegate =
            Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(
                LogLevel.Warning,
                new EventId(1, nameof(LogIncompleteData)),
                "Data health check: {IncompleteCount} of {TotalCount} ZipCodes rows have missing data.");

        private static readonly Action<ILogger, int, Exception?> LogAllCompleteDelegate =
            Microsoft.Extensions.Logging.LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(2, nameof(LogAllComplete)),
                "Data health check: all {TotalCount} ZipCodes rows are complete.");

        private static readonly Action<ILogger, Exception?> LogCheckFailedDelegate =
            Microsoft.Extensions.Logging.LoggerMessage.Define(
                LogLevel.Error,
                new EventId(3, nameof(LogCheckFailed)),
                "Data health check failed.");
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Runs for the entire lifetime of the service, checking the <c>ZipCodes</c> table
        /// on a fixed interval until cancellation is requested (service stop/shutdown).
        /// </summary>
        /// <param name="stoppingToken">Signaled when the host is shutting down.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CheckInterval);

            do
            {
                await CheckDataHealthAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        #endregion

        #region Helper Functions
        // Runs a single data-health check, using a fresh DI scope (and therefore a fresh
        // LocationLookupContext) for this one check only, see Training Notes above.
        private async Task CheckDataHealthAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LocationLookupContext>();

                var incompleteCount = await db.ZipCodes
                    .Where(z => string.IsNullOrWhiteSpace(z.State)
                             || string.IsNullOrWhiteSpace(z.County)
                             || string.IsNullOrWhiteSpace(z.City)
                             || string.IsNullOrWhiteSpace(z.ZipCode1))
                    .CountAsync(cancellationToken);

                var totalCount = await db.ZipCodes.CountAsync(cancellationToken);

                if (incompleteCount > 0) LogIncompleteData(incompleteCount, totalCount);
                else LogAllComplete(totalCount);
            }
            catch (Exception ex)
            {
                LogCheckFailed(ex);
            }
        }

        // Thin wrappers around the precompiled delegates above, so the rest of the class
        // reads exactly like a normal method call (LogIncompleteData(a, b)) rather than
        // reaching for the delegate fields directly everywhere they're needed.
        private void LogIncompleteData(int incompleteCount, int totalCount) =>
            LogIncompleteDataDelegate(logger, incompleteCount, totalCount, null);

        private void LogAllComplete(int totalCount) =>
            LogAllCompleteDelegate(logger, totalCount, null);

        private void LogCheckFailed(Exception ex) =>
            LogCheckFailedDelegate(logger, ex);
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
