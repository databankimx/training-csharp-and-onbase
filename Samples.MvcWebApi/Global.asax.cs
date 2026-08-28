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
using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Samples.MvcWebApi.HelperClasses;
#endregion

namespace Samples.MvcWebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // *Migration Note: log4net's XmlConfigurator.Configure() call replaced with Serilog,
            //   DataBank's current logging standard. serilog.json defines WHERE and in WHAT
            //   FORMAT logs are written (the sink), matching Samples.AsmxWebService/
            //   Samples.WcfService's own pattern. "debugMode" (a plain appSettings entry here,
            //   this project has no custom <serviceSettings> section the way the ASMX/WCF
            //   samples do) drives the minimum level via a LoggingLevelSwitch. See
            //   LectureNotes.md.
            //
            // *Migration Note: ConfigurationManager is fully qualified below,
            //   Microsoft.Extensions.Configuration defines its OWN ConfigurationManager class,
            //   genuinely ambiguous against System.Configuration's once both namespaces are
            //   "using"-imported in the same file, the same gotcha already hit (and fixed) in
            //   Samples.WcfService's ExampleWebService.svc.cs. See LectureNotes.md.
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

            ErrorHandling.DebugMode = debugMode;
            ErrorHandling.Logger = Filters.LogFilter.Logger = Filters.ExceptionFilter.Logger = Log.Logger;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            // Support for Chrome (and other browsers using "OPTIONS" header)
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (HttpContext.Current.Request.HttpMethod != "OPTIONS") return;
            HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
            HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
            HttpContext.Current.Response.End();
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
