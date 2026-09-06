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

namespace Unity.TestHarness.Models
{
    /// <summary>
    /// Which of the three ways to find documents the Retrieval page is currently using.
    /// </summary>
    public enum SearchMode
    {
        /// <summary>
        /// Search by one or more Document Types (with keyword fields common to all
        /// selected types).
        /// </summary>
        DocumentType,

        /// <summary>
        /// Search by a Custom Query (with its own keyword fields).
        /// </summary>
        CustomQuery,

        /// <summary>
        /// Retrieve a single document directly by its Document ID.
        /// </summary>
        DocumentId
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
