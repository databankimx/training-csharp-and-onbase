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
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using Unity.TestHarness.Web.Infrastructure;
#endregion

namespace Unity.TestHarness.Web
{
    #region Training Notes
    /*
     * *Migration Note: serilog.json is still the single source of truth for logging
     * settings, still editable without a recompile, exactly like Unity.TestHarness (the
     * WPF version)'s own App.xaml.cs. What changed is HOW it's read: the original
     * approach (Microsoft.Extensions.Configuration + Serilog.Settings.Configuration,
     * matching the WPF version) pulled in a deep transitive dependency chain
     * (Microsoft.Extensions.Configuration.Abstractions, .FileExtensions,
     * Microsoft.Extensions.FileProviders.*, Microsoft.Extensions.Primitives, ...) that
     * triggered a CASCADING series of binding-redirect FileLoadExceptions specific to
     * this being a legacy Web Application project (AutoGenerateBindingRedirects, which
     * works reliably for the WPF EXE, was NOT reliably generating/refreshing entries for
     * this whole chain here). Parsing serilog.json directly with Newtonsoft.Json (already
     * a dependency for this project, and one with a far shallower footprint) and building
     * LoggerConfiguration from the PARSED VALUES avoids that entire dependency chain
     * while keeping every value (levels, file paths, rolling behavior) genuinely
     * file-driven, nothing here is a hardcoded literal duplicating serilog.json's content.
     * Only the File sinks (and MinimumLevel/Override) are parsed, matching what
     * serilog.json for this project actually declares (no Console sink, see Global.asax.cs's
     * earlier note on why).
     */
    #endregion

    /// <summary>
    /// The ASP.NET MVC application entry point.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        #region Methods
        /// <summary>
        /// Runs once, at application startup.
        /// </summary>
        protected void Application_Start()
        {
            ConfigureLogging();

            Log.Information("Unity Test Harness Web starting up.");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Runs once, at application shutdown.
        /// </summary>
        protected void Application_End()
        {
            Log.Information("Unity Test Harness Web shutting down.");
            Log.CloseAndFlush();
        }

        /// <summary>
        /// Runs when a session expires (InProc mode only, already in use, see
        /// Web.config). The only server-side signal available at all for "this browser
        /// tab was closed without a manual Disconnect": a closed tab sends nothing to the
        /// server, so this only fires once the session's own inactivity timeout elapses,
        /// a delayed safety net, not immediate cleanup.
        /// </summary>
        protected void Session_End()
        {
            SessionConnectionManager.DisconnectOrphanedSession(Session);
        }
        #endregion

        #region Private Methods
        // Read serilog.json and build Log.Logger from its parsed values (see Training
        // Notes above for why this doesn't go through Microsoft.Extensions.Configuration)
        private static void ConfigureLogging()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serilog.json");
            var root = JObject.Parse(File.ReadAllText(path))["Serilog"];

            var loggerConfig = new LoggerConfiguration();

            var minimumLevel = root["MinimumLevel"];
            loggerConfig.MinimumLevel.Is(ParseLevel(minimumLevel["Default"]?.Value<string>()));

            if (minimumLevel["Override"] is JObject overrides)
            {
                foreach (var property in overrides.Properties())
                {
                    loggerConfig.MinimumLevel.Override(property.Name, ParseLevel(property.Value.Value<string>()));
                }
            }

            foreach (var sink in root["WriteTo"])
            {
                if (sink["Name"]?.Value<string>() != "File") continue;

                var writeToArgs = sink["Args"];
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, writeToArgs["path"].Value<string>());

                loggerConfig.WriteTo.File(
                    filePath,
                    restrictedToMinimumLevel: ParseLevel(writeToArgs["restrictedToMinimumLevel"]?.Value<string>()),
                    outputTemplate: writeToArgs["outputTemplate"]?.Value<string>() ?? "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: ParseRollingInterval(writeToArgs["rollingInterval"]?.Value<string>()),
                    fileSizeLimitBytes: writeToArgs["fileSizeLimitBytes"]?.Value<long?>(),
                    rollOnFileSizeLimit: writeToArgs["rollOnFileSizeLimit"]?.Value<bool?>() ?? false,
                    retainedFileCountLimit: writeToArgs["retainedFileCountLimit"]?.Value<int?>());
            }

            Log.Logger = loggerConfig.CreateLogger();
        }

        // Parse a Serilog level name (e.g. "Verbose", "Information") from serilog.json
        // Serilog's own LogEventLevel member names match these strings exactly.
        private static LogEventLevel ParseLevel(string value)
        {
            return string.IsNullOrEmpty(value)
                ? LogEventLevel.Information
                : (LogEventLevel)Enum.Parse(typeof(LogEventLevel), value, ignoreCase: true);
        }

        // Parse a Serilog rolling interval name (e.g. "Day") from serilog.json
        private static RollingInterval ParseRollingInterval(string value)
        {
            return string.IsNullOrEmpty(value)
                ? RollingInterval.Infinite
                : (RollingInterval)Enum.Parse(typeof(RollingInterval), value, ignoreCase: true);
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
