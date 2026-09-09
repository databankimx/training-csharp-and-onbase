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
     * *Migration Note: serializable DTOs for the Taxonomy page's cascading, AJAX-driven
     * UI (Document Type Group -> Document Types -> Keyword Groups/Standalone Keywords ->
     * a group's own Keyword Types), a JSON equivalent of what TaxonomyViewModel binds
     * directly to Hyland.Unity types for in the WPF version. A web request/response
     * cycle can't hold onto a live Unity API object between an AJAX call and the next,
     * these are what actually travel over the wire.
     */
    #endregion

    /// <summary>
    /// A Document Type Group or Document Type, by name (used as the lookup key for both,
    /// per OnBaseTaxonomy's own name-or-ID convention) and display name.
    /// </summary>
    [Serializable]
    public class NamedItem
    {
        /// <summary>Item ID.</summary>
        public long Id { get; set; }

        /// <summary>Item name.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// A Keyword Type: ID, name, data type, and length.
    /// </summary>
    [Serializable]
    public class KeywordTypeItem
    {
        /// <summary>Keyword Type ID.</summary>
        public long Id { get; set; }

        /// <summary>Keyword Type name.</summary>
        public string Name { get; set; }

        /// <summary>Keyword Type's data type (e.g. "Text", "Date", "Number").</summary>
        public string DataType { get; set; }

        /// <summary>Keyword Type's maximum length.</summary>
        public long Length { get; set; }
    }

    /// <summary>
    /// A named Keyword Group Type (MultiInstance or SingleInstance).
    /// </summary>
    [Serializable]
    public class KeywordGroupItem
    {
        /// <summary>Keyword Group Type ID.</summary>
        public long Id { get; set; }

        /// <summary>Keyword Group Type name.</summary>
        public string Name { get; set; }

        /// <summary>Whether this group allows more than one instance.</summary>
        public bool MultiInstance { get; set; }
    }

    /// <summary>
    /// Response shape for the "load a Document Type's keyword groups/standalone
    /// keywords" AJAX endpoint.
    /// </summary>
    [Serializable]
    public class KeywordGroupsAndStandaloneResult
    {
        /// <summary>The Document Type's named (non-StandAlone) Keyword Group Types.</summary>
        public List<KeywordGroupItem> Groups { get; set; } = [];

        /// <summary>The Document Type's standalone Keyword Types.</summary>
        public List<KeywordTypeItem> Standalone { get; set; } = [];
    }

    /// <summary>
    /// A found File Type, or an indication that none was found.
    /// </summary>
    [Serializable]
    public class FileTypeLookupResult
    {
        /// <summary>Whether a File Type was found.</summary>
        public bool Found { get; set; }

        /// <summary>The found File Type's ID, if any.</summary>
        public long Id { get; set; }

        /// <summary>The found File Type's name, if any.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// A found Unity Form template, or an indication that none was found.
    /// </summary>
    [Serializable]
    public class UnityFormLookupResult
    {
        /// <summary>Whether a Unity Form template was found.</summary>
        public bool Found { get; set; }

        /// <summary>The found template's ID, if any.</summary>
        public long Id { get; set; }

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
