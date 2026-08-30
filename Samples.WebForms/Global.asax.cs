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

using System;
using System.Configuration;
using System.Web;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Samples.WebForms
{
    public class Global : HttpApplication
    {
        // Serilog logging utility, shared by every page's code-behind in this project.
        public static ILogger Logger { get; private set; }

        protected void Application_Start(object sender, EventArgs e)
        {
            // Serilog setup matches the pattern already established in
            //   Samples.AsmxWebService/Samples.WcfService/Samples.MvcWebApi: serilog.json
            //   defines the sink, "debugMode" (a plain appSettings entry here, this project has
            //   no custom <serviceSettings> section) drives the minimum level via a
            //   LoggingLevelSwitch.
            bool debugMode = bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["debugMode"], out bool parsed) && parsed;

            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("serilog.json", optional: false, reloadOnChange: true)
                .Build();

            var levelSwitch = new LoggingLevelSwitch(debugMode ? LogEventLevel.Debug : LogEventLevel.Error);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Logger = Log.Logger;
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception != null) Logger?.Error(exception, "Unhandled exception on {Path}", Request.Path);
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}
