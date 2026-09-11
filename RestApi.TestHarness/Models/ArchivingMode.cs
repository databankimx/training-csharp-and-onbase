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
     * *Migration Note: identical to Unity.TestHarness's own ArchivingMode, purely generic
     * UI infrastructure with no Unity API/REST API dependency at all.
     */
    #endregion

    /// <summary>
    /// Which of the five archiving operations the Archiving page is currently using.
    /// </summary>
    public enum ArchivingMode
    {
        /// <summary>
        /// Store a brand-new document.
        /// </summary>
        StoreNew,

        /// <summary>
        /// Modify an existing document's keywords/document type/date.
        /// </summary>
        ModifyMetadata,

        /// <summary>
        /// Add a new revision to an existing document.
        /// </summary>
        AddRevision,

        /// <summary>
        /// Add a new rendition to an existing document's latest revision.
        /// </summary>
        AddRendition,

        /// <summary>
        /// Delete an existing document.
        /// </summary>
        Delete
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
