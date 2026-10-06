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
    /// Represents a request to look up location information by ZIP code and correlate it with a request identifier.
    /// </summary>
    public class LocationLookupRequest
    {
        #region Properties
        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets or sets the ZIP Code.
        /// </summary>
        [JsonPropertyName("zipCode")]
        public string? ZipCode { get; set; }
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
