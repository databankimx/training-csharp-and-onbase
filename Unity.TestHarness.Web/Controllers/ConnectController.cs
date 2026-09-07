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
using System.Threading.Tasks;
using System.Web.Mvc;
using Unity.TestHarness.Web.Infrastructure;
using Unity.TestHarness.Web.Models;
#endregion

namespace Unity.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of Unity.TestHarness (the WPF version)'s Connect
     * page (ConnectionViewModel + ConnectView.xaml). Every action here is a plain
     * form-post-then-redirect (not AJAX): Connect/Disconnect/Reconnect/TestServer all
     * change session-wide state the shared _Layout.cshtml itself displays (the sidebar's
     * connection dot, the Output panel), so a full page reload after each action is the
     * simplest way to keep everything in sync, rather than partially updating pieces of
     * the page via JS. AJAX is used elsewhere in this app (Taxonomy's cascading
     * dropdowns, Archiving's dynamic keyword rows) where a full reload would genuinely
     * hurt the feel; a page of buttons that change server-side connection state doesn't
     * need it.
     *
     * Index() re-checks the Application Server's availability on every visit, matching
     * ConnectionViewModel's own "runs automatically every time you visit this page"
     * behavior in Unity.TestHarness (the WPF version), not just the first.
     */
    #endregion

    /// <summary>
    /// The Connect page: connect/disconnect, reconnect by Session ID, Keep Alive, and
    /// Application Server availability checking.
    /// </summary>
    public class ConnectController : Controller
    {
        #region Public Methods
        /// <summary>
        /// Displays the Connect page, re-checking the Application Server's availability
        /// on every visit.
        /// </summary>
        /// <returns>The Connect page.</returns>
        public async Task<ActionResult> Index()
        {
            if (!string.IsNullOrEmpty(SessionConnectionManager.GetServicePath())) await SessionConnectionManager.TestServer();
            return View(BuildModel());
        }

        /// <summary>
        /// Connects using whatever's currently configured in Settings.
        /// </summary>
        /// <returns>A redirect back to the Connect page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Connect()
        {
            SessionConnectionManager.Connect();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Disconnects the current session.
        /// </summary>
        /// <returns>A redirect back to the Connect page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disconnect()
        {
            SessionConnectionManager.Disconnect();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Reconnects to an ad hoc Session ID, independent of whatever Settings currently holds.
        /// </summary>
        /// <param name="reconnectSessionIdInput">The Session ID to reconnect to.</param>
        /// <returns>A redirect back to the Connect page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReconnectBySessionId(string reconnectSessionIdInput)
        {
            if (!string.IsNullOrEmpty(reconnectSessionIdInput)) SessionConnectionManager.ReconnectBySessionId(reconnectSessionIdInput);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Sets Keep Alive on the currently-configured connection settings.
        /// </summary>
        /// <param name="keepAlive">The new Keep Alive value.</param>
        /// <returns>A redirect back to the Connect page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetKeepAlive(bool keepAlive)
        {
            SessionConnectionManager.SetKeepAlive(keepAlive);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Checks the Application Server's availability directly, independent of the
        /// Unity API.
        /// </summary>
        /// <returns>A redirect back to the Connect page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TestServer()
        {
            await SessionConnectionManager.TestServer();
            return RedirectToAction("Index");
        }
        #endregion

        #region Private Methods
        // Build the page model from the current session's connection state
        private static ConnectPageModel BuildModel()
        {
            return new ConnectPageModel
            {
                IsConnected = SessionConnectionManager.IsConnected(),
                SessionId = SessionConnectionManager.GetSessionId(),
                CurrentUserDisplayName = SessionConnectionManager.GetCurrentUserDisplayName(),
                ServicePath = SessionConnectionManager.GetServicePath(),
                DataSource = SessionConnectionManager.GetDataSource(),
                AuthenticationMode = SessionConnectionManager.GetAuthenticationMode().ToString(),
                KeepAlive = SessionConnectionManager.GetKeepAlive(),
                IsServerAvailable = SessionConnectionManager.GetIsServerAvailable(),
                ServerStatusMessage = SessionConnectionManager.GetServerStatusMessage() ?? "Not checked yet."
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
