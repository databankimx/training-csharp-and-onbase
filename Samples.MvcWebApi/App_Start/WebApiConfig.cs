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
using System.Web.Http;
using Samples.MvcWebApi.HelperClasses;
#endregion

namespace Samples.MvcWebApi
{
    /// <summary>
    /// Web API configuration and services
    /// </summary>
    public static class WebApiConfig
    {
        #region Public Methods
        /// <summary>
        /// Registers Web API routing and formatting settings.  
        /// </summary>
        /// <remarks>Defines a default route template of "api/{controller}/{id}/{data}" with optional "id"
        /// and "data" route parameters.</remarks>
        /// <param name="config">Specifies the HTTP configuration used to map attribute routes, define the default API route template, and
        /// add the browser JSON formatter.</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Modify the route to accept our arguments in REST format
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}/{data}",
                defaults: new
                {
                    id = RouteParameter.Optional,
                    data = RouteParameter.Optional
                }
            );

            config.Formatters.Add(new BrowserJsonFormatter());
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
