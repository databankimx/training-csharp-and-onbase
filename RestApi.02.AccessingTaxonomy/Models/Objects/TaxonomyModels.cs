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

namespace RestApi._02.AccessingTaxonomy.Models.Objects
{
    #region Training Notes
    /*
     * *Migration Note: plain POCOs matching document-api.json's own schemas
     * (DocumentTypeGroup, DocumentType, KeywordTypeGroup, KeywordType, CustomQuery,
     * FileType), the REST-API counterpart to Unity.02's direct use of Hyland.Unity's own
     * DocumentTypeGroup/DocumentType/KeywordRecordType/KeywordType/CustomQuery/FileType
     * classes. One real, consistent difference worth flagging: every "id" in the REST API
     * is a STRING, not Unity API's long. Deserialized case-insensitively
     * (JsonSerializerOptions.PropertyNameCaseInsensitive, set once in OnBaseTaxonomy)
     * rather than [JsonPropertyName] on every property, since every field here maps
     * cleanly by name modulo casing (id -> Id, systemName -> SystemName, etc.).
     *
     * DocumentTypeKeywordGroup is NOT a direct schema from document-api.json, it's this
     * project's own resolved combination of two separate calls
     * (GET .../keyword-type-groups, which gives only group id + each keyword type's bare
     * id, and GET /keyword-types, which gives the full metadata for those ids), the same
     * "up to four calls to fully render keywords" resolution the Working with Keywords
     * guide describes. OnBaseTaxonomy.GetDocumentTypeKeywordGroupsAsync does that
     * resolution once, so callers get a single, fully-populated result, the same
     * convenience Unity API's own docType.KeywordRecordTypes property gives for free. A
     * null Id here means Standalone (the group has no id in the API response at all,
     * this is confirmed directly against the KeywordTypeGroupOnDocumentType schema), not
     * a missing/unset value.
     */
    #endregion

    /// <summary>Document Type Group metadata.</summary>
    public class DocumentTypeGroup
    {
        /// <summary>The unique identifier of the document type group.</summary>
        public string Id { get; set; }

        /// <summary>The localized name of the document type group.</summary>
        public string Name { get; set; }

        /// <summary>The untranslated system name of the document type group.</summary>
        public string SystemName { get; set; }
    }

    /// <summary>Document Type metadata.</summary>
    public class DocumentType
    {
        /// <summary>The unique identifier of the document type.</summary>
        public string Id { get; set; }

        /// <summary>The localized name of the document type.</summary>
        public string Name { get; set; }

        /// <summary>The untranslated system name of the document type.</summary>
        public string SystemName { get; set; }

        /// <summary>The unique identifier of the default file format for the document type.</summary>
        public string DefaultFileTypeId { get; set; }

        /// <summary>The id of the document type group the document type is assigned to.</summary>
        public string DocumentTypeGroupId { get; set; }

        /// <summary>The Id of the autofill keyword set associated with this document type, if any.</summary>
        public string AutofillKeywordSetId { get; set; }
    }

    /// <summary>Keyword Type Group metadata (flat lookup; see <see cref="DocumentTypeKeywordGroup"/> for the per-Document-Type resolved shape).</summary>
    public class KeywordTypeGroup
    {
        /// <summary>The unique identifier of the keyword type group.</summary>
        public string Id { get; set; }

        /// <summary>The localized name of the keyword type group.</summary>
        public string Name { get; set; }

        /// <summary>The untranslated system name of the keyword type group.</summary>
        public string SystemName { get; set; }

        /// <summary>"SingleInstance" or "MultiInstance". See <see cref="DocumentTypeKeywordGroup"/> for how Standalone is represented.</summary>
        public string StorageType { get; set; }
    }

    /// <summary>Keyword Type metadata.</summary>
    public class KeywordType
    {
        /// <summary>The unique identifier of the keyword type.</summary>
        public string Id { get; set; }

        /// <summary>The localized name of the keyword type.</summary>
        public string Name { get; set; }

        /// <summary>The untranslated system name of the keyword type.</summary>
        public string SystemName { get; set; }

        /// <summary>"Numeric9", "Numeric20", "Alphanumeric", "Currency", "SpecificCurrency", "Date", "DateTime", or "FloatingPoint".</summary>
        public string DataType { get; set; }

        /// <summary>Whether the keyword type is used for document retrieval.</summary>
        public bool UsedForRetrieval { get; set; }

        /// <summary>Whether the keyword type is configured for security masking.</summary>
        public bool IsSecurityMasked { get; set; }
    }

    /// <summary>
    /// A Document Type's Keyword Group Type, resolved with its full Keyword Type
    /// metadata. Named (non-Standalone) groups have an <see cref="Id"/>; Standalone has
    /// none (matching the API's own representation, see this file's Training Notes).
    /// </summary>
    public class DocumentTypeKeywordGroup
    {
        /// <summary>The Keyword Type Group's ID, or null for the Standalone group.</summary>
        public string Id { get; set; }

        /// <summary>The Keyword Type Group's name, or null for the Standalone group.</summary>
        public string Name { get; set; }

        /// <summary>"SingleInstance" or "MultiInstance", or null for the Standalone group.</summary>
        public string StorageType { get; set; }

        /// <summary>This group's fully-resolved Keyword Types.</summary>
        public List<KeywordType> KeywordTypes { get; set; } = [];
    }

    /// <summary>Custom Query metadata.</summary>
    public class CustomQuery
    {
        /// <summary>The unique identifier of the custom query.</summary>
        public string Id { get; set; }

        /// <summary>The localized name of the custom query.</summary>
        public string Name { get; set; }

        /// <summary>The untranslated system name of the custom query.</summary>
        public string SystemName { get; set; }

        /// <summary>Information describing the usage and/or purpose of the custom query.</summary>
        public string Instructions { get; set; }

        /// <summary>"DocumentType", "DocumentTypeGroup", "Keyword", or "SQL".</summary>
        public string QueryType { get; set; }
    }

    /// <summary>File Type metadata.</summary>
    public class FileType
    {
        /// <summary>The unique identifier of the file type.</summary>
        public string Id { get; set; }

        /// <summary>The localized name of the file type.</summary>
        public string Name { get; set; }

        /// <summary>The untranslated system name of the file type.</summary>
        public string SystemName { get; set; }
    }

    /// <summary>
    /// Unity Form Template metadata, from the separate Forms API (see OnBaseTaxonomy's
    /// own Training Notes on the Unity Form methods for why this is a distinct API/base
    /// URL, not another Document Management API endpoint). Deliberately does not model
    /// formFieldDefinitions (a polymorphic hierarchy of Calculated/NestedTable/Repeater/ValueField
    /// definitions) — out of scope for a name/id lookup panel; RestApi.TestHarness's
    /// Taxonomy page only needs this template-level metadata, the same level of detail
    /// Unity.TestHarness's own Unity Form lookup shows.
    /// </summary>
    public class UnityFormTemplate
    {
        /// <summary>The unique identifier of the Unity Form template.</summary>
        public string Id { get; set; }

        /// <summary>The localized name of the Unity Form template.</summary>
        public string Name { get; set; }

        /// <summary>The untranslated system name of the Unity Form template.</summary>
        public string SystemName { get; set; }

        /// <summary>"Html" or "Image".</summary>
        public string Type { get; set; }

        /// <summary>Whether the template is creatable by the logged-in user.</summary>
        public bool Creatable { get; set; }

        /// <summary>The unique identifier of the Document Type this template is associated with.</summary>
        public string DocumentTypeId { get; set; }

        /// <summary>The revision number, for display/ordering purposes.</summary>
        public int RevisionNumber { get; set; }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
