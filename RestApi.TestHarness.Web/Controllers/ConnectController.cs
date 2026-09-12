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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestApi._00.CommonFunctionality.Models.Enumerations;
using RestApi._01.ConnectingToOnBase.HelperClasses.OnBase;
using RestApi.TestHarness.Web.Infrastructure;
using RestApi.TestHarness.Web.Models;
#endregion

namespace RestApi.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * ConnectController, genuinely reshaped for the reasons documented in
     * SessionManagementStore's/ConnectPageModel's own Training Notes: DI-injected
     * services (SessionManagementStore/SessionLogService) instead of static classes
     * reading HttpContext.Current, and a Connect action that collects a username/password
     * directly (per the confirmed requirement that every user authenticates with their
     * own credentials), not one that just uses whatever's already configured.
     *
     * No Reconnect/TestServer actions at all: both confirmed gaps (no SessionId-reconnect
     * concept, no documented health-check endpoint), see ConnectPageModel's own Training
     * Notes.
     *
     * Same plain form-post-then-redirect pattern as Unity.TestHarness.Web's own version,
     * for the same reason: every action here changes session-wide state the shared
     * _Layout.cshtml itself displays (the sidebar's connection dot, the Output panel), so
     * a full page reload keeps everything in sync without partial-update JS.
     *
     * *Correction*: BuildModel() originally only ever read from the session's own
     * SessionManagement (via store.TryGet), which is null until the user's FIRST Connect
     * submission (GetOrCreate is only called from the Connect action itself). That meant
     * a first-time visitor saw blank "(not set)" fields even though appsettings.json's
     * own RestApi section had real values configured, store.TryGet's whole point is to
     * avoid fabricating a session-specific entry just for a read-only display (see its
     * own Training Notes), but that meant this controller had nothing to show before a
     * session existed. Fixed by also injecting IOptions&lt;RestApiWebSettings&gt; (the
     * shared, appsettings.json-bound settings SessionManagementStore itself uses to build
     * a NEW SessionManagement) and falling back to it in BuildModel() when no
     * session-specific one exists yet, so the page shows the app's actual configured
     * defaults from the very first visit, not just after connecting once.
     */
    #endregion

    /// <summary>
    /// The Connect page: connect (with per-user credentials) / disconnect, and Keep Alive.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the ConnectController class
    /// </remarks>
    /// <param name="store">The per-session connection store.</param>
    /// <param name="log">The shared, per-session output log.</param>
    /// <param name="sharedSettings">The shared, appsettings.json-bound connection infrastructure settings, used as a fallback when no session-specific SessionManagement exists yet.</param>
    public class ConnectController(SessionManagementStore store, SessionLogService log, IOptionsMonitor<RestApiWebSettings> sharedSettings) : Controller
    {
        #region Public Methods
        /// <summary>
        /// Displays the Connect page.
        /// </summary>
        /// <returns>The Connect page.</returns>
        public IActionResult Index()
        {
            var session = store.TryGet(HttpContext.Session.Id);
            return View(BuildModel(session, sharedSettings.CurrentValue));
        }

        /// <summary>
        /// Connects using the submitted username/password, against whatever server/IdP
        /// infrastructure is currently configured.
        /// </summary>
        /// <param name="username">The OnBase username to connect with.</param>
        /// <param name="password">The OnBase password to connect with.</param>
        /// <returns>A redirect back to the Connect page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Connect(string username, string password)
        {
            try
            {
                var session = store.GetOrCreate(HttpContext.Session.Id);
                session.ServiceLocation.Username = username;
                session.ServiceLocation.Password = password;

                await session.ConnectAsync();
                log.Success("Connected.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Disconnects the current session.
        /// </summary>
        /// <returns>A redirect back to the Connect page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disconnect()
        {
            await store.RemoveAsync(HttpContext.Session.Id);
            log.Success("Disconnected.");
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Sets Keep Alive on the current session's connection settings.
        /// </summary>
        /// <param name="keepAlive">The new Keep Alive value.</param>
        /// <returns>A redirect back to the Connect page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetKeepAlive(bool keepAlive)
        {
            var session = store.TryGet(HttpContext.Session.Id);
            if (session?.ServiceLocation != null) session.ServiceLocation.KeepAlive = keepAlive;
            return RedirectToAction("Index");
        }
        #endregion

        #region Private Methods
        // Build the page model from the given session's connection state, falling back to
        // the shared, appsettings.json-bound defaults if no session-specific
        // SessionManagement exists yet (see this class's own Training Notes)
        private static ConnectPageModel BuildModel(SessionManagement session, RestApiWebSettings sharedSettings)
        {
            return new ConnectPageModel
            {
                IsConnected = session?.IsConnected ?? false,
                ApiServerUrl = session?.ServiceLocation?.ApiServerUrl ?? sharedSettings.ApiServerUrl,
                FormsApiUrl = session?.ServiceLocation?.FormsApiUrl ?? sharedSettings.FormsApiUrl,
                AuthenticationMode = (session?.ServiceLocation?.AuthenticationMode ?? AuthenticationMode.OnBaseCredentials).ToString(),
                KeepAlive = session?.ServiceLocation?.KeepAlive ?? sharedSettings.KeepAlive,
                Username = session?.ServiceLocation?.Username
            };
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
