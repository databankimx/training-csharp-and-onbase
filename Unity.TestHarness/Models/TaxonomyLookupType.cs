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

namespace Unity.TestHarness.Models
{
    /// <summary>
    /// Which kind of OnBase taxonomy object the Taxonomy page is currently browsing.
    /// </summary>
    public enum TaxonomyLookupType
    {
        /// <summary>
        /// Document types (OnBaseTaxonomy.GetDocumentType/GetDocumentTypes).
        /// </summary>
        DocumentType,

        /// <summary>
        /// Document type groups (OnBaseTaxonomy.GetDocumentTypeGroup/GetDocumentTypeGroups).
        /// </summary>
        DocumentTypeGroup,

        /// <summary>
        /// Keyword types, optionally scoped to a document type
        /// (OnBaseTaxonomy.GetKeywordType/GetKeywordTypes).
        /// </summary>
        KeywordType,

        /// <summary>
        /// Keyword group (record) types, optionally scoped to a document type
        /// (OnBaseTaxonomy.GetKeywordGroupType/GetKeywordGroupTypes).
        /// </summary>
        KeywordGroupType,

        /// <summary>
        /// Custom queries (OnBaseTaxonomy.GetCustomQuery/GetCustomQueries).
        /// </summary>
        CustomQuery,

        /// <summary>
        /// File types, by extension or ID (OnBaseTaxonomy.GetFileType). No "list all"
        /// equivalent exists for this type.
        /// </summary>
        FileType,

        /// <summary>
        /// Unity Form templates (OnBaseTaxonomy.GetUnityForm). No "list all" equivalent
        /// exists for this type.
        /// </summary>
        UnityForm
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
