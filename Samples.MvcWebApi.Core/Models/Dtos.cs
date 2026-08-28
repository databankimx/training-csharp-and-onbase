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

namespace Samples.MvcWebApi.Core.Models
{
    /// <summary>
    /// Records returned by LocationLookupController, a plain projection of ZipCode, not the
    /// EF Core entity itself, see LectureNotes.md for why that separation matters.
    /// </summary>
    public record Location(string State, string County, string City, string ZipCode);

    public record LocationLookupResponse(string RequestId, IReadOnlyList<Location> Data);

    public record TestRequest(string RequestId, string Data);

    public record TestResponse(string RequestId, string Data);
}
