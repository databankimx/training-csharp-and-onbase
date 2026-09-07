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
using System.Linq;
using Unity._03.DocumentRetrieval.Models.Objects;
#endregion

namespace Unity.TestHarness.Web.Models
{
    #region Training Notes
    /*
     * *Migration Note: deliberately NOT duplicating DocumentInfo/RevisionInfo/
     * RenditionInfo/DocumentLink as parallel web-specific DTOs, Unity.03.DocumentRetrieval's
     * own versions (added while auditing Unity.TestHarness, the WPF version, for logic
     * that belongs in the library) are already plain, serializable classes with no Unity
     * API dependencies, exactly what this page needs for session storage and JSON
     * responses. This class and RetrievalDetailModel are the only web-specific wrapping:
     * page-level state (which taxonomy is loaded, current search results, which
     * revision/rendition is currently selected) that has no equivalent in the library
     * because it's UI state, not domain data.
     */
    #endregion

    /// <summary>
    /// Which of the three search modes the Retrieval page is currently using.
    /// </summary>
    public enum RetrievalSearchMode
    {
        /// <summary>Search by one or more Document Types.</summary>
        DocumentType,

        /// <summary>Search by a Custom Query.</summary>
        CustomQuery,

        /// <summary>Retrieve a single document directly by ID.</summary>
        DocumentId
    }

    /// <summary>
    /// View-facing data for the Retrieval page: the loaded taxonomy (for the search mode
    /// selectors), the current search results, and the currently-loaded document's detail
    /// (if any), all held per-session.
    /// </summary>
    public class RetrievalPageModel
    {
        /// <summary>Whether taxonomy data has been loaded yet this session.</summary>
        public bool IsTaxonomyLoaded { get; set; }

        /// <summary>Every Document Type Group, for the group filter.</summary>
        public List<NamedItem> DocumentTypeGroups { get; set; } = new List<NamedItem>();

        /// <summary>Every Document Type.</summary>
        public List<NamedItem> AllDocumentTypes { get; set; } = new List<NamedItem>();

        /// <summary>Every Custom Query.</summary>
        public List<NamedItem> CustomQueries { get; set; } = new List<NamedItem>();

        /// <summary>The current search results, if a search has been run.</summary>
        public List<DocumentInfo> SearchResults { get; set; } = new List<DocumentInfo>();

        /// <summary>The currently-loaded document's detail, or <see langword="null"/> if none is loaded.</summary>
        public RetrievalDetailModel Detail { get; set; }
    }

    /// <summary>
    /// The currently-selected document's detail pane: metadata, links, and its full
    /// revision/rendition tree, with the currently-selected revision/rendition tracked by ID.
    /// </summary>
    public class RetrievalDetailModel
    {
        /// <summary>The document's metadata (keywords, keyword groups, and other display fields).</summary>
        public DocumentInfo Metadata { get; set; }

        /// <summary>The document's DocPop/UnityPop links.</summary>
        public DocumentLink Links { get; set; }

        /// <summary>The document's revisions.</summary>
        public List<RevisionInfo> Revisions { get; set; } = new List<RevisionInfo>();

        /// <summary>The currently-selected revision's ID.</summary>
        public long SelectedRevisionId { get; set; }

        /// <summary>The currently-selected rendition's file type ID.</summary>
        public long SelectedRenditionFileTypeId { get; set; }

        /// <summary>The currently-selected revision.</summary>
        public RevisionInfo SelectedRevision => Revisions.FirstOrDefault(r => r.Id == SelectedRevisionId);

        /// <summary>The currently-selected rendition.</summary>
        public RenditionInfo SelectedRendition => SelectedRevision?.Renditions.FirstOrDefault(r => r.FileTypeId == SelectedRenditionFileTypeId);
    }
}
