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
    /// Defines a Contact as a subclass of person and as a superclass for all address book contacts
    /// </summary>
    public class Contact : Person
    {
        #region Properties
        /// <summary>
        /// Home phone number
        /// </summary>
        public Telephone HomePhone { get; set; }

        /// <summary>
        /// Work phone number
        /// </summary>
        public Telephone WorkPhone { get; set; }

        /// <summary>
        /// Cell phone number
        /// </summary>
        public Telephone MobilePhone { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set;  }

        /// <summary>
        /// Home mailing address
        /// </summary>
        public Address HomeAddress { get; set; }

        /// <summary>
        /// Work mailing address
        /// </summary>
        public BusinessAddress WorkAddress { get; set; }
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
