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

#nullable disable

#region Using Directives
using System.Text.Json.Serialization;
#endregion

namespace EForms.TrainingNavigator.Models
{
    /// <summary>
    /// Represents a geographic location with state, county, city, and ZIP code information.
    /// </summary>
    public class Location
    {
        #region Properties
        /// <summary>
        /// Gets or sets the current state.
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the county name.
        /// </summary>
        [JsonPropertyName("county")]
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the city name.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the ZIP or postal code.
        /// </summary>
        [JsonPropertyName("zipCode")]
        public string ZipCode { get; set; }
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
