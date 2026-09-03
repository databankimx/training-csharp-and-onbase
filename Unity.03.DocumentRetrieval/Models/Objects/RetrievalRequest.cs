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
    /// Defines a request object to pass document query filters
    /// </summary>
    public class RetrievalRequest
    {
        #region Properties
        /// <summary>
        /// Document type name to search
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// Custom query name to search
        /// </summary>
        public string CustomQuery { get; set; }

        /// <summary>
        /// TO and FROM dates to search
        /// </summary>
        public DateRange DateRange { get; set; }

        /// <summary>
        /// Keyword groups to search
        /// </summary>
        public List<KeywordGroup> KeywordGroups { get; set; }

        /// <summary>
        /// Keywords to search
        /// </summary>
        public List<KeywordInfo> Keywords { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the RetrievalRequest class
        /// </summary>
        public RetrievalRequest()
        {
            DateRange = new DateRange();
            KeywordGroups = [];
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
