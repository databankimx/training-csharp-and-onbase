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
    /// An e-form or Unity Form's fields and repeaters.
    /// </summary>
    public class FormInfo
    {
        #region Properties
        /// <summary>
        /// Unity Form template ID. Only relevant for Unity Forms, ignored for e-forms.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Unity Form template name. Only relevant for Unity Forms, ignored for e-forms.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Repeater sections and their field values.
        /// </summary>
        public List<RepeaterInfo> Repeaters { get; set; }

        /// <summary>
        /// Field values.
        /// </summary>
        public List<FieldInfo> Fields { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the FormInfo class
        /// </summary>
        public FormInfo()
        {
            Repeaters = [];
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
