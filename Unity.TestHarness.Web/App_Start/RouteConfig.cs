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
using System.Web.Routing;
#endregion

namespace Unity.TestHarness.Web
{
    #region Training Notes
    /*
     * *Migration Note: default route targets Connect directly (not a HomeController that
     * redirects), mirroring MainViewModel's own "navigate to the first sidebar item on
     * startup" behavior in Unity.TestHarness (the WPF version) without an unnecessary
     * redirect hop.
     */
    #endregion

    /// <summary>
    /// Route configuration class for the ASP.NET MVC application.
    /// </summary>
    public static class RouteConfig
    {
        #region Methods
        /// <summary>
        /// Registers the routes for the application.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Connect", action = "Index", id = UrlParameter.Optional }
            );
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
