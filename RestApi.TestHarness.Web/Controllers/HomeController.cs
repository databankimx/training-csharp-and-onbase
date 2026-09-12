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
using Microsoft.AspNetCore.Mvc;
using RestApi.TestHarness.Web.Infrastructure;
#endregion

namespace RestApi.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * HomeController, same "not a page controller, just cross-cutting actions the shared
     * layout needs" role. Request.UrlReferrer (a System.Web-only API) is replaced with
     * reading the Referer request header directly (the standard, if historically
     * misspelled, HTTP header name), ASP.NET Core has no equivalent convenience property.
     */
    #endregion

    /// <summary>
    /// Holds cross-cutting actions used by the shared layout, not a page controller in
    /// its own right.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the HomeController class
    /// </remarks>
    /// <param name="store">The per-session connection store.</param>
    /// <param name="log">The shared, per-session output log.</param>
    public class HomeController(SessionManagementStore store, SessionLogService log) : Controller
    {
        #region Public Methods
        /// <summary>
        /// Clears the current session's output log, then returns to the referring page.
        /// </summary>
        /// <returns>A redirect back to the referring page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearLog()
        {
            log.Clear();
            var referer = Request.Headers.Referer.ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? Url.Action("Index", "Connect") : referer);
        }

        /// <summary>
        /// Disconnects the current session, then shows a page that attempts to close the
        /// browser tab. That attempt (window.close() in the view) is NOT guaranteed to
        /// work: browsers only allow a script to close a tab it opened itself, and refuse
        /// to for an ordinarily-navigated tab, a genuine browser security restriction with
        /// no client-side workaround. The disconnect itself always happens either way; the
        /// view's fallback text covers the case where the tab doesn't actually close.
        /// </summary>
        /// <returns>The Closed confirmation page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close()
        {
            await store.RemoveAsync(HttpContext.Session.Id);
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
