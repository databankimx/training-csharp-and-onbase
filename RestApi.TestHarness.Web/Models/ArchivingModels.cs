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

namespace RestApi.TestHarness.Web.Models
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * ArchivingModels. Same overall design (a plain schema the view renders from, flat
     * kw_group_{typeGroupId}_{instanceKey}_{fieldId}/kw_standalone_{keywordId}_{valueKey}
     * form fields parsed back server-side in ArchivingController, since the shape is only
     * known at submit time), but:
     *
     * - Every id is a string, matching RestApi.02/03/04's own id typing (not Unity API's
     *   long).
     *
     * - KeywordGroupSchema.Id represents the Keyword Type GROUP id (RestApi.03's own
     *   KeywordGroup.TypeGroupId, "which KIND of group this is"), not a specific
     *   instance's id: the instance identity itself (GroupId, "which INSTANCE this is")
     *   is never tracked here at all, matching how Unity's own version never tracked
     *   per-instance identity either, RestApi.04's own UpdateMetadataAsync always fully
     *   replaces a document's keyword collection from what's submitted (there's no
     *   OverwriteKeywords toggle needed the way Unity's own UpdateDocumentRequest
     *   exposed, RestApi.04's own request shape makes "replace everything" implicit, see
     *   DocumentStorage's own Training Notes), so a fresh instance list with GroupId left
     *   null on every entry is exactly the right payload either way.
     *
     * - No PurgeDocument on ArchivingPageModel: a confirmed gap, see RestApi.04's own
     *   ArchivingModels Training Notes (DeleteRequest has no PurgeDocument field at all).
     *
     * - No IsRevisable/IsRenditionable pre-flight flags: also a confirmed gap (see
     *   DocumentStorage's own Training Notes), Add Revision/Add Rendition are always
     *   offered once a document is loaded.
     */
    #endregion

    /// <summary>
    /// Which of the five archiving operations the Archiving page is currently using.
    /// </summary>
    public enum ArchivingMode
    {
        /// <summary>Store a brand-new document.</summary>
        StoreNew,

        /// <summary>Modify an existing document's keywords/document type/date.</summary>
        ModifyMetadata,

        /// <summary>Add a new revision to an existing document.</summary>
        AddRevision,

        /// <summary>Add a new rendition to an existing document's latest revision.</summary>
        AddRendition,

        /// <summary>Delete an existing document.</summary>
        Delete
    }

    /// <summary>
    /// A single Keyword Type's ID/name, used as a group instance's field definition.
    /// </summary>
    public class KeywordFieldSchema
    {
        /// <summary>Keyword Type ID.</summary>
        public string Id { get; set; }

        /// <summary>Keyword Type name.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// A named Keyword Group Type's schema: its field definitions, and any existing
    /// instances' values (empty for a brand-new document's Store New editor).
    /// </summary>
    public class KeywordGroupSchema
    {
        /// <summary>Keyword Type Group ID (which KIND of group this is, see this class's own Training Notes).</summary>
        public string Id { get; set; }

        /// <summary>Keyword Group Type name.</summary>
        public string Name { get; set; }

        /// <summary>Whether this group allows more than one instance.</summary>
        public bool MultiInstance { get; set; }

        /// <summary>This group's own Keyword Type field definitions.</summary>
        public List<KeywordFieldSchema> FieldDefinitions { get; set; } = [];

        /// <summary>
        /// Existing instances' values (each a Keyword Type ID -> value map), if editing
        /// an existing document. Empty for Store New, where the view renders exactly one
        /// blank instance instead.
        /// </summary>
        public List<Dictionary<string, string>> Instances { get; set; } = [];
    }

    /// <summary>
    /// A standalone Keyword Type's schema: its definition, and any existing values (empty
    /// for a brand-new document's Store New editor).
    /// </summary>
    public class StandaloneKeywordSchema
    {
        /// <summary>Keyword Type ID.</summary>
        public string Id { get; set; }

        /// <summary>Keyword Type name.</summary>
        public string Name { get; set; }

        /// <summary>
        /// Existing values, if editing an existing document. Empty for Store New, where
        /// the view renders exactly one blank value instead.
        /// </summary>
        public List<string> Values { get; set; } = [];
    }

    /// <summary>
    /// A Document Type (or existing document)'s full keyword-editing schema: named groups
    /// and standalone keywords.
    /// </summary>
    public class KeywordEditorSchema
    {
        /// <summary>Named (non-StandAlone) Keyword Group Type schemas.</summary>
        public List<KeywordGroupSchema> Groups { get; set; } = [];

        /// <summary>Standalone Keyword Type schemas.</summary>
        public List<StandaloneKeywordSchema> Standalone { get; set; } = [];
    }

    /// <summary>
    /// View-facing data for the Archiving page, held per-session.
    /// </summary>
    public class ArchivingPageModel
    {
        /// <summary>Which archiving operation is currently active.</summary>
        public ArchivingMode Mode { get; set; } = ArchivingMode.StoreNew;

        // --- Store New ---

        /// <summary>Whether taxonomy data has been loaded yet this session.</summary>
        public bool IsTaxonomyLoaded { get; set; }

        /// <summary>Every Document Type Group, for the group filter.</summary>
        public List<NamedItem> DocumentTypeGroups { get; set; } = [];

        /// <summary>Every Document Type.</summary>
        public List<NamedItem> AllDocumentTypes { get; set; } = [];

        /// <summary>The Document Type currently selected to store a new document as.</summary>
        public string SelectedDocumentTypeId { get; set; }

        /// <summary>The selected Document Type's own name, for display.</summary>
        public string SelectedDocumentTypeName { get; set; }

        /// <summary>The selected Document Type's keyword editor schema. Never null (see
        /// RetrievalPageModel.Detail's own Training Note for why), an empty instance
        /// represents "nothing selected yet."</summary>
        public KeywordEditorSchema NewEditorSchema { get; set; } = new KeywordEditorSchema();

        // --- Existing document (Modify/AddRevision/AddRendition/Delete) ---

        /// <summary>The Document ID last entered to load for editing.</summary>
        public string DocumentIdInput { get; set; }

        /// <summary>Whether a document is currently loaded for editing.</summary>
        public bool HasLoadedDocument { get; set; }

        /// <summary>The loaded document's ID.</summary>
        public string LoadedDocumentId { get; set; }

        /// <summary>The loaded document's document type name (editable for Modify Metadata).</summary>
        public string LoadedDocumentTypeName { get; set; }

        /// <summary>The loaded document's document date (editable for Modify Metadata).</summary>
        public DateTime LoadedDocumentDate { get; set; } = DateTime.Today;

        /// <summary>The loaded document's keyword editor schema, pre-populated with
        /// current values. Never null, matching NewEditorSchema's own convention.</summary>
        public KeywordEditorSchema ExistingEditorSchema { get; set; } = new KeywordEditorSchema();
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
