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
using CSharp.SharedLibrary.Models;
using Samples.AsmxWebService.Client.Models.Configuration;
#endregion

#pragma warning disable S125
namespace Samples.AsmxWebService.Client
{
    #region Training Notes
    /*
     * ASMX Web Services are the original .NET web service methodology
     *
     * Although these are outdated, and you should NOT develop new services using ASMX, you will inevitably
     *   run into some in support issues, upgrades, etc.
     *
     * Because ASMX publishes a WSDL (accessed as http://<asmx service URL>?wsdl), it is relatively easy to consume in .NET
     * 1. Generate a proxy class from the WSDL - Using the Visual Studio command line console, run the following:
     *      wsdl.exe /l:cs <path to input WSDL file> /o:<path to output .CS file>
     * 2. Using that class in your program, you can simply instantiate the service class as a client
     *      var client = new <WebServiceClass> {Url = <WebServiceURL>};
     * 3. Using the client object generated above, simply call the exposed methods
     *      var result = client.<MethodName>(<arguments>);
     */
    #endregion

    /// <summary>
    /// Sample Console client to consume an ASMX web service
    /// </summary>
    internal static class Program
    {
        #region Constants
        // Default value for TestService data
        private const string DefaultTestData = "Test Data";

        // Default value for LookupLocation data
        private const string DefaultZipCode = "75067";
        #endregion

        #region Globals
        // Config file settings object
        private static WebServiceSettings settings;

        // Web service proxy client
        private static ExampleWebService client;
        #endregion

        #region Main Executable
        private static void Main()
        {
            try
            {
                // Initialize global variables
                Initialize();
                Pause();

                // Generate client channel to consume web service
                Connect();
                Pause();

                // Validate that the service is online
                Ping();
                Pause();

                // Validate that the service is online and can receive data
                TestService();
                Pause();

                // Run the location lookup method from the web service
                LookupLocation();
                Pause();
            }
            catch (Exception ex)
            {
                string nl = Environment.NewLine;
                while (ex != null)
                {
                    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}{nl}{nl}Stack Trace:{nl}{ex.StackTrace}");
                    ex = ex.InnerException;
                }
            }
            finally
            {
                // Close client channel factory for web service
                Disconnect();
                Console.WriteLine("Done! Press <ENTER> to exit...");
                Console.ReadLine();
            }
        }
        #endregion

        #region Helper Functions
        // Initialize global variables
        private static void Initialize()
        {
            try
            {
                settings = (WebServiceSettings)ConfigurationManager.GetSection(WebServiceSettings.SectionName);
                Console.WriteLine("Web service settings initialized...");
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error initializing program!", ex);
            }
        }

        // Open client connection to web service
        private static void Connect()
        {
            try
            {
                client = new ExampleWebService{Url = settings.WebServiceUrl};
                Console.WriteLine("Web service channel opened...");
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error opening web service channel!", ex);
            }
        }

        // Close client for web service
        private static void Disconnect()
        {
            try
            {
                client?.Dispose();
                Console.WriteLine("Web service channel closed...");
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error closing web service channel!", ex);
            }
        }

        // Validate that the service is online
        private static void Ping()
        {
            try
            {
                if (client == null) throw new DatabankException("Cannot execute without an active channel!");
                Console.WriteLine("Executing Ping() method...");

                Console.WriteLine("Press <ENTER> to continue...");
                Console.ReadLine();

                Console.WriteLine(client.Ping());
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error executing web service Ping() method!", ex);
            }
        }

        // Validate that the service is online and can receive data
        private static void TestService()
        {
            try
            {
                if (client == null) throw new DatabankException("Cannot execute without an active channel!");
                Console.WriteLine("Executing TestService() method...");

                Console.WriteLine("Enter some test data to continue...");
                string data = Console.ReadLine();
                if (string.IsNullOrEmpty(data)) data = DefaultTestData;

                string requestId = Guid.NewGuid().ToString();

                var request = new ServiceTestRequest
                {
                    RequestId = requestId,
                    Data = data
                };

                var response = client.TestService(request);
                if (!string.Equals(requestId, response.RequestId, StringComparison.CurrentCultureIgnoreCase))
                    throw new DatabankException($"Request ID [{response.RequestId}] does not match [{requestId}]!");

                if (response.Errors != null) ProcessErrors(response.Errors);
                else Console.WriteLine(response.Data);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error executing web service TestService() method!", ex);
            }
        }

        // Run the location lookup method from the web service
        private static void LookupLocation()
        {
            try
            {
                if (client == null) throw new DatabankException("Cannot execute without an active channel!");
                Console.WriteLine("Executing LookupLocation() method...");

                Console.WriteLine("Enter a zip code to continue...");
                string zipCode = Console.ReadLine();
                if (string.IsNullOrEmpty(zipCode)) zipCode = DefaultZipCode;

                string requestId = Guid.NewGuid().ToString();

                var request = new LocationLookupRequest
                {
                    RequestId = requestId,
                    ZipCode = zipCode
                };

                var response = client.LookupLocation(request);
                if (!string.Equals(requestId, response.RequestId, StringComparison.CurrentCultureIgnoreCase))
                    throw new DatabankException($"Request ID [{response.RequestId}] does not match [{requestId}]!");


                if (response.Errors != null) ProcessErrors(response.Errors);
                else foreach (var location in response.Data) Console.WriteLine(location.Info);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error executing web service LookupLocation() method!", ex);
            }
        }

        // Report any errors returned by the web service
        private static void ProcessErrors(string[] errors)
        {
            Console.WriteLine($"Errors!{Environment.NewLine}-----------------");
            foreach (string error in errors) Console.WriteLine(error);
        }

        // Pause for user to view results
        private static void Pause()
        {
            Console.WriteLine($"{Environment.NewLine}Press <ENTER> to continue...");
            Console.ReadLine();
        }
        #endregion
    }
}
#pragma warning restore S125

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
