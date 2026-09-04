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

namespace Unity._04.DocumentArchiving.Models.Objects
{
    /// <summary>
    /// Parameters for deleting or purging an existing document.
    /// </summary>
    public class DeleteRequest
    {
        #region Properties
        /// <summary>
        /// The ID of the document to delete.
        /// </summary>
        public long DocumentId { get; set; }

        /// <summary>
        /// When <see langword="true"/>, permanently purges the document instead of a
        /// recoverable delete.
        /// </summary>
        public bool PurgeDocument { get; set; }
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
