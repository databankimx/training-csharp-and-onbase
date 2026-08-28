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
using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Serilog;
#endregion

namespace Samples.MvcWebApi.Filters
{
    /*
     * In MVC, you can add functionality in the form of filters.
     * These provide functionality that executes either before or after an MVC action
     *
     * Filters can be applied to an action method or controller in a declarative or programmatic way.
     * Declarative means by applying a filter attribute to an action method or controller class,
     * and programmatic means by implementing a corresponding interface.
     */

    /// <summary>
    /// Logs the Start and End of an Action Method
    /// </summary>
    internal class LogFilter : ActionFilterAttribute
    {
        #region Properties
        /// <summary>
        /// Serilog logging utility
        /// </summary>
        public static ILogger Logger { get; set; } = Log.Logger;
        #endregion

        #region Public Methods
        /// <summary>
        /// Log Action Method begin
        /// </summary>
        /// <param name="actionContext"><see cref="HttpActionContext"/></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
            var actionName = actionContext.ActionDescriptor.ActionName;
            Logger?.Debug($"Action Method Begin @ {DateTime.Now} in Controller '{controllerName}', Action Method '{actionName}'");
        }

        /// <summary>
        /// Log Action Method end
        /// </summary>
        /// <param name="actionExecutedContext"><see cref="HttpActionExecutedContext"/></param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var controllerName = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
            var actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
            Logger?.Debug($"Action Method End @ {DateTime.Now} in Controller '{controllerName}', Action Method '{actionName}'");
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
