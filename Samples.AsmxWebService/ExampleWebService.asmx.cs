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
using System.Configuration;
using System.Web.Script.Services;
using System.Web.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Samples.AsmxWebService.HelperClasses;
using Samples.AsmxWebService.Models.Configuration;
using Samples.AsmxWebService.Models.Objects;
#endregion

#region Training Notes
/*
 * ASMX Web Services are the original .NET web service methodology
 *
 * Although these are outdated, and you should NOT develop new services using ASMX, you will inevitably
 *   run into some in support issues, upgrades, etc.
 *
 * ASMX is relatively simple to implement (using the System.Web.Services namespace), but it has some limitations:
 *
 * 1. ASMX has no mechanism for defining a RESTful URI and thus is unable to be implemented using the most current consumption methods
 * 2. Although you can force it to accept and return JSON, ASMX is inherently SOAP based
 *
 * *Migration Note: the original version of this note also claimed "ASMX only supports TLS 1.1", that's
 * not accurate, TLS version is negotiated by the OS/.NET Framework itself (via SchUseStrongCrypto and
 * similar settings), not restricted by ASMX as a technology. Corrected here, see LectureNotes.md.
 */
#endregion

namespace Samples.AsmxWebService
{
    /// <summary>
    /// Summary description for ExampleWebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // The following line allows this Web Service to be called from JavaScript in an HTML form
    [ScriptService]
    public class ExampleWebService : WebService
    {
        #region Private Members
        // Web service config settings
        private static readonly ServiceSettings Settings;

        // Serilog logging utility
        private static readonly ILogger Logger;
        #endregion

        #region Web Service Methods
        /// <summary>
        /// Verify that the web service is online
        /// </summary>
        /// <returns>Online validation string</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Ping()
        {
            try
            {
                if (Settings.DebugMode) Logger.Debug("Executing Ping() method...");
                return $"{Stamp.DateAndTime()} - The web service is running.";
            }
            catch (Exception ex)
            {
                var errors = ex.HandleException();
                return errors[errors.Count - 1];
            }
        }

        /// <summary>
        /// Verify that the web service is online and can accept incoming data
        /// </summary>
        /// <param name="request"><see cref="ServiceTestRequest"/></param>
        /// <returns><see cref="ServiceTestResponse"/></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ServiceTestResponse TestService(ServiceTestRequest request)
        {
            try
            {
                if (Settings.DebugMode) Logger.Debug("Executing TestService() method...");
                return new ServiceTestResponse
                {
                    RequestId = request.RequestId,
                    Data = $"{Stamp.DateAndTime()} - The web service is running and received data: [{request.Data}]",
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                return new ServiceTestResponse
                {
                    RequestId = request.RequestId,
                    Data = null,
                    Errors = ex.HandleException()
                };
            }
        }

        /// <summary>
        /// Look up location details by Zip code
        /// </summary>
        /// <param name="request"><see cref="LocationLookupRequest"/></param>
        /// <returns><see cref="LocationLookupResponse"/></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public LocationLookupResponse LookupLocation(LocationLookupRequest request)
        {
            try
            {
                if (Settings.DebugMode) Logger.Debug("Executing LookupLocation() method...");
                return new LocationLookupResponse
                {
                    RequestId = request.RequestId,
                    Data = request.ZipCode.LookupLocations(),
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                return new LocationLookupResponse()
                {
                    RequestId = request.RequestId,
                    Data = null,
                    Errors = ex.HandleException()
                };
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize global variables on first ExampleWebService instance creation
        /// </summary>
        #pragma warning disable S3963 // Intentionally illustrating a static constructor
        static ExampleWebService()
        #pragma warning restore S3963
        {
            Settings = (ServiceSettings)System.Configuration.ConfigurationManager.GetSection(ServiceSettings.SectionName);

            // serilog.json defines WHERE and in WHAT FORMAT logs are written (the sink), that
            //   can change without a recompile. It deliberately does NOT define a MinimumLevel,
            //   that's still driven right here, from Web.config's existing debugMode setting,
            //   via a LoggingLevelSwitch, exactly the same runtime toggle the original
            //   hand-coded version had, just with the sink details moved out to JSON.
            //
            // *Migration Note: ConfigurationManager and ConfigurationBuilder are both fully
            //   qualified below, Microsoft.Extensions.Configuration defines its OWN classes
            //   with those exact names, genuinely ambiguous against System.Configuration's
            //   classes of the same name (yes, System.Configuration really does have its own
            //   ConfigurationBuilder too, the abstract base for the configBuilders feature),
            //   with both namespaces "using"-imported, the compiler can't pick one on its own.
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("serilog.json", optional: false, reloadOnChange: true)
                .Build();

            var levelSwitch = new LoggingLevelSwitch(Settings.DebugMode ? LogEventLevel.Debug : LogEventLevel.Error);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            Logger = Log.Logger;

            Database.Settings = Settings.Database;
            Database.DebugMode = ErrorHandling.DebugMode = Settings.DebugMode;
            Database.Logger = ErrorHandling.Logger = Logger;
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
