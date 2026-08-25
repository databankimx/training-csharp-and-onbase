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

namespace CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Enumerations
{
    /// <summary>
    /// How thoroughly a class's changes should be audited
    /// </summary>
    public enum AuditLevel
    {
        /// <summary>
        /// No auditing at all, even if Enabled = true. This is a valid combination, meaning "auditing is enabled, but this class doesn't need it."
        /// </summary>
        None,

        /// <summary>
        /// Basic auditing, e.g. record that a change happened, who did it, and when
        /// </summary>
        Basic,

        /// <summary>
        /// Full auditing, e.g. record the before-and-after values of every property that changed
        /// </summary>
        Full
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
