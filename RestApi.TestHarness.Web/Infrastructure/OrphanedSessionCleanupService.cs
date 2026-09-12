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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#endregion

namespace RestApi.TestHarness.Web.Infrastructure
{
    #region Training Notes
    /*
     * *Migration Note: replaces Unity.TestHarness.Web's own Global.asax.cs Session_End
     * handler (which called SessionConnectionManager.DisconnectOrphanedSession). Classic
     * ASP.NET's InProc session mode fires Session_End when a session expires; ASP.NET
     * Core's own session middleware has no equivalent event at all, expiration is purely
     * a property of the underlying distributed cache entry, with no notification hook to
     * react to.
     *
     * This BackgroundService is the replacement: an active periodic sweep (via
     * SessionManagementStore.SweepAsync), not a passive event handler, checking every
     * CheckInterval for entries idle longer than MaxIdle and disconnecting/disposing
     * them, the same "delayed safety net for a closed browser tab" role
     * DisconnectOrphanedSession played (a closed tab sends the server no signal at all,
     * HTTP is stateless; this can only ever be a delayed cleanup, not immediate).
     *
     * MaxIdle is set independently of ASP.NET Core's own session cookie/idle timeout
     * (Program.cs's AddSession call): it doesn't need to match exactly, only be in the
     * same neighborhood, since this sweep's only job is releasing an abandoned OnBase
     * license reasonably promptly, not tracking the session's own expiration precisely.
     */
    #endregion

    /// <summary>
    /// Periodically sweeps <see cref="SessionManagementStore"/> for sessions that have
    /// been idle longer than <see cref="MaxIdle"/>, disconnecting and disposing them, so
    /// an abandoned browser tab doesn't leave an OnBase license held indefinitely.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the OrphanedSessionCleanupService class
    /// </remarks>
    /// <param name="store">The session store to sweep.</param>
    /// <param name="logger">Logger for sweep activity/errors.</param>
    public class OrphanedSessionCleanupService(SessionManagementStore store, ILogger<OrphanedSessionCleanupService> logger) : BackgroundService
    {
        #region Constants
        // How often to run a sweep
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

        // How long a session may go unused before it's considered orphaned
        private static readonly TimeSpan MaxIdle = TimeSpan.FromMinutes(30);
        #endregion

        #region Protected Methods
        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CheckInterval);

            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    await store.SweepAsync(MaxIdle).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Swallowed (beyond logging): one failed sweep shouldn't stop the
                    // background service, the next scheduled tick tries again.
                    logger.LogError(ex, "Error sweeping orphaned OnBase sessions");
                }
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
