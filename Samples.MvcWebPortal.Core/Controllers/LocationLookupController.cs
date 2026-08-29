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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samples.MvcWebPortal.Core.Data;
#endregion

namespace Samples.MvcWebPortal.Core.Controllers
{
    /// <summary>
    /// Performs the data lookup and delivers the result set for a provided ZIP code, using a
    /// genuinely async EF Core query (contrast against Samples.MvcWebPortal's synchronous
    /// .ToList() call). See LectureNotes.md.
    /// </summary>
    public class LocationLookupController(LocationLookupContext db) : Controller
    {
        #region Methods
        /// <summary>
        /// Asynchronously retrieves zip code records that match the provided zip code and returns the view with the
        /// results.
        /// </summary>
        /// <param name="zipCode">Zip code value used to filter records.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// that renders the matching records.</returns>
        public async Task<IActionResult> Index(string zipCode)
        {
            var results = await db.ZipCodes.Where(x => x.ZipCode1 == zipCode).ToListAsync();
            ViewBag.ZipCode = zipCode;
            return View(results);
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
