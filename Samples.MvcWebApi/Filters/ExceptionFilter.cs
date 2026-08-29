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
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Samples.MvcWebApi.Common;
using Samples.MvcWebApi.HelperClasses;
using Serilog;
#endregion

namespace Samples.MvcWebApi.Filters
{
    /// <summary>
    /// Logs the Unhandled Exception of any Action Method
    /// </summary>
    #pragma warning disable S3376
    #pragma warning disable S3993
    internal class ExceptionFilter : ExceptionFilterAttribute
    #pragma warning restore S3993
    #pragma warning restore S3376
    {
        #region Properties
        /// <summary>
        /// Serilog logging utility
        /// </summary>
        public static ILogger Logger { get; set; } = Log.Logger;
        #endregion

        #region Public Methods
        /// <summary>
        /// Log unhandled exception and return HTTP Internal Server Error
        /// </summary>
        /// <param name="actionExecutedContext"><see cref="HttpActionExecutedContext"/></param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var controllerName = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
            var actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
            #pragma warning disable S6668 // Parameter names used in logging statements should match an existing method parameter
            Logger?.Error("Exception in Controller '{ControllerName}', Action Method '{ActionName}'\n{ExceptionName}", controllerName, actionName, actionExecutedContext.Exception);
            #pragma warning restore S6668

            switch (controllerName)
            {
                case "Ping":
                    var errors = actionExecutedContext.Exception.HandleException();
                    actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, errors[errors.Count - 1]);
                    break;
                case "LocationLookup":
                    var locationResponse = new LocationResponse
                    {
                        Id = GetRequestId(actionExecutedContext, actionName),
                        Data = null,
                        Errors = actionExecutedContext.Exception.HandleException()
                    };
                    actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, locationResponse);
                    break;
                case "Test":
                    var testResponse = new TestResponse
                    {
                        Id = GetRequestId(actionExecutedContext, actionName),
                        Data = null,
                        Errors = actionExecutedContext.Exception.HandleException()
                    };
                    actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, testResponse);
                    break;
                    // No Default, no action for unlisted Controllers such as HomeController
            }
        }

        /// <summary>
        /// If the "Get" Action then get the ID from the "id" parameter, else if "Post" Action then get the ID from the request.Id parameter
        /// </summary>
        /// <param name="actionExecutedContext"><see cref="HttpActionExecutedContext"/></param>
        /// <param name="actionName">Action Name</param>
        /// <returns>Request ID</returns>
        private static string GetRequestId(HttpActionExecutedContext actionExecutedContext, string actionName)
        {
            return actionName == "Get"
                ? (string)actionExecutedContext.ActionContext.ActionArguments["id"]
                : ((ApiRequestBase)actionExecutedContext.ActionContext.ActionArguments["request"]).Id;
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
