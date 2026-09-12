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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
#endregion

namespace RestApi.TestHarness.Web.Controllers
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * HelpController, same "almost entirely static content" design. ShowLicense reads
     * from wwwroot/Resources/LICENSE now, not AppDomain.CurrentDomain.BaseDirectory: this
     * project's own static assets live under wwwroot (ASP.NET Core's own static-file
     * convention, see RestApi.TestHarness.Web's own .csproj Training Notes), not the
     * output directory root the way classic ASP.NET's own BaseDirectory pointed to.
     */
    #endregion

    /// <summary>
    /// The Help page: usage instructions, an About section, and license/support terms.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the HelpController class
    /// </remarks>
    /// <param name="environment">Used to locate wwwroot/Resources/LICENSE on disk.</param>
    public class HelpController(IWebHostEnvironment environment) : Controller
    {
        #region Public Methods
        /// <summary>
        /// Displays the Help page.
        /// </summary>
        /// <returns>The Help page.</returns>
        public IActionResult Index()
        {
            ViewBag.AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
            return View();
        }

        /// <summary>
        /// AJAX: the license's real text, read from wwwroot/Resources/LICENSE, for the popup.
        /// </summary>
        /// <returns>The license text.</returns>
        [HttpGet]
        public IActionResult ShowLicense()
        {
            try
            {
                var path = Path.Combine(environment.WebRootPath, "Resources", "LICENSE");
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
