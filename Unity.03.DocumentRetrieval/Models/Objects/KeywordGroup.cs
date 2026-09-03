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

namespace Unity._03.DocumentRetrieval.Models.Objects
{
    /// <summary>
    /// Defines an MIKG or SIKG for integration retrieval
    /// </summary>
    public class KeywordGroup
    {
        #region Properties
        /// <summary>
        /// Keyword group type id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Keyword group type name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if this is an MIKG, false if SIKG
        /// </summary>
        public bool MultiInstance { get; set; }

        /// <summary>
        /// Keywords in keyword group instance
        /// </summary>
        public List<KeywordInfo> Keywords { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the KeywordGroup class
        /// </summary>
        public KeywordGroup()
        {
            Keywords = [];
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
