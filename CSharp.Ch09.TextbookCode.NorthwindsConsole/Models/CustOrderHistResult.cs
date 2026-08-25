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

namespace NorthwindsConsole.Models
{
    /// <summary>
    /// Maps to one row of the CustOrderHist stored procedure's result set (see README.md
    /// for the procedure's definition)
    /// </summary>
    public class CustOrderHistResult
    {
        #region Properties
        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Total quantity ordered
        /// </summary>
        public int Total { get; set; }
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
