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
using System.Collections.Generic;
#endregion

namespace RestApi.TestHarness.Web.Models
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * TaxonomyModels. Id fields are strings throughout, matching RestApi.02's own id
     * typing (not Unity API's long). No Length on KeywordTypeItem: RestApi.02's own
     * KeywordType doesn't expose a data-length field at all (document-api.json's
     * KeywordType schema has none), so there's nothing to carry through here either,
     * same gap already documented for RestApi.TestHarness's own SearchKeywordField.
     *
     * KeywordGroupItem now carries its own KeywordTypes directly, eliminating
     * Unity.TestHarness.Web's own third AJAX endpoint (GetGroupKeywordTypes) entirely:
     * RestApi.02's own GetDocumentTypeKeywordGroupsAsync already fully resolves every
     * group's KeywordTypes up front (see that project's own Training Notes for why), so
     * GetKeywordGroupsAndStandalone can just include them directly rather than requiring
     * a follow-up round trip once the user selects a specific group.
     */
    #endregion

    /// <summary>
    /// A Document Type Group, Document Type, or Custom Query, by ID and display name.
    /// </summary>
    public class NamedItem
    {
        /// <summary>Item ID.</summary>
        public string Id { get; set; }

        /// <summary>Item name.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// A Keyword Type: ID, name, and data type.
    /// </summary>
    public class KeywordTypeItem
    {
        /// <summary>Keyword Type ID.</summary>
        public string Id { get; set; }

        /// <summary>Keyword Type name.</summary>
        public string Name { get; set; }

        /// <summary>Keyword Type's data type (e.g. "Numeric9", "Alphanumeric", "Date").</summary>
        public string DataType { get; set; }
    }

    /// <summary>
    /// A named Keyword Group Type (Multi-Instance or Single-Instance), with its own
    /// Keyword Types already resolved (see this class's own Training Notes).
    /// </summary>
    public class KeywordGroupItem
    {
        /// <summary>Keyword Group Type ID.</summary>
        public string Id { get; set; }

        /// <summary>Keyword Group Type name.</summary>
        public string Name { get; set; }

        /// <summary>Whether this group allows more than one instance.</summary>
        public bool MultiInstance { get; set; }

        /// <summary>This group's own Keyword Types.</summary>
        public List<KeywordTypeItem> KeywordTypes { get; set; } = [];
    }

    /// <summary>
    /// Response shape for the "load a Document Type's keyword groups/standalone
    /// keywords" AJAX endpoint.
    /// </summary>
    public class KeywordGroupsAndStandaloneResult
    {
        /// <summary>The Document Type's named (non-Standalone) Keyword Group Types.</summary>
        public List<KeywordGroupItem> Groups { get; set; } = [];

        /// <summary>The Document Type's standalone Keyword Types.</summary>
        public List<KeywordTypeItem> Standalone { get; set; } = [];
    }

    /// <summary>
    /// A found File Type, or an indication that none was found.
    /// </summary>
    public class FileTypeLookupResult
    {
        /// <summary>Whether a File Type was found.</summary>
        public bool Found { get; set; }

        /// <summary>The found File Type's ID, if any.</summary>
        public string Id { get; set; }

        /// <summary>The found File Type's name, if any.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// A found Unity Form template, or an indication that none was found.
    /// </summary>
    public class UnityFormLookupResult
    {
        /// <summary>Whether a Unity Form template was found.</summary>
        public bool Found { get; set; }

        /// <summary>The found template's ID, if any.</summary>
        public string Id { get; set; }

        /// <summary>The found template's name, if any.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// View-facing data for the Taxonomy page's top-level load: Document Type Groups and
    /// Custom Queries. Everything below Document Type Group is loaded on demand via AJAX
    /// as the user drills in, rather than fetched all at once.
    /// </summary>
    public class TaxonomyPageModel
    {
        /// <summary>Whether taxonomy data has been loaded yet this session.</summary>
        public bool IsLoaded { get; set; }

        /// <summary>Every Document Type Group.</summary>
        public List<NamedItem> DocumentTypeGroups { get; set; } = [];

        /// <summary>Every Custom Query.</summary>
        public List<NamedItem> CustomQueries { get; set; } = [];
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
