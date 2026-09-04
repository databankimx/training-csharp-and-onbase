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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using CSharp.SharedLibrary.Models;
using Samples.MvcWebApi.Common;
#endregion

#pragma warning disable S125 // Allow commented code
namespace Samples.MvcWebApi.Client
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
     *   - In this example, we have created a separate class library project in order to share the request and response models
     *
     * Additionally, because Web APIs do not incorporate a preconfigured service model, consuming them in code uses
     *   the native HttpWebRequest object as opposed to a proxy client. The process is as follows:
     *
     * 1. Create the request object:
     *    var request = (HttpWebRequest)WebRequest.Create("<Web API URL>/<Method>");
     *
     * 2. Write the payload to the request stream:
     *    byte[] payload = <Payload as byte array>;
     *    using (var stream = request.GetRequestStream()) stream.Write(payload, 0, payload.Length);
     *
     * 3. Specify the headers for the request:
     *    request.Accept = "<MIME Type>";
     *    request.Method = "<POST | GET>";
     *    request.ContentType = "<MIME Type>";
     *    request.ContentLength = payload.Length;
     *
     * 4. Execute the request and read out the data
     *    var response = (HttpWebResponse)request.GetResponse();
     *    string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
     */
    #endregion

    internal static class Program
    {
        #region Constants
        // URL to Web API
        // *Migration Note: verified against Samples.MvcWebApi's actual IIS Express SSL port
        //   (44312, set in its own .csproj), see LectureNotes.md.
        private static readonly string WebApiUrl = ConfigurationManager.AppSettings["WebApiUrl"];

        // Data to pass to Test() method if none is entered by the user
        private const string DefaultTestData = "My test data...";

        // Data to pass to LocationLookup() method if none is entered by the user
        private const string DefaultZipCode = "75067";
        #endregion

        #region Private Globals
        // Web API Request GUID
        private static string requestId;
        #endregion

        #region Main Method
        // Main executable method
        private static void Main()
        {
            try
            {
                string result = CallWebApi("Ping");
                ProcessResult("Ping", result);

                result = CallWebApi("Test");
                ProcessResult("Test", result);

                result = CallWebApi("LocationLookup");
                ProcessResult("LocationLookup", result, false);
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine(ex);
                    ex = ex.InnerException;
                }
            }
            finally
            {
                Console.WriteLine($"{Environment.NewLine}Done! Press <ENTER> to exit...");
                Console.ReadLine();
            }
        }
        #endregion

        #region Helper Functions
        // Consume the Web API via HTTP request
        private static string CallWebApi(string method)
        {
            try
            {
                requestId = Guid.NewGuid().ToString();
                string json = CreatePayload(method, requestId);

                var request = (HttpWebRequest)WebRequest.Create($"{WebApiUrl}{method}");
                request.Accept = "application/json";
                request.Method = "POST";
                if (!string.IsNullOrEmpty(json))
                {
                    Console.WriteLine($"{Environment.NewLine}Sending JSON request to {method}...");
                    Console.WriteLine(json);
                    var bytes = Encoding.ASCII.GetBytes(json);
                    request.ContentType = "application/json";
                    request.ContentLength = bytes.Length;
                    if (bytes.Length > 0)
                    {
                        using (var stream = request.GetRequestStream()) stream.Write(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    request.ContentLength = 0;
                    Console.WriteLine($"Sending empty request to {method}...");
                }

                var response = (HttpWebResponse)request.GetResponse();
                return response == null
                    ? throw new DatabankException("No response was returned from the Web API!")
                    : new StreamReader(response.GetResponseStream() ?? new MemoryStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error in CallWebApi method!", ex);
            }
        }

        // Create the JSON payload to the web API request
        private static string CreatePayload(string method, string id)
        {
            try
            {
                switch (method.ToLower())
                {
                    case "test":
                        Console.WriteLine("Enter some test data...");
                        string info = Console.ReadLine();

                        var testRequest = new TestRequest
                        {
                            Id = id,
                            Data = string.IsNullOrEmpty(info) ? DefaultTestData : info
                        };
                        return new JavaScriptSerializer().Serialize(testRequest);
                    case "locationlookup":
                        Console.WriteLine("Enter a zip code...");
                        string zip = Console.ReadLine();

                        var locationRequest = new LocationRequest()
                        {
                            Id = id,
                            ZipCode = string.IsNullOrEmpty(zip) ? DefaultZipCode : zip
                        };
                        return new JavaScriptSerializer().Serialize(locationRequest);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error creating JSON payload for API call!", ex);
            }
        }

        // Process and display the results returned from the Web API
        private static void ProcessResult(string method, string json, bool pause = true)
        {
            try
            {
                Console.WriteLine($"{Environment.NewLine}Received JSON:{Environment.NewLine}{json}");

                switch (method.ToLower())
                {
                    case "ping":
                        break;
                    case "test":
                        var testResponse = new JavaScriptSerializer().Deserialize<TestResponse>(json);
                        if (testResponse.Errors != null)
                        {
                            ProcessApiErrors(testResponse.Errors);
                            break;
                        }
                        if (!string.Equals(testResponse.Id, requestId))
                            throw new DatabankException($"Request ID [{requestId}] does not match response ID [{testResponse.Id}]!");
                        Console.WriteLine($"{Environment.NewLine}Processed Test Response:");
                        Console.WriteLine($" - ID:   {testResponse.Id}");
                        Console.WriteLine($" - Data: {testResponse.Data}");
                        break;
                    case "locationlookup":
                        var locationResponse = new JavaScriptSerializer().Deserialize<LocationResponse>(json);
                        if (locationResponse.Errors != null)
                        {
                            ProcessApiErrors(locationResponse.Errors);
                            break;
                        }
                        if (!string.Equals(locationResponse.Id, requestId))
                            throw new DatabankException($"Request ID [{requestId}] does not match response ID [{locationResponse.Id}]!");
                        Console.WriteLine($"{Environment.NewLine}Processed Location Lookup Response:");
                        Console.WriteLine($" - ID:   {locationResponse.Id}");
                        Console.WriteLine(" - Locations:");
                        foreach (var location in locationResponse.Data)
                        {
                            Console.WriteLine($"   - Zip Code: {location.ZipCode}");
                            Console.WriteLine($"     - State:  {location.State}");
                            Console.WriteLine($"     - County: {location.County}");
                            Console.WriteLine($"     - City:   {location.City}");
                        }
                        break;
                    default:
                        throw new DatabankException($"Method [{method}] not supported!");
                }

                if (pause) Pause();
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error processing result JSON!", ex);
            }
        }

        // Display errors returned by the Web API
        private static void ProcessApiErrors(List<string> errors)
        {
            foreach (string error in errors)
            {
                Console.WriteLine(error);
            }
        }

        // Pause for user input
        private static void Pause()
        {
            Console.WriteLine($"{Environment.NewLine}Press <ENTER> to continue...");
            Console.ReadLine();
            Console.Clear();
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
