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
#endregion

namespace Samples.MvcWebApi.Core.Controllers
{
    /// <summary>
    /// Confirms the API is online. Unlike Samples.MvcWebApi's PingController (which supports
    /// both Get() and Post() for the same operation, a WebApi 2 convention-based routing
    /// pattern), this uses one explicit, attribute-routed GET endpoint, the idiomatic ASP.NET
    /// Core style. See LectureNotes.md.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PingController : ControllerBase
    {
        #region API Methods
        /// <summary>
        /// Gets a simple message confirming the API is running, with a UTC timestamp. This is a GET
        /// </summary>
        /// <returns>A message confirming the API is running.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Get() => Ok($"{DateTime.UtcNow:u} - The web API is running.");
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
