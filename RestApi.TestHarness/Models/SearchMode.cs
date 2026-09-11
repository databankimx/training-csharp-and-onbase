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

namespace RestApi.TestHarness.Models
{
    #region Training Notes
    /*
     * *Migration Note: identical to Unity.TestHarness's own SearchMode. RestApi.03's own
     * RetrievalRequest.Scope supports a fourth option (DocumentTypeGroup, a genuinely new
     * REST-only search scope, see RestApi.03's own LectureNotes.md) that this app's own
     * UI deliberately does NOT surface, per the "identical application" goal for
     * RestApi.TestHarness itself, that richer capability lives in the library layer, not
     * necessarily in every app built on it.
     */
    #endregion

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
