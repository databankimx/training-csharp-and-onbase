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
using System.Web.Mvc;
using Unity.TestHarness.Web.Infrastructure;
#endregion

namespace Unity.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: not a page controller (the default route targets Connect
     * directly, see RouteConfig), this exists only for cross-cutting actions every
     * page's shared _Layout.cshtml needs: clearing the output log (mirroring
     * LogViewModel.ClearCommand in Unity.TestHarness, the WPF version) and closing the
     * session (see Close's own Training Note below).
     */
    #endregion

    /// <summary>
    /// Holds cross-cutting actions used by the shared layout, not a page controller in
    /// its own right.
    /// </summary>
    public class HomeController : Controller
    {
        #region Public Methods
        /// <summary>
        /// Clears the current session's output log, then returns to the referring page.
        /// </summary>
        /// <returns>A redirect back to the referring page.</returns>
        [HttpPost]
        public ActionResult ClearLog()
        {
            SessionLog.Clear();
            return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index", "Connect"));
        }

        /// <summary>
        /// Disconnects the current session (mirroring MainWindow.xaml.cs's
        /// disconnect-on-close in Unity.TestHarness, the WPF version), then shows a page
        /// that attempts to close the browser tab. That attempt (window.close() in the
        /// view) is NOT guaranteed to work: browsers only allow a script to close a tab it
        /// opened itself, and refuse to for an ordinarily-navigated tab, a genuine browser
        /// security restriction with no client-side workaround. The disconnect itself
        /// always happens either way; the view's fallback text covers the case where the
        /// tab doesn't actually close.
        /// </summary>
        /// <returns>The Closed confirmation page.</returns>
        [HttpPost]
        public ActionResult Close()
        {
            SessionConnectionManager.Disconnect();
            return View("Closed");
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
