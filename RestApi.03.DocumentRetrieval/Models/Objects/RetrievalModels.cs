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

namespace RestApi._03.DocumentRetrieval.Models.Objects
{
    #region Training Notes
    /*
     * *Migration Note: this file's DTOs mostly mirror Unity.03.DocumentRetrieval's own
     * shapes closely, string ids instead of Unity's long (matching RestApi.02's own
     * convention), no DocumentLink (DocPop/UnityPop, a confirmed feature gap, see
     * RestApi.00's ServiceLocation Training Notes).
     *
     * RetrievalRequest is the deliberate exception, genuinely reshaped, not just ported,
     * per an explicit decision to expose the REST API's real query capability rather than
     * cap it to Unity's simpler shape:
     *
     * - Scope replaces DocumentType/DocumentTypes/CustomQuery as separate fields with one
     *   QueryScope enum + an Ids list, matching document-api.json's own QueryType schema
     *   directly (type: CustomQuery/DocumentType/DocumentTypeGroup, ids: string[]).
     *   DocumentTypeGroup is a genuinely NEW search scope Unity API's own RetrievalRequest
     *   never exposed at this layer.
     *
     * - DateRanges (plural) replaces the single DateRange: the REST API's own
     *   documentDateRangeCollection supports multiple OR'd ranges per query, Unity's
     *   AddDateRange only ever took one.
     *
     * - Keywords now carry Operator/Relation (Equal/LessThan/GreaterThan/LessThanEqual/
     *   GreaterThanEqual/NotEqual/Literal, and And/Or/To respectively), matching
     *   document-api.json's own QueryKeyword schema. Unity's flat KeywordInfo had neither,
     *   every keyword was implicitly "Equal, AND'd with everything else."
     *
     * - No KeywordGroups (MultiInstance-specific query records): the REST API's
     *   queryKeywordCollection is flat, a keyword type's group membership doesn't affect
     *   how it's queried, unlike Unity's own AddQueryKeywordRecord vs AddKeyword split.
     *   Simpler here, not a gap, there's nothing this drops that the REST query model
     *   actually needs to express group membership for.
     *
     * - MaxResults is new: document-api.json's own QueryInformation.maxResults, no Unity
     *   API equivalent was exposed at this layer either (ExecuteQueryResults took a
     *   maxDocuments parameter directly, RetrievalRequest itself never carried it).
     */
    #endregion

    /// <summary>Which kind of REST API query to run. See document-api.json's own QueryType schema.</summary>
    public enum QueryScope
    {
        /// <summary>Search within one or more Document Types.</summary>
        DocumentType,

        /// <summary>Search within one or more Document Type Groups.</summary>
        DocumentTypeGroup,

        /// <summary>Run a single named Custom Query (only one id is supported for this scope).</summary>
        CustomQuery
    }

    /// <summary>A single OR'd date range within a query's <see cref="RetrievalRequest.DateRanges"/>.</summary>
    public class QueryDateRange
    {
        /// <summary>The range's start date, or null for the API's own default minimum.</summary>
        public DateTime? Start { get; set; }

        /// <summary>The range's end date, or null for the API's own default maximum.</summary>
        public DateTime? End { get; set; }
    }

    /// <summary>A single keyword filter within a query, with its own operator/relation.</summary>
    public class QueryKeyword
    {
        /// <summary>The Keyword Type id to filter on.</summary>
        public string TypeId { get; set; }

        /// <summary>The value to compare against.</summary>
        public string Value { get; set; }

        /// <summary>"Equal" (default), "LessThan", "GreaterThan", "LessThanEqual", "GreaterThanEqual", "NotEqual", or "Literal".</summary>
        public string Operator { get; set; } = "Equal";

        /// <summary>"And" (default), "Or", or "To" (this keyword's relation to the others in the same query).</summary>
        public string Relation { get; set; } = "And";
    }

    /// <summary>A request to search for documents.</summary>
    public class RetrievalRequest
    {
        /// <summary>Which kind of query to run.</summary>
        public QueryScope Scope { get; set; } = QueryScope.DocumentType;

        /// <summary>
        /// The ids to search: Document Type ids, Document Type Group ids, or (for
        /// <see cref="QueryScope.CustomQuery"/>) exactly one Custom Query id.
        /// </summary>
        public List<string> Ids { get; set; } = [];

        /// <summary>Limits the number of results returned, or null for no explicit limit.</summary>
        public int? MaxResults { get; set; }

        /// <summary>One or more OR'd date ranges to filter by.</summary>
        public List<QueryDateRange> DateRanges { get; set; } = [];

        /// <summary>Keyword filters, each with its own operator/relation.</summary>
        public List<QueryKeyword> Keywords { get; set; } = [];
    }

