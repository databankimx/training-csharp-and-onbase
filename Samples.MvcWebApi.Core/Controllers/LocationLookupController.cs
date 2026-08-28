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
using Microsoft.EntityFrameworkCore;
using Samples.MvcWebApi.Core.Data;
using Samples.MvcWebApi.Core.Models;

namespace Samples.MvcWebApi.Core.Controllers
{
    /// <summary>
    /// Looks up city/county/state by ZIP code, backed by EF Core against the same ZipCodes
    /// table Samples.MvcWebApi's EF6 version queries. Note the async/await + LINQ query,
    /// EF Core's asynchronous query methods (ToListAsync, etc.) genuinely don't block a thread
    /// waiting on the database round-trip the way EF6's synchronous LINQ-to-Entities calls do.
    /// See LectureNotes.md.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LocationLookupController(LocationLookupContext db) : ControllerBase
    {
        [HttpGet("{zipCode}")]
        public async Task<ActionResult<LocationLookupResponse>> Get(string zipCode)
        {
            var requestId = Guid.NewGuid().ToString();

            var locations = await db.ZipCodes
                .Where(z => z.ZipCode1 == zipCode)
                .Select(z => new Location(z.State, z.County, z.City, z.ZipCode1))
                .ToListAsync();

            return Ok(new LocationLookupResponse(requestId, locations));
        }
    }
}
