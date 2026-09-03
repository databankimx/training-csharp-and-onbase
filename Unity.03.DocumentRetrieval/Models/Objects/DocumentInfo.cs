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
#endregion

namespace Unity._03.DocumentRetrieval.Models.Objects
{
    /// <summary>
    /// Defines document metadata (including keywords) for an integration application
    /// </summary>
    public class DocumentInfo
    {
        #region Properties
        /// <summary>
        /// Document ID
        /// </summary>
        public long Handle { get; set; }

        /// <summary>
        /// Document auto-name string
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Document type name
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Document Date
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// Document archived date
        /// </summary>
        public DateTime DateStored { get; set; }

        /// <summary>
        /// Document MIKG and SIKG instances
        /// </summary>
        public List<KeywordGroup> KeywordGroups { get; set; }

        /// <summary>
        /// Document keywords
        /// </summary>
        public List<KeywordInfo> Keywords { get; set; }

        /// <summary>
        /// Document date as MM-dd-yyyy
        /// </summary>
        public string DocDateString => DocumentDate.ToString("d");

        /// <summary>
        /// Document archived date as MM-dd-yyyy
        /// </summary>
        public string DateStoredString => DateStored.ToString("d");
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the DocumentInfo class
        /// </summary>
        public DocumentInfo()
        {
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
