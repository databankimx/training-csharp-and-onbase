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
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Samples.MvcWebApi.Core.Common;
#endregion

#region Training Notes
/*
 * Samples.MvcWebApi.Client (the classic sibling of this project) deliberately kept the old
 *   HttpWebRequest + JavaScriptSerializer style, "here's what you'll find in legacy code."
 *   This project is the direct, modern contrast: HttpClient, async/await throughout, and
 *   System.Text.Json's typed helpers (PostAsJsonAsync/GetFromJsonAsync), no manual stream
 *   writing, no manual JSON parsing.
 *
 * Also worth knowing: ASP.NET Core's default JSON serialization uses camelCase property
 *   names on the wire ("requestId", not "RequestId"), even though the C# record properties
 *   themselves are PascalCase. HttpClientJsonExtensions handles this automatically as long as
 *   PropertyNameCaseInsensitive is set for deserialization, see BuildJsonOptions() below.
 *
 * *Migration Note: ApiBaseUrl originally lived here as a hardcoded const string, static
 *   analysis correctly flagged that (csharpsquid:S1075). Since this project is a plain
 *   console app, not a WebApplicationBuilder-hosted one, it doesn't get appsettings.json
 *   loading for free, so a small ConfigurationBuilder is wired up by hand below, the same
 *   general idea as Samples.MvcWebApi.Core's own Serilog/CORS configuration, just without the
 *   web-hosting machinery that normally does this automatically. See LectureNotes.md.
 */
#endregion

#region Main Program
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var apiBaseUrl = configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json!");
const string DefaultTestData = "My test data...";
const string DefaultZipCode = "75067";

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

using var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

try
{
    await RunPing();
    await RunTest();
    await RunLocationLookup();
}
catch (Exception? ex)
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
#endregion

#region Helper Methods
// Run the Ping endpoint to verify the API is reachable and responding.
async Task RunPing()
{
    Console.WriteLine($"{Environment.NewLine}Calling GET ping...");
    var response = await client.GetAsync("ping");
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Response: {result}");
    Pause();
}

// Run the Test endpoint to verify POST requests are handled correctly.
async Task RunTest()
{
    Console.WriteLine($"{Environment.NewLine}Calling POST test...");
    Console.WriteLine("Enter some test data...");
    var input = Console.ReadLine();

    var requestId = Guid.NewGuid().ToString();
    var request = new TestRequest(requestId, string.IsNullOrEmpty(input) ? DefaultTestData : input);

    var response = await client.PostAsJsonAsync("test", request, jsonOptions);
    response.EnsureSuccessStatusCode();

    var testResponse = await response.Content.ReadFromJsonAsync<TestResponse>(jsonOptions);
    if (testResponse is null || testResponse.RequestId != requestId)
        throw new InvalidOperationException($"Request ID [{requestId}] does not match response ID [{testResponse?.RequestId}]!");

    Console.WriteLine($"Response Data: {testResponse.Data}");
    Pause();
}

// Run the LocationLookup endpoint to retrieve location information based on a zip code.
async Task RunLocationLookup()
{
    Console.WriteLine($"{Environment.NewLine}Calling GET locationlookup...");
    Console.WriteLine("Enter a zip code...");
    var input = Console.ReadLine();
    var zipCode = string.IsNullOrEmpty(input) ? DefaultZipCode : input;

    var response = await client.GetAsync($"locationlookup/{Uri.EscapeDataString(zipCode)}");
    response.EnsureSuccessStatusCode();

    var locationResponse = await response.Content.ReadFromJsonAsync<LocationLookupResponse>(jsonOptions) ?? throw new InvalidOperationException("No response body was returned!");
    Console.WriteLine("Locations:");
    foreach (var location in locationResponse.Data)
    {
        Console.WriteLine($"  - Zip Code: {location.ZipCode}");
        Console.WriteLine($"    - State:  {location.State}");
        Console.WriteLine($"    - County: {location.County}");
        Console.WriteLine($"    - City:   {location.City}");
    }
    Pause();
}

// Pause the console to allow the user to read the output before continuing.
void Pause()
{
    Console.WriteLine($"{Environment.NewLine}Press <ENTER> to continue...");
    Console.ReadLine();
}
#endregion

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
