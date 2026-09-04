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

namespace Unity._04.DocumentArchiving.Models.Objects
{
    /// <summary>
    /// A single field on an e-form or Unity Form.
    /// </summary>
    public class FieldInfo
    {
        #region Properties
        /// <summary>
        /// Field ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Field name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Field value.
        /// </summary>
        public string Value { get; set; }
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
