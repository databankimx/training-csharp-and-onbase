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
using Samples.MvcWebApi.Core.Common;
#endregion

namespace Samples.MvcWebApi.Core.Controllers
{
    /// <summary>
    /// Confirms the API can receive and echo structured data. No try/catch here at all, an
    /// unhandled exception is caught by GlobalExceptionHandler (see Program.cs's
    /// app.UseExceptionHandler()) rather than a per-controller [ExceptionFilter] the way
    /// Samples.MvcWebApi's TestController needed. See LectureNotes.md.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        #region API Methods
        /// <summary>
        /// Gets a simple message confirming the API is running, with a UTC timestamp. This is a POST
        /// </summary>
        /// <param name="request">The test request containing the request ID and data.</param>
        /// <returns>A TestResponse containing the request ID and a message confirming the API is running.</returns>
        [HttpPost]
        public ActionResult<TestResponse> Post(TestRequest request)
        {
            var response = new TestResponse(request.RequestId, $"{DateTime.UtcNow:u} - The web API is running and received data: [{request.Data}].");
            return Ok(response);
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
