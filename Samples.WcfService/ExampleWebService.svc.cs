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
using System.IO;
using System.Runtime.Serialization.Json;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using CSharp.SharedLibrary.Models;
using Samples.WcfService.HelperClasses;
using Samples.WcfService.Models.Configuration;
using Samples.WcfService.Models.Objects;
#endregion

#region Training Notes
/*
 * WCF (Windows Communication Foundation) Services are the second generation of .NET web service methodology
 *
 * Some claim that WCF is outdated (or at least outmoded), having been surpassed functionally by the MVC framework,
 *   but for many implementations, WCF is still the most efficient means of exposing web endpoints
 *
 * This service exposes TWO endpoints from the SAME contract (IExampleWebService), see Web.config's
 *   <system.serviceModel><services> section:
 *   - "appEndpoint" (basicHttpBinding): a genuine SOAP/WSDL endpoint for .NET clients, consumed by
 *     Samples.WcfService.Client via ChannelFactory<IExampleWebService>
 *   - "webEndpoint" (webHttpBinding): a REST/JSON endpoint for the browser, consumed by
 *     Samples.WcfService.WebClient via plain AJAX
 *
 * That's why TestServiceRest()/LookupLocationRest() exist alongside TestService()/LookupLocation():
 *   the REST-suffixed operations use [WebGet] with a UriTemplate, meaningful only for the "webEndpoint",
 *   and manually serialize their own JSON response, while the plain operations rely on WCF's normal
 *   contract-based serialization, usable from either endpoint.
 */
#endregion

namespace Samples.WcfService
{
    public class ExampleWebService : IExampleWebService
    {
        #region Private Members
        // Web service config settings
        private static readonly ServiceSettings Settings;

        // Serilog logging utility
        private static readonly ILogger Logger;
        #endregion

        #region IExampleWebService
        /// <summary>
        /// Verify that the web service is online
        /// </summary>
        /// <returns>Online validation string</returns>
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
        /// Verify that the web service is online and can accept incoming data (REST)
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="data">Test Data</param>
        /// <returns>JSON encoded result object</returns>
        public string TestServiceRest(string requestId, string data)
        {
            var ser = new DataContractJsonSerializer(typeof(ServiceTestResponse));
            var ms = new MemoryStream();
            ServiceTestResponse response;
            try
            {
                var request = new ServiceTestRequest
                {
                    RequestId = requestId,
                    Data = data
                };
                response = TestService(request);
            }
            catch (Exception ex)
            {
                response = new ServiceTestResponse
                {
                    RequestId = requestId,
                    Data = null,
                    Errors = ex.HandleException()
                };
            }
            ser.WriteObject(ms, response);
            return StreamToString(ms);
        }

        /// <summary>
        /// Look up location details by Zip code
        /// </summary>
        /// <param name="request"><see cref="LocationLookupRequest"/></param>
        /// <returns><see cref="LocationLookupResponse"/></returns>
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

        /// <summary>
        /// Look up location details by Zip code (REST)
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="zipCode">Zip Code for Lookup</param>
        /// <returns>JSON encoded result object</returns>
        public string LookupLocationRest(string requestId, string zipCode)
        {
            var ser = new DataContractJsonSerializer(typeof(LocationLookupResponse));
            var ms = new MemoryStream();
            LocationLookupResponse response;
            try
            {
                var request = new LocationLookupRequest
                {
                    RequestId = requestId,
                    ZipCode = zipCode
                };
                response = LookupLocation(request);
            }
            catch (Exception ex)
            {
                response = new LocationLookupResponse
                {
                    RequestId = requestId,
                    Data = null,
                    Errors = ex.HandleException()
                };
            }
            ser.WriteObject(ms, response);
            return StreamToString(ms);
        }
        #endregion

        #region Helper Functions
        // Convert stream to string
        private static string StreamToString(Stream stream)
        {
            try
            {
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error converting stream to string!", ex);
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize global variables on first ExampleWebService instance creation
        /// </summary>
        #pragma warning disable S3963 // Using static constructor to initialize static members is acceptable here, as this is a WCF service class
        static ExampleWebService()
        #pragma warning restore S3963
        {
            Settings = (ServiceSettings)System.Configuration.ConfigurationManager.GetSection(ServiceSettings.SectionName);

            // serilog.json defines WHERE and in WHAT FORMAT logs are written (the sink), that
            //   can change without a recompile. It deliberately does NOT define a MinimumLevel,
            //   that's still driven right here, from Web.config's existing debugMode setting,
            //   via a LoggingLevelSwitch, matching Samples.AsmxWebService's own pattern exactly.
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
