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

namespace Samples.RazorPages.Models
{
    /// <summary>
    /// EF Core entity, Code-First, matching every other Samples.*.Core project's own
    /// ZipCode entity, kept consistent across the whole training set.
    /// </summary>
    public class ZipCode
    {
        #region Properties
        /// <summary>
        /// Zip Code ID (Primary Key)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Zip Code's State (e.g., "TX" for Texas)
        /// </summary>
        public required string State { get; set; }

        /// <summary>
        /// Zip Code's County (e.g., "Denton" for 75067)
        /// </summary>
        public required string County { get; set; }

        /// <summary>
        /// Zip Code's City (e.g., "Lewisville" for 75067)
        /// </summary>
        public required string City { get; set; }

        /// <summary>
        /// Zip Code's Postal Code (e.g., "75067")
        /// </summary>
        public required string ZipCode1 { get; set; }
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
