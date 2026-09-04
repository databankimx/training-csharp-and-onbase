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
 * ******************************************************************** */
#endregion

#region Using Directives
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Samples.Grpc;
#endregion

#region Training Notes
/*
 * LocationLookupService.LocationLookupServiceClient (constructed below) is generated at
 *   BUILD TIME from ..\Samples.Grpc\Protos\locationlookup.proto (see this project's own
 *   .csproj), the SAME .proto file the server itself uses, not a duplicated or hand-
 *   written copy. There's no separate DTO library the way Samples.MvcWebApi.Client needs
 *   Samples.MvcWebApi.Common for, the generated client and the generated server base
 *   class both come from the identical source of truth.
 *
 * The "await foreach" loop below is what makes this genuinely different from
 *   Samples.MvcWebApi.Core.Client's HttpClient calls: LookupLocation() returns
 *   immediately with an open stream, and each LocationReply arrives and is processed as
 *   soon as the server writes it, not after the whole response has been received and
 *   deserialized. For this sample's small result sets the difference isn't dramatic, but
 *   the SAME code would behave identically against a server streaming thousands of rows,
 *   nothing here needs to change to handle that.
 */
#endregion

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var serverUrl = configuration["GrpcServerUrl"]
    ?? throw new InvalidOperationException("GrpcServerUrl is not configured in appsettings.json!");

Console.WriteLine("Enter a zip code to search...");
var zipCode = Console.ReadLine();
if (string.IsNullOrWhiteSpace(zipCode)) zipCode = "75067";

try
{
    using var channel = GrpcChannel.ForAddress(serverUrl);
    var client = new LocationLookupService.LocationLookupServiceClient(channel);

    using var call = client.LookupLocation(new ZipCodeRequest { ZipCode = zipCode });

    var found = false;
    await foreach (var location in call.ResponseStream.ReadAllAsync())
    {
        found = true;
        Console.WriteLine($"  - Zip Code: {location.ZipCode}");
        Console.WriteLine($"    - State:  {location.State}");
        Console.WriteLine($"    - County: {location.County}");
        Console.WriteLine($"    - City:   {location.City}");
    }

    if (!found) Console.WriteLine($"No results found for ZIP code {zipCode}.");
}
catch (Exception ex)
{
    Exception? current = ex;
    while (current != null)
    {
        Console.WriteLine(current);
        current = current.InnerException;
    }
}
finally
{
    Console.WriteLine($"{Environment.NewLine}Done! Press <ENTER> to exit...");
    Console.ReadLine();
}
