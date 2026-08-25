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
using System;
using CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Enumerations;
#endregion

namespace CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Attributes
{
    /// <summary>
    /// Marks a class as subject to auditing, and records how thorough that auditing should be.
    /// Both properties here use C#'s named-initializer syntax when applied (rather than being
    /// set through a constructor), and Level is an enum, both worth seeing demonstrated. Also
    /// deliberately marked Inherited = true, so a subclass of an [Auditable] class is
    /// considered auditable too, without needing its own [Auditable] attribute, see
    /// NotInheritedAttribute for the contrasting case.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class AuditableAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Whether auditing is actually turned on for this class
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// How thorough the audit trail should be
        /// </summary>
        public AuditLevel Level { get; set; }
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
