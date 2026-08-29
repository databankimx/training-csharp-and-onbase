#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
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
using Samples.MvcWebApi.Filters;
#endregion

namespace Samples.MvcWebApi.Controllers
{
    /// <summary>
    /// Home controller class for the MVC application.
    /// </summary>
    public class HomeController : Controller
    {
        #region Public Methods
        /// <summary>
        /// Displays the Home page.
        /// </summary>
        /// <remarks>Execution is wrapped by logging and exception filters.</remarks>
        /// <returns>An <see cref="ActionResult"/> that renders the default view for the action.</returns>
        [LogFilter]
        [ExceptionFilter]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
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
