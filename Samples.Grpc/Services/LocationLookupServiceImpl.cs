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
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Samples.Grpc.Data;
#endregion

namespace Samples.Grpc.Services
{
    #region Training Notes
    /*
     * LocationLookupServiceBase (below) is generated at BUILD TIME from
     * Protos/locationlookup.proto, not written by hand, the same way an EF6 EDMX
     * generates ZipCode/LocationLookupModel.Context.cs elsewhere in this training set.
     * Overriding LookupLocation here is the ENTIRE implementation, everything else
     * (message serialization, HTTP/2 framing, the actual RPC dispatch) is handled by the
     * Grpc.AspNetCore framework.
     *
     * responseStream.WriteAsync(...) is what makes this genuinely different from a plain
     * unary RPC (or a REST endpoint): each LocationReply is sent to the client as soon as
     * it's ready, over the SAME HTTP/2 connection, rather than being collected into one
     * list and returned all at once. For a single ZIP code this distinction barely
     * matters (there's usually only one or two matching rows), but the pattern itself
     * scales to genuinely large result sets without ever buffering the whole response in
     * memory on either side, worth knowing even though this sample's own dataset is small.
     */
    #endregion

    /// <summary>
    /// Implements the <c>LocationLookupService</c> gRPC service defined in
    /// <c>Protos/locationlookup.proto</c>. Looks up city/county/state by ZIP code, the
    /// same task as every other sample in this training set, streaming each match back to
    /// the caller as it's found.
    /// </summary>
    /// <param name="dbContextFactory">Used to create a fresh, short-lived <see cref="LocationLookupContext"/> for each call.</param>
    /// <param name="logger">The logger this service writes to.</param>
    public class LocationLookupServiceImpl(
        IDbContextFactory<LocationLookupContext> dbContextFactory,
        ILogger<LocationLookupServiceImpl> logger) : LocationLookupService.LocationLookupServiceBase
    {
        #region Public Methods
        /// <summary>
        /// Looks up city/county/state for the ZIP code in <paramref name="request"/>,
        /// writing each matching row to <paramref name="responseStream"/> as it's found.
        /// </summary>
        /// <param name="request">The ZIP code to look up.</param>
        /// <param name="responseStream">The stream results are written to.</param>
        /// <param name="context">Per-call metadata and the call's <see cref="CancellationToken"/>.</param>
        public override async Task LookupLocation(ZipCodeRequest request, IServerStreamWriter<LocationReply> responseStream, ServerCallContext context)
        {
            logger.LogInformation("Looking up locations for ZIP code {ZipCode}.", request.ZipCode);

            await using var db = await dbContextFactory.CreateDbContextAsync(context.CancellationToken);

            var query = db.ZipCodes
                .Where(z => z.ZipCode1 == request.ZipCode)
                .AsAsyncEnumerable();

            await foreach (var zipCode in query.WithCancellation(context.CancellationToken))
            {
                await responseStream.WriteAsync(new LocationReply
                {
                    State = zipCode.State,
                    County = zipCode.County,
                    City = zipCode.City,
                    ZipCode = zipCode.ZipCode1
                });
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
