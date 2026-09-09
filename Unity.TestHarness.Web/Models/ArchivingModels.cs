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

namespace Unity.TestHarness.Web.Models
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of Unity.TestHarness (the WPF version)'s
     * KeywordEditorSet/KeywordGroupEditor/StandaloneKeywordEditor. Where the WPF version
     * builds bindable editor objects the UI edits directly, this is a plain SCHEMA the
     * view renders from (each group's field DEFINITIONS, plus its EXISTING instances'
     * values, if any) and the client-side JS drives Add/Remove Instance/Value
     * dynamically, generating new form fields on the fly. The actual submitted values
     * come back as flat form fields using a naming convention
     * (kw_group_{groupId}_{instanceKey}_{fieldId}, kw_standalone_{keywordId}_{valueKey}),
     * parsed server-side in ArchivingController rather than model-bound, since the shape
     * (how many instances, how many values) is only known at submit time.
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

        /// <summary>Delete (or purge) an existing document.</summary>
        Delete
    }

    /// <summary>
    /// A single Keyword Type's ID/name, used as a group instance's field definition.
    /// </summary>
    [Serializable]
    public class KeywordFieldSchema
    {
        /// <summary>Keyword Type ID.</summary>
        public long Id { get; set; }

        /// <summary>Keyword Type name.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// A named Keyword Group Type's schema: its field definitions, and any existing
    /// instances' values (empty for a brand-new document's Store New editor).
    /// </summary>
    [Serializable]
    public class KeywordGroupSchema
    {
        /// <summary>Keyword Group Type ID.</summary>
        public long Id { get; set; }

        /// <summary>Keyword Group Type name.</summary>
        public string Name { get; set; }

        /// <summary>Whether this group allows more than one instance.</summary>
        public bool MultiInstance { get; set; }

        /// <summary>This group's own Keyword Type field definitions.</summary>
        public List<KeywordFieldSchema> FieldDefinitions { get; set; } = new List<KeywordFieldSchema>();

        /// <summary>
        /// Existing instances' values (each a Keyword Type ID -> value map), if editing
        /// an existing document. Empty for Store New, where the view renders exactly one
        /// blank instance instead.
        /// </summary>
        public List<Dictionary<long, string>> Instances { get; set; } = new List<Dictionary<long, string>>();
    }

    /// <summary>
    /// A standalone Keyword Type's schema: its definition, and any existing values (empty
    /// for a brand-new document's Store New editor).
    /// </summary>
    [Serializable]
    public class StandaloneKeywordSchema
    {
        /// <summary>Keyword Type ID.</summary>
        public long Id { get; set; }

        /// <summary>Keyword Type name.</summary>
        public string Name { get; set; }

        /// <summary>
        /// Existing values, if editing an existing document. Empty for Store New, where
        /// the view renders exactly one blank value instead.
        /// </summary>
        public List<string> Values { get; set; } = new List<string>();
    }

    /// <summary>
    /// A Document Type (or existing document)'s full keyword-editing schema: named groups
    /// and standalone keywords.
    /// </summary>
    [Serializable]
    public class KeywordEditorSchema
    {
        /// <summary>Named (non-StandAlone) Keyword Group Type schemas.</summary>
        public List<KeywordGroupSchema> Groups { get; set; } = new List<KeywordGroupSchema>();

        /// <summary>Standalone Keyword Type schemas.</summary>
        public List<StandaloneKeywordSchema> Standalone { get; set; } = new List<StandaloneKeywordSchema>();
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
        public List<NamedItem> DocumentTypeGroups { get; set; } = new List<NamedItem>();

        /// <summary>Every Document Type.</summary>
        public List<NamedItem> AllDocumentTypes { get; set; } = new List<NamedItem>();

        /// <summary>The Document Type currently selected to store a new document as.</summary>
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
        public long LoadedDocumentId { get; set; }

        /// <summary>The loaded document's document type name (editable for Modify Metadata).</summary>
        public string LoadedDocumentTypeName { get; set; }

        /// <summary>The loaded document's document date (editable for Modify Metadata).</summary>
        public DateTime LoadedDocumentDate { get; set; } = DateTime.Today;

        /// <summary>Whether the loaded document's type allows new revisions.</summary>
        public bool IsRevisable { get; set; }

        /// <summary>Whether the loaded document's type allows new renditions.</summary>
        public bool IsRenditionable { get; set; }

        /// <summary>The loaded document's keyword editor schema, pre-populated with
        /// current values. Never null, matching NewEditorSchema's own convention.</summary>
        public KeywordEditorSchema ExistingEditorSchema { get; set; } = new KeywordEditorSchema();

        /// <summary>Whether Delete should permanently purge rather than a regular, recoverable delete.</summary>
        public bool PurgeDocument { get; set; }
    }
}
