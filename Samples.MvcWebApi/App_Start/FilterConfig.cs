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
#endregion

namespace Samples.MvcWebApi
{
    /// <summary>
    /// FilterConfig class is used to register global filters for the ASP.NET MVC application.
    /// </summary>
    public static class FilterConfig
    {
        #region Public Methods
        /// <summary>
        /// Registers the application's global MVC filters.
        /// </summary>
        /// <remarks>Adds a HandleErrorAttribute to the global filter collection.</remarks>
        /// <param name="filters">Global filter collection to populate.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
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
