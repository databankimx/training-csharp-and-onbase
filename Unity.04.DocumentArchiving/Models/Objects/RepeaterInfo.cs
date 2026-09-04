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
using System.Collections.Generic;
#endregion

namespace Unity._04.DocumentArchiving.Models.Objects
{
    /// <summary>
    /// A repeater section's field values on an e-form or Unity Form.
    /// </summary>
    public class RepeaterInfo
    {
        #region Properties
        /// <summary>
        /// Repeater ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Field values for this repeater row.
        /// </summary>
        public List<FieldInfo> Fields { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the RepeaterInfo class
        /// </summary>
        public RepeaterInfo()
        {
            Fields = [];
        }
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
