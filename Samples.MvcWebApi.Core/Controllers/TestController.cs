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

using Microsoft.AspNetCore.Mvc;
using Samples.MvcWebApi.Core.Models;

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
        [HttpPost]
        public ActionResult<TestResponse> Post(TestRequest request)
        {
            var response = new TestResponse(request.RequestId, $"{DateTime.UtcNow:u} - The web API is running and received data: [{request.Data}].");
            return Ok(response);
        }
    }
}
