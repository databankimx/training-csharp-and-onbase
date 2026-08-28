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

#region Directives
using System.Web.Http;
using Samples.MvcWebApi.Common;
using Samples.MvcWebApi.Filters;
using Samples.MvcWebApi.HelperClasses;
#endregion

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
    /// Web API test methods
    /// </summary>
    [LogFilter]
    [ExceptionFilter]
    public class TestController : ApiController
    {
        #region API Methods
        /// <summary>
        /// Expose API test as RESTful URL with arguments appended
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="data">Test Text</param>
        /// <returns>Test Success Message</returns>
        public TestResponse Get(string id, string data)
        {
            var request = new TestRequest
            {
                Id = id,
                Data = data
            };
            return Post(request);

            // Below is what we would implement in the absence of filters
            //try
            //{
            //    var request = new TestRequest
            //    {
            //        Id = id,
            //        Data = data
            //    };
            //    return Post(request);
            //}
            //catch (Exception ex)
            //{
            //    return new TestResponse
            //    {
            //        Id = data,
            //        Data = null,
            //        Errors = ex.HandleException()
            //    };
            //}
        }

        /// <summary>
        /// Expose API test as POST request accepting request object
        /// </summary>
        /// <param name="request">API Test Request</param>
        /// <returns>Test Success Message</returns>
        public TestResponse Post(TestRequest request)
        {
            return new TestResponse
            {
                Id = request.Id,
                Data = $"{Stamp.DateAndTime()} - The web service is running and received data: [{request.Data}].",
                Errors = null
            };

            // Below is what we would implement in the absence of filters
            //try
            //{
            //    return new TestResponse
            //    {
            //        Id = request.Id,
            //        Data = $"{Stamp.DateAndTime()} - The web service is running and received data: [{request.Data}].",
            //        Errors = null
            //    };
            //}
            //catch (Exception ex)
            //{
            //    return new TestResponse
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
