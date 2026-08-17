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

namespace CSharp.Ch05.Supplemental.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines an address for a contact
    /// </summary>
    public class Address
    {
        #region Properties
        /// <summary>
        /// Contact street name and number
        /// </summary>
        public string StreetAddress { get; set; }

        /// <summary>
        /// Contact city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Contact state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Contact ZIP (postal) code
        /// </summary>
        public string ZipCode { get; set; }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