    /// <summary>A single keyword's metadata/value.</summary>
    public class KeywordInfo
    {
        /// <summary>Keyword Type id.</summary>
        public string Id { get; set; }

        /// <summary>Keyword Type name.</summary>
        public string Name { get; set; }

        /// <summary>Keyword value.</summary>
        public string Value { get; set; }
    }

    /// <summary>A named Keyword Type Group instance (Single- or Multi-Instance) with its keyword values.</summary>
    public class KeywordGroup
    {
        /// <summary>
        /// The Keyword Type Group id ("typeGroupId" in the REST API itself) — which KIND
        /// of group this is, shared by every instance of it. Always present for a named
        /// group. *Correction*: an earlier version of this file collapsed this and
        /// <see cref="GroupId"/> into one Id property, which only ever captured GroupId,
        /// leaving Single-Instance groups (which have a TypeGroupId but no GroupId at
        /// all) with no group identity captured whatsoever. Fixed to track both
        /// separately, matching the API's own two-field distinction.
        /// </summary>
        public string TypeGroupId { get; set; }

        /// <summary>
        /// The Keyword Type Group INSTANCE id ("groupId" in the REST API itself) — which
        /// specific instance this is, only present for Multi-Instance groups (a
        /// Single-Instance group has exactly one instance, with no separate instance id
        /// of its own). Null for a NEW instance being submitted for archival (the server
        /// assigns one), and null for every Single-Instance group's own single instance.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>Keyword Type Group name.</summary>
        public string Name { get; set; }

        /// <summary>True if this group allows more than one instance (equivalent to "GroupId can be non-null").</summary>
        public bool MultiInstance { get; set; }

        /// <summary>This instance's keyword values.</summary>
        public List<KeywordInfo> Keywords { get; set; } = [];
    }

    /// <summary>Document metadata (including keywords), from a search result or a single document lookup.</summary>
    public class DocumentInfo
    {
        /// <summary>Document id.</summary>
        public string Handle { get; set; }

        /// <summary>Document auto-name string.</summary>
        public string Name { get; set; }

        /// <summary>Document Type name.</summary>
        public string Type { get; set; }

        /// <summary>Document date.</summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>Date the document was archived.</summary>
        public DateTime DateStored { get; set; }

        /// <summary>Named (Single-/Multi-Instance) Keyword Type Group instances.</summary>
        public List<KeywordGroup> KeywordGroups { get; set; } = [];

        /// <summary>Standalone keywords.</summary>
        public List<KeywordInfo> Keywords { get; set; } = [];

        /// <summary>Document date, formatted for display.</summary>
        public string DocDateString => DocumentDate.ToString("d");

        /// <summary>Archived date, formatted for display.</summary>
        public string DateStoredString => DateStored.ToString("d");
    }

    /// <summary>Rendition metadata.</summary>
    public class RenditionInfo
    {
        /// <summary>File Type id.</summary>
        public string FileTypeId { get; set; }

        /// <summary>File Type name, resolved separately (not part of the REST API's own Rendition schema); may be null if not resolved.</summary>
        public string FileTypeName { get; set; }

        /// <summary>Number of pages in this rendition.</summary>
        public int PageCount { get; set; }

        /// <summary>The date the rendition was stored.</summary>
        public DateTime? Created { get; set; }

        /// <summary>The rendition creator's user id.</summary>
        public string CreatedByUserId { get; set; }

        /// <summary>Rendition comment.</summary>
        public string Comment { get; set; }
    }

    /// <summary>
    /// Revision metadata. Deliberately sparser than Unity API's own RevisionInfo: the
    /// REST API's Revision schema is just id + revisionNumber, no comment/creator at the
    /// revision level (those live on Rendition instead, see RenditionInfo.Comment/CreatedByUserId).
    /// </summary>
    public class RevisionInfo
    {
        /// <summary>Revision id.</summary>
        public string Id { get; set; }

        /// <summary>Revision number, for display/ordering purposes.</summary>
        public int RevisionNumber { get; set; }

        /// <summary>This revision's renditions.</summary>
        public List<RenditionInfo> Renditions { get; set; } = [];
    }

    /// <summary>A document's file content.</summary>
    public class DocumentFile
    {
        /// <summary>File contents as a byte array.</summary>
        public byte[] Content { get; set; }

        /// <summary>File contents as a base64-encoded string, computed from <see cref="Content"/>. Null when <see cref="Content"/> is null.</summary>
        public string Base64Content => Content == null ? null : Convert.ToBase64String(Content);
    }

    /// <summary>A document's full metadata + file contents, for a single-document retrieval.</summary>
    public class DocumentData
    {
        /// <summary>Document information and keywords.</summary>
        public DocumentInfo Metadata { get; set; }

        /// <summary>Document file contents.</summary>
        public DocumentFile File { get; set; }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
