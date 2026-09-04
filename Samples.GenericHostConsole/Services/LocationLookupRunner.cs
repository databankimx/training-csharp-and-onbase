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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Samples.GenericHostConsole.Data;
#endregion

namespace Samples.GenericHostConsole.Services
{
    #region Training Notes
    /*
     * This is a plain class, not a BackgroundService, and Program.cs never calls
     * host.Run()/host.RunAsync() anywhere. That's the whole point of this sample: the
     * Generic Host is genuinely useful even when nothing needs to run continuously,
     * dependency injection, configuration binding, and structured logging are all things
     * a one-shot CLI tool wants too, not just a long-running daemon.
     *
     * LocationLookupContext is injected directly here (not IDbContextFactory, the
     * pattern Samples.Blazor.Server/Samples.Grpc use), because THIS class's own lifetime
     * genuinely matches the DbContext's: both are created fresh within the SAME explicit
     * scope Program.cs creates for this one run (see host.Services.CreateScope() there),
     * used once, and disposed together at the end of that scope. There's no long-lived
     * circuit or per-request boundary to worry about outliving the context the way there
     * is in those other samples, just one scope, created once, for one unit of work.
     */
    #endregion

    /// <summary>
    /// Looks up city/county/state for a single ZIP code and writes the result to the
    /// console, the same task as every other sample in this training set.
    /// </summary>
    /// <param name="db">The database context to query.</param>
    /// <param name="logger">The logger this runner writes to.</param>
    public class LocationLookupRunner(LocationLookupContext db, ILogger<LocationLookupRunner> logger)
    {
        #region Public Methods
        /// <summary>
        /// Looks up city/county/state for <paramref name="zipCode"/> and writes the
        /// result to the console.
        /// </summary>
        /// <param name="zipCode">The ZIP code to look up.</param>
        /// <param name="cancellationToken">A token used to cancel the operation.</param>
        public async Task RunAsync(string zipCode, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Looking up locations for ZIP code {ZipCode}.", zipCode);

            var results = await db.ZipCodes
                .Where(z => z.ZipCode1 == zipCode)
                .ToListAsync(cancellationToken);

            if (results.Count == 0)
            {
                Console.WriteLine($"No results found for ZIP code {zipCode}.");
                return;
            }

            foreach (var location in results)
            {
                Console.WriteLine($"  - Zip Code: {location.ZipCode1}");
                Console.WriteLine($"    - State:  {location.State}");
                Console.WriteLine($"    - County: {location.County}");
                Console.WriteLine($"    - City:   {location.City}");
            }
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
