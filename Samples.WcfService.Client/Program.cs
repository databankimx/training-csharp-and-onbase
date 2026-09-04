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
using System.ServiceModel;
using CSharp.SharedLibrary.Models;
using Samples.WcfService.Client.Models.Configuration;
using Samples.WcfService.Models.Objects;
#endregion

#pragma warning disable S125 // Allow commented out code
#region Training Notes
/*
 * WCF (Windows Communication Foundation) Services are the second generation of .NET web service methodology
 *
 * Some claim that WCF is outdated (or at least outmoded), having been surpassed functionally by the MVC framework,
 *   but for many implementations, WCF is still the most efficient means of exposing web endpoints
 *
 * Because WCF publishes a WSDL (accessed as http://<WCF service URL>?singlewsdl), it is relatively easy to consume in .NET
 *   * Note: Always use ?singlewsdl as opposed to ?wsdl, since this will include all of the data contracts (data type classes)
 *
 * 1. Generate a proxy class from the WSDL - Using the Visual Studio command line console, run the following:
 *      svcutil.exe /l:cs <path to input WSDL file> /o:<path to output .CS file>
 *
 * 2. Generate a binding object matching one in the web.config file (typically BasicHttpBinding)
 *      var binding = new BasicHttpBinding();
 *
 * 3. If you are using SSL/TLS (URL starts with HTTPS:), enable transport security
 *      binding.Security.Mode = BasicHttpSecurityMode.Transport;
 *
 * 4. Create an endpoint address
 *      var address = new EndpointAddress(settings.WebServiceUrl);
 *
 * 5. There are two options for instantiating a client object at this point
 *    a. Use the generated "client" object in the class
 *       var client = new <ServiceName>Client(binding, address);
 *
 *       or
 *
 *    b. Create a channel factory and a channel instance (this allows you to open multiple channels at once)
 *       var factory = new ChannelFactory<IExampleWebService>(binding, address);
 *       var client = factory.CreateChannel();
 *
 * 6. Using the client object generated above, simply call the exposed methods
 *       var result = client.<MethodName>(<arguments>);
 */
#endregion

namespace Samples.WcfService.Client
{
    /// <summary>
    /// Sample Console client to consume a WCF web service
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

        // Web service proxy client factory
        private static ChannelFactory<IExampleWebService> factory;

        // Web service proxy client
        private static IExampleWebService channel;
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

        // Generate client channel to consume web service
        private static void Connect()
        {
            try
            {
                var binding = new BasicHttpBinding();
                if (settings.WebServiceUrl.ToLower().StartsWith("https")) binding.Security.Mode = BasicHttpSecurityMode.Transport;

                var address = new EndpointAddress(settings.WebServiceUrl);

                factory = new ChannelFactory<IExampleWebService>(binding, address);
                channel = factory.CreateChannel();
                Console.WriteLine("Web service channel opened...");
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error opening web service channel!", ex);
            }
        }

        // Close client channel factory for web service
        private static void Disconnect()
        {
            try
            {
                factory?.Close();
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
                if (channel == null) throw new DatabankException("Cannot execute without an active channel!");
                Console.WriteLine("Executing Ping() method...");

                Console.WriteLine("Press <ENTER> to continue...");
                Console.ReadLine();

                Console.WriteLine(channel.Ping());
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
                if (channel == null) throw new DatabankException("Cannot execute without an active channel!");
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

                var response = channel.TestService(request);
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
                if (channel == null) throw new DatabankException("Cannot execute without an active channel!");
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

                var response = channel.LookupLocation(request);
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

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
