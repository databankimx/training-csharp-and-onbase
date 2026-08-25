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

namespace NorthwindsClient.Models
{
    /// <summary>
    /// Client-side shape of a Category, as seen through the OData service, not the same
    /// class as CSharp.Ch09.TextbookCode.NorthwindsWCFDataService's own Category (that one
    /// maps to the database table directly; this one is just a plain client-side model the
    /// OData client deserializes service responses into).
    /// </summary>
    public class Category
    {
        #region Properties
        /// <summary>
        /// Category ID
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Category description
        /// </summary>
        public string Description { get; set; }
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
