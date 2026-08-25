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

#region Using Directives
using CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Attributes;
#endregion

namespace CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Objects
{
    /// <summary>
    /// Represents a database row for a customer, with its entire column mapping table
    /// expressed as stacked DataMappingAttribute instances (AllowMultiple = true)
    /// </summary>
    [DataMapping("cust_id", "Id")]
    [DataMapping("cust_name", "Name")]
    [DataMapping("cust_email", "Email")]
    public class CustomerRecord
    {
        #region Properties
        /// <summary>
        /// Customer ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Customer Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Customer Email
        /// </summary>
        public string Email { get; set; }
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
