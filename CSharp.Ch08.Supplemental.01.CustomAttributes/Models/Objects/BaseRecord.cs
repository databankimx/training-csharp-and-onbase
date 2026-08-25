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
using CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Enumerations;
#endregion

namespace CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Objects
{
    /// <summary>
    /// A base record marked auditable. Both attributes are applied here using named-initializer
    /// syntax, and AuditLevel.Full demonstrates an enum-typed attribute property.
    /// </summary>
    [Auditable(Enabled = true, Level = AuditLevel.Full)]
    [ClassSpecific]
    public class BaseRecord
    {
        #region Properties
        /// <summary>
        /// Record ID
        /// </summary>
        public int Id { get; set; }
        #endregion
    }

    /// <summary>
    /// A subclass of BaseRecord that declares no attributes of its own. See LectureNotes.md
    /// for what reflection reports finding on this class, and why.
    /// </summary>
    public class DerivedRecord : BaseRecord
    {
        #region Properties
        /// <summary>
        /// Additional, derived-only property
        /// </summary>
        public string Note { get; set; }
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
