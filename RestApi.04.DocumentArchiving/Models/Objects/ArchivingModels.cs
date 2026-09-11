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
using RestApi._03.DocumentRetrieval.Models.Objects;
using RestApi._04.DocumentArchiving.Models.Enumerations;
#endregion

namespace RestApi._04.DocumentArchiving.Models.Objects
{
    #region Training Notes
    /*
     * *Migration Note: reuses RestApi.03.DocumentRetrieval's own KeywordInfo/KeywordGroup
     * directly, the same way Unity.04 reuses Unity.03's, rather than duplicating them
     * here. No StorageRequest base class the way Unity.04's NewDocumentRequest/
     * UpdateDocumentRequest both derive from one (via StorageType) - not needed, this
     * project is scoped to conventional documents only, see this project's own .csproj
     * Training Notes and LectureNotes.md.
     *
     * DeleteRequest has no PurgeDocument property: confirmed directly against
     * document-api.json, DELETE /documents/{documentId} takes no body/query parameters
     * beyond the id itself, "purge" does not appear anywhere in the spec at all. This is
     * a genuine feature gap, not an oversight, RestApi.TestHarness's Archiving page will
     * need to omit the Purge checkbox entirely.
     *
     * Files (on NewDocumentRequest/UpdateDocumentRequest) are still local file paths,
     * matching Unity API's own NewDocumentRequest.Files/UpdateDocumentRequest.Files
     * shape, even though the REST API itself needs them staged through a separate
     * three-step upload process first (see DocumentStorage's own Training Notes): the
     * request MODEL still just takes paths, the staging happens internally, so this
     * class's public shape doesn't leak REST-specific upload mechanics to callers.
     */
    #endregion

    /// <summary>A request to store a new document.</summary>
    public class NewDocumentRequest
    {
        /// <summary>The Document Type id or name to store the document as.</summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// The File Type id for the document, or null to use the Document Type's own
        /// default File Type (resolved via GetDefaultUploadFileTypeAsync if not supplied).
        /// </summary>
        public string FileTypeId { get; set; }

        /// <summary>The new document's document date.</summary>
        public DateTime DocumentDate { get; set; } = DateTime.Today;

        /// <summary>Local file path(s) to upload, in page order.</summary>
        public List<string> Files { get; set; } = [];

        /// <summary>Standalone keywords.</summary>
        public List<KeywordInfo> Keywords { get; set; } = [];

        /// <summary>Named (Single-/Multi-Instance) Keyword Type Group instances.</summary>
        public List<KeywordGroup> KeywordGroups { get; set; } = [];

        /// <summary>
        /// Forces the document to be stored as new, regardless of the Document Type's own
        /// Revisable/Renditionable settings (which would otherwise trigger a 300 Multiple
        /// Choices response if existing matching documents are found; see this project's
        /// own DocumentStorage for how that's currently handled).
        /// </summary>
        public bool StoreAsNew { get; set; }

        /// <summary>Comment, used if the Document Type is Revisable/Renditionable.</summary>
        public string Comment { get; set; }
    }

    /// <summary>A request to update an existing document: its metadata, a new revision, or a new rendition.</summary>
    public class UpdateDocumentRequest
    {
        /// <summary>Which kind of update this is.</summary>
        public UpdateType UpdateType { get; set; }

        /// <summary>The document id to update.</summary>
        public string DocumentId { get; set; }

        /// <summary>The Document Type id or name to reindex into, for UpdateType.Metadata.</summary>
        public string DocumentType { get; set; }

        /// <summary>The document date, for UpdateType.Metadata.</summary>
        public DateTime DocumentDate { get; set; } = DateTime.Today;

        /// <summary>
        /// The File Type id for a new revision/rendition, or null to use the Document
        /// Type's own default.
        /// </summary>
        public string FileTypeId { get; set; }

        /// <summary>Local file path(s) to upload, for UpdateType.Revision/Rendition.</summary>
        public List<string> Files { get; set; } = [];

        /// <summary>Standalone keywords, for UpdateType.Metadata.</summary>
        public List<KeywordInfo> Keywords { get; set; } = [];

        /// <summary>Named Keyword Type Group instances, for UpdateType.Metadata.</summary>
        public List<KeywordGroup> KeywordGroups { get; set; } = [];

        /// <summary>Comment, for UpdateType.Revision/Rendition.</summary>
        public string Comment { get; set; }
    }

    /// <summary>A request to delete a document.</summary>
    public class DeleteRequest
    {
        /// <summary>The document id to delete.</summary>
        public string DocumentId { get; set; }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
