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
using System.Web.Http;
using Samples.MvcWebApi.Common;
using Samples.MvcWebApi.Filters;
#endregion

#pragma warning disable S125 // Sections of code are commented out for demonstration purposes
namespace Samples.MvcWebApi.Controllers
{
    #region Training Notes
    /*
     * The next evolution of web services methodology is the Web API
     * Using the MVC framework, Web APIs are natively RESTful but can still be configured to accept a POST request
     *
     * There are a few advantages of using Web API
     *
     * 1. With native access to the Entity Framework, querying a mapped database is as easy as a single LINQ statement
     *    as opposed to writing your own code to access the database
     *
     * 2. Using route templates, it's easy to implement RESTful patters for passing arguments
     *
     * 3. Each controller can (by default) be created with Get() and Post() methods in order to support multiple modes of consumption
     *
     * One significant drawback, however, is that unlike WCF, Web API does not expose a WSDL or data contracts,
     *   so it is often necessary to define classes in the client application as well as the web API itself.
     */
    #endregion

    /// <summary>
    /// Web API location lookup methods
    /// </summary>
    [LogFilter]
    [ExceptionFilter]
    public class LocationLookupController : ApiController
    {
        #region API Methods
        /// <summary>
        /// Expose location lookup as RESTful URL with arguments appended
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="data">Zip Code to Search</param>
        /// <returns>Location information matching zip code</returns>
        public LocationResponse Get(string id, string data)
        {
            var request = new LocationRequest
            {
                Id = id,
                ZipCode = data
            };
            return Post(request);

            // Below is what we would implement in the absence of filters
            //try
            //{
            //    var request = new LocationRequest
            //    {
            //        Id = id,
            //        ZipCode = data
            //    };
            //    return Post(request);
            //}
            //catch (Exception ex)
            //{
            //    return new LocationResponse
            //    {
            //        Id = id,
            //        Data = null,
            //        Errors = ex.HandleException()
            //    };
            //}
        }

        /// <summary>
        /// Expose location lookup as POST request accepting request object
        /// </summary>
        /// <param name="request">Lookup request containing zip code</param>
        /// <returns>Location information matching zip code</returns>
        public LocationResponse Post(LocationRequest request)
        {
            var response = new LocationResponse { Id = request.Id };
            // Instead of creating a manual DB connection, we access our database mapped using the Entity Framework
            var db = new LocationLookupDatabase();
            // Here, a single LINQ query executed against teh entity-mapped database returns our lookup results
            var results = db.ZipCodes.Where(x => string.Equals(x.ZipCode1, request.ZipCode)).ToList();
            foreach (var location in results) response.Data.Add(new Location
            {
                Id = location.Id,
                State = location.State,
                County = location.County,
                City = location.City,
                ZipCode = location.ZipCode1
            });
            return response;

            // Below is what we would implement in the absence of filters
            //try
            //{
            //    var response = new LocationResponse { Id = request.Id };
            //    // Instead of creating a manual DB connection, we access our database mapped using the Entity Framework
            //    var db = new LocationLookupDatabase();
            //    // Here, a single LINQ query executed against teh entity-mapped database returns our lookup results
            //    var results = db.ZipCodes.Where(x => string.Equals(x.ZipCode1, request.ZipCode)).ToList();
            //    foreach (var location in results) response.Data.Add(new Location
            //    {
            //        Id = location.Id,
            //        State = location.State,
            //        County = location.County,
            //        City = location.City,
            //        ZipCode = location.ZipCode1
            //    });
            //    return response;
            //}
            //catch (Exception ex)
            //{
            //    return new LocationResponse
            //    {
            //        Id = request.Id,
            //        Data = null,
            //        Errors = ex.HandleException()
            //    };
            //}
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
