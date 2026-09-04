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
using System.Web.Http;
using Samples.MvcWebApi.Filters;
using Samples.MvcWebApi.HelperClasses;
#endregion

#pragma warning disable S125 // Sections of code commented out for demonstration purposes
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

    /*
     * You may notice that we are not implementing try/catch in our methods here.
     * This is because we are leveraging the built-in "Filter" functionality of MVC for error handling.
     * Review ./Filters/ExceptionFilter.cs
     */
    #endregion

    /// <summary>
    /// Web API ping methods
    /// </summary>
    [LogFilter]
    [ExceptionFilter]
    public class PingController : ApiController
    {
        #region API Methods
        /// <summary>
        /// Expose ping as RESTful URL
        /// </summary>
        /// <returns>Server Awake Message</returns>
        public string Get()
        {
            return Post();

            // Below is what we would implement in the absence of filters
            //try
            //{
            //    return Post();
            //}
            //catch (Exception ex)
            //{
            //    var errors = ex.HandleException();
            //    return errors[errors.Count - 1];
            //}
        }

        /// <summary>
        /// Expose ping as POST request
        /// </summary>
        /// <returns>Server Awake Message</returns>
        public string Post()
        {
            return $"{Stamp.DateAndTime()} - The web API is running.";

            // Below is what we would implement in the absence of filters
            //try
            //{
            //    return $"{Stamp.DateAndTime()} - The web API is running.";
            //}
            //catch (Exception ex)
            //{
            //    var errors = ex.HandleException();
            //    return errors[errors.Count - 1];
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
