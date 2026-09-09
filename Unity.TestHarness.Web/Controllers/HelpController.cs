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
using System.Reflection;
using System.Web.Mvc;
#endregion

namespace Unity.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of Unity.TestHarness (the WPF version)'s Help page.
     * Almost entirely static content, living directly in the View, matching the WPF
     * version's own "HelpViewModel is deliberately minimal" design. ShowLicense reads
     * Resources\LICENSE from disk and returns it as plain text for the View's own
     * JS-driven popup (an actual popup, not just a link to the static file directly,
     * matching what was specifically asked for when this was built in the WPF version).
     */
    #endregion

    /// <summary>
    /// The Help page: usage instructions, an About section, and license/support terms.
    /// </summary>
    public class HelpController : Controller
    {
        #region Public Methods
        /// <summary>
        /// Displays the Help page.
        /// </summary>
        /// <returns>The Help page.</returns>
        public ActionResult Index()
        {
            ViewBag.AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
            return View();
        }

        /// <summary>
        /// AJAX: the license's real text, read from Resources\LICENSE, for the popup.
        /// </summary>
        /// <returns>The license text.</returns>
        [HttpGet]
        public ActionResult ShowLicense()
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "LICENSE");
                var text = System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : "LICENSE file not found.";
                return Content(text, "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"Error reading LICENSE file: {ex.Message}", "text/plain");
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
