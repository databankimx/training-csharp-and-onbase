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

namespace Samples.NuGetLibrary
{
    /// <summary>
    /// A framework-agnostic representation of a ZIP code lookup result. Deliberately not
    /// an EF Core/EF6 entity, this type has no data-access dependency of any kind, and can
    /// be constructed by any consumer (an EF Core Code-First query, an EF6 Database-First
    /// query, a REST API response, a gRPC message) equally easily. See LectureNotes.md.
    /// </summary>
    /// <param name="ZipCode">The ZIP code.</param>
    /// <param name="City">The ZIP code's city.</param>
    /// <param name="County">The ZIP code's county.</param>
    /// <param name="State">The ZIP code's state.</param>
    public record Location(string ZipCode, string City, string County, string State);
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
