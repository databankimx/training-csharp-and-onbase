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
using System.Linq;
using System.Web.Mvc;
using Samples.MvcWebPortal.Models;
#endregion

namespace Samples.MvcWebPortal.Controllers
{
    /// <summary>
    /// Performs the data lookup and delivers the result sets for a provided Zip Code
    /// </summary>
    public class LocationLookupController : Controller
    {
        #region Private Globals
        // Entity Framework Data Source
        private readonly ExternalDataEntities db = new();
        #endregion

        #region Public Methods
        /// <summary>
        /// Performs the data lookup and delivers the result sets for a provided Zip Code
        /// </summary>
        /// <param name="zipCode"></param>
        /// <returns>Lookup Zip Code</returns>
        public ActionResult Index(string zipCode)
        {
            var results = db.ZipCodes.Where(x => string.Equals(x.ZipCode1, zipCode)).ToList();
            return View(results);
        }
        #endregion

        #region Parent Class Overrides
        /// <summary>
        /// Dispose the Entity Framework data context when the controller itself is disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
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
