#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
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

#nullable enable

#region Using Directives
using System.Text.Json.Serialization;
#endregion

namespace EForms.TrainingNavigator.Models
{
    /// <summary>
    /// Represents the result of a location lookup request, including returned locations and any reported errors.
    /// </summary>
    /// <remarks>Contains the request identifier for correlation, a collection of matching locations, and a
    /// collection of error messages when the lookup is not fully successful.</remarks>
    public class LocationLookupResponse
    {
        #region Properties
        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets or sets the collection of locations.
        /// </summary>
        [JsonPropertyName("data")]
        public List<Location> Data { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of error messages.
        /// </summary>
        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = [];
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
