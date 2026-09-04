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
using System.Collections.Generic;
using Unity._03.DocumentRetrieval.Models.Objects;
#endregion

namespace Unity._04.DocumentArchiving.Models.Objects
{
    /// <summary>
    /// Base class for storage/update request parameters common to new documents,
    /// e-forms, and Unity Forms.
    /// </summary>
    public class StorageRequest
    {
        #region Properties
        /// <summary>
        /// The document type name or ID to store as.
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// The document date. Defaults to today.
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// Keyword groups (records) to apply.
        /// </summary>
        public List<KeywordGroup> KeywordGroups { get; set; }

        /// <summary>
        /// Stand-alone keywords to apply.
        /// </summary>
        public List<KeywordInfo> Keywords { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the StorageRequest class
        /// </summary>
        public StorageRequest()
        {
            KeywordGroups = [];
            Keywords = [];
            DocumentDate = DateTime.Today;
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
