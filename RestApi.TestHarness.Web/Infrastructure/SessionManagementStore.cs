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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RestApi._00.CommonFunctionality.Models.Configuration;
using RestApi._00.CommonFunctionality.Models.Enumerations;
using RestApi._01.ConnectingToOnBase.HelperClasses.OnBase;
using RestApi.TestHarness.Web.Models;
#endregion

namespace RestApi.TestHarness.Web.Infrastructure
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of RestApi.TestHarness (the WPF version)'s own
     * ConnectionViewModel.Session, but genuinely reshaped for a multi-user, per-request
     * web context: ONE SessionManagement instance per browser session, not one for the
     * whole process.
     *
     * Where the live object actually lives is the key design decision here.
     * ASP.NET Core's own ISession can only store serializable byte[] data (see
     * Microsoft's own ISession documentation), it CANNOT hold a SessionManagement
     * instance directly (which owns a live HttpClient/HttpClientHandler, not
     * serializable). This mirrors, but doesn't match, Unity.TestHarness.Web's own
     * SessionConnectionManager, which stores the live Hyland.Unity.Application object
     * directly IN classic ASP.NET's Session (only possible because InProc session mode
     * can hold arbitrary object references, a capability ASP.NET Core's own session
     * middleware doesn't have at all).
     *
     * The fix: a separate, server-memory-resident store (this class, registered as a
     * SINGLETON in Program.cs, not scoped), keyed by each user's own
     * HttpContext.Session.Id (a plain string ASP.NET Core's session middleware DOES
     * generate and persist via its own session cookie, even though the middleware itself
     * can't store our object). GetOrCreate(sessionId) returns that session's own
     * SessionManagement, building a fresh one (from the shared, appsettings.json-bound
     * RestApiWebSettings) the first time a given session id is seen.
     *
     * No built-in ASP.NET Core equivalent of classic ASP.NET's Session_End event exists
     * to trigger cleanup when a session expires (the underlying distributed cache simply
     * expires the entry, with no notification hook). SweepAsync(), called periodically by
     * OrphanedSessionCleanupService (a BackgroundService, see its own Training Notes),
     * is the replacement: entries whose LastAccessed exceeds the configured idle window
     * are disconnected and disposed, the same "delayed safety net for a closed browser
     * tab" role Unity.TestHarness.Web's own DisconnectOrphanedSession played, achieved
     * through a different mechanism (an active sweep, not a fired event).
     */
    #endregion

    /// <summary>
    /// Holds one <see cref="SessionManagement"/> instance per browser session, keyed by
    /// <see cref="Microsoft.AspNetCore.Http.ISession.Id"/>. Registered as a singleton;
    /// see this class's own Training Notes for why the SessionManagement instances
    /// themselves can't live directly in ASP.NET Core's own session state.
    /// </summary>
    public class SessionManagementStore
    {
        #region Private Members
        private readonly IOptions<RestApiWebSettings> settings;
        private readonly ConcurrentDictionary<string, SessionEntry> entries = new();
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the SessionManagementStore class
        /// </summary>
        /// <param name="settings">The shared, appsettings.json-bound connection infrastructure settings.</param>
        public SessionManagementStore(IOptions<RestApiWebSettings> settings)
        {
            this.settings = settings;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the given session's SessionManagement instance, creating a new one
        /// (from the shared <see cref="RestApiWebSettings"/>) the first time this session
        /// id is seen. Also refreshes that session's own idle tracking, so
        /// <see cref="SweepAsync"/> won't disconnect an actively-used session.
        /// </summary>
        /// <param name="sessionId">The current user's <c>HttpContext.Session.Id</c>.</param>
        /// <returns>That session's own SessionManagement instance.</returns>
        public SessionManagement GetOrCreate(string sessionId)
        {
            var entry = entries.AddOrUpdate(sessionId,
                _ => new SessionEntry(BuildSessionManagement()),
                (_, existing) =>
                {
                    existing.Touch();
                    return existing;
                });

            return entry.Session;
        }

        /// <summary>
        /// Returns the given session's SessionManagement instance, WITHOUT creating one if
        /// none exists yet. Used by read-only checks (e.g. the shared layout's own
        /// "Connected"/"Not Connected" indicator) that shouldn't accidentally create a
        /// new, never-connected entry just by being displayed.
        /// </summary>
        /// <param name="sessionId">The current user's <c>HttpContext.Session.Id</c>.</param>
        /// <returns>That session's own SessionManagement instance, or <see langword="null"/> if none exists yet.</returns>
        public SessionManagement TryGet(string sessionId)
        {
            return entries.TryGetValue(sessionId, out var entry) ? entry.Session : null;
        }

        /// <summary>
        /// Disconnects (if connected) and disposes the given session's SessionManagement,
        /// removing it from the store entirely. Used by an explicit Disconnect action.
        /// </summary>
        /// <param name="sessionId">The session id to remove.</param>
        public async Task RemoveAsync(string sessionId)
        {
            if (entries.TryRemove(sessionId, out var entry))
            {
                await DisposeEntryAsync(entry).ConfigureAwait(false);
            }
        }
        #endregion

        #region Internal Methods
        // Disconnect/dispose every entry whose LastAccessed exceeds maxIdle, called
        // periodically by OrphanedSessionCleanupService
        internal async Task SweepAsync(TimeSpan maxIdle)
        {
            var cutoff = DateTime.UtcNow - maxIdle;

            foreach (var kvp in entries)
            {
                if (kvp.Value.LastAccessed >= cutoff) continue;
                if (entries.TryRemove(kvp.Key, out var entry))
                {
                    await DisposeEntryAsync(entry).ConfigureAwait(false);
                }
            }
        }
        #endregion

        #region Private Methods
        // Disconnect (best-effort) and dispose a session entry
        private static async Task DisposeEntryAsync(SessionEntry entry)
        {
            try
            {
                if (entry.Session.IsConnected) await entry.Session.DisconnectAsync().ConfigureAwait(false);
            }
            catch
            {
                // Swallowed deliberately: best-effort cleanup, either on explicit
                // Disconnect (where a failure here shouldn't block removing the entry)
                // or an orphaned sweep (where there's no request/user to report to).
            }
            finally
            {
                entry.Session.Dispose();
            }
        }

        // Build a new SessionManagement from the shared infrastructure settings.
        // Username/Password are left unset here (this app's own ConnectController sets
        // them directly on ServiceLocation before calling ConnectAsync(), see this
        // class's own Training Notes for why per-user credentials live there, not here).
        private SessionManagement BuildSessionManagement()
        {
            var config = settings.Value;

            var serviceLocation = new ServiceLocation
            {
                ApiServerUrl = config.ApiServerUrl,
                FormsApiUrl = config.FormsApiUrl,
                AuthenticationMode = AuthenticationMode.OnBaseCredentials,
                KeepAlive = config.KeepAlive
            };

            var idpSettings = new IdpSettings
            {
                IdpUrl = config.IdpUrl,
                IdpTenant = config.IdpTenant,
                IdpClientId = config.IdpClientId,
                IdpClientSecret = config.IdpClientSecret,
                IdpScope = config.IdpScope,
                IdpGrantType = config.IdpGrantType
            };

            return new SessionManagement(serviceLocation, idpSettings);
        }
        #endregion

        #region Nested Types
        // One store entry: the session's own SessionManagement, plus idle tracking for SweepAsync
        private class SessionEntry(SessionManagement session)
        {
            public SessionManagement Session { get; } = session;
            public DateTime LastAccessed { get; private set; } = DateTime.UtcNow;
            public void Touch() => LastAccessed = DateTime.UtcNow;
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
