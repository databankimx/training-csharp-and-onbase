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
#endregion

namespace Unity._03.DocumentRetrieval.Models.Objects
{
    #region Training Notes
    /*
     * *Migration Note: see RevisionInfo's own Training Notes for why this exists. Note
     * for whoever builds the web layer: FileTypeId is likely usable as a stable
     * "re-locate this rendition later" key alongside (DocumentId, RevisionId), since
     * DocumentStorage.UpdateRendition already rejects a second rendition of the same
     * file type on one revision, but that's an implementation detail this DTO doesn't
     * formally guarantee, a proper retrieval-by-ID design wasn't worked out in this pass.
     */
    #endregion

    /// <summary>
    /// Defines rendition metadata for an integration application, a serializable
    /// alternative to holding a live <see cref="Hyland.Unity.Rendition"/> reference.
    /// </summary>
    public class RenditionInfo
    {
        #region Properties
        /// <summary>
        /// File type name
        /// </summary>
        public string FileTypeName { get; set; }

        /// <summary>
        /// File type ID
        /// </summary>
        public long FileTypeId { get; set; }

        /// <summary>
        /// The file extension of this rendition's first page's relative path, use this
        /// (not FileTypeName) to name a retrieved file correctly, this is what actually
        /// distinguishes e.g. DOC from DOCX, not the bare file type.
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Number of pages in this rendition
        /// </summary>
        public long NumberOfPages { get; set; }

        /// <summary>
        /// Rendition comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Rendition creator
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Rendition creation date
        /// </summary>
        public DateTime CreationDate { get; set; }
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
