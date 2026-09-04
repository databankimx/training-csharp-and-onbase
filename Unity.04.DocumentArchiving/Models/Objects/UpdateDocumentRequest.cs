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
using Unity._04.DocumentArchiving.Models.Enumerations;
#endregion

namespace Unity._04.DocumentArchiving.Models.Objects
{
    /// <summary>
    /// Parameters for updating an existing document, extending
    /// <see cref="NewDocumentRequest"/> with the target document ID and which kind of
    /// update to perform.
    /// </summary>
    public class UpdateDocumentRequest : NewDocumentRequest
    {
        #region Properties
        /// <summary>
        /// The ID of the document to update.
        /// </summary>
        public long DocumentId { get; set; }

        /// <summary>
        /// When <see langword="true"/>, existing values for keywords supplied on this
        /// request are overwritten rather than added alongside.
        /// </summary>
        public bool OverwriteKeywords { get; set; }

        /// <summary>
        /// The kind of update to perform.
        /// </summary>
        public UpdateType UpdateType { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the UpdateDocumentRequest class
        /// </summary>
        /// <param name="updateType">The kind of update to perform.</param>
        /// <param name="storageType">The kind of document being updated.</param>
        public UpdateDocumentRequest(UpdateType updateType = UpdateType.Metadata, StorageType storageType = StorageType.Document)
        {
            UpdateType = updateType;
            StorageType = storageType;
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
