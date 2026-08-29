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

namespace Samples.MvcWebApi.Core.Common
{
    // Shared request/response shapes for Samples.MvcWebApi.Core and
    // Samples.MvcWebApi.Core.Client, moved into this separate project once a second
    // consumer of them existed, exactly the same rationale as the classic
    // Samples.MvcWebApi.Common, ASP.NET Core Web API still has no built-in WSDL/data
    // contract, same limitation as classic Web API 2, see LectureNotes.md.

    #region Request/Response Shapes
    /// <summary>
    /// Represents a location with state, county, city, and zip code information.
    /// </summary>
    /// <param name="State">The state of the location.</param>
    /// <param name="County">The county of the location.</param>
    /// <param name="City">The city of the location.</param>
    /// <param name="ZipCode">The zip code of the location.</param>
    public record Location(string State, string County, string City, string ZipCode);

    /// <summary>
    /// Represents a request for location lookup, containing a request ID and a list of locations.
    /// </summary>
    /// <param name="RequestId">The unique identifier for the request.</param>
    /// <param name="Data">The list of locations returned in the response.</param>
    public record LocationLookupResponse(string RequestId, IReadOnlyList<Location> Data);

    /// <summary>
    /// Represents a request with an identifier and associated data.
    /// </summary>
    /// <param name="RequestId">The unique identifier for the request.</param>
    /// <param name="Data">The data associated with the request.</param>
    public record TestRequest(string RequestId, string Data);

    /// <summary>
    /// Represents a response containing a request identifier and associated data.
    /// </summary>
    /// <param name="RequestId">Unique identifier of the request.</param>
    /// <param name="Data">Data returned for the request.</param>
    public record TestResponse(string RequestId, string Data);
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
