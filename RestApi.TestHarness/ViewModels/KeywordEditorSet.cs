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
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using RestApi._02.AccessingTaxonomy.HelperClasses.OnBase;
using RestApi._03.DocumentRetrieval.HelperClasses.OnBase;
#endregion

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own
     * KeywordEditorSet, but genuinely reshaped: both of that class's constructors are
     * replaced with static async factory methods (CreateForDocumentTypeAsync/
     * CreateForDocumentAsync) here, not constructors, since C# constructors can't be
     * async and RestApi.02/03's own lookups this class needs (GetDocumentTypeKeywordGroupsAsync,
     * GetDocumentInfoAsync) are genuinely asynchronous HTTP calls, unlike Unity API's
     * synchronous, purely in-memory equivalents.
     *
     * CreateForDocumentAsync's own "combine schema with existing values" step is also
     * genuinely simpler than Unity.TestHarness's own second constructor: there's no
     * separate "walk doc.KeywordRecords" pass needed here, RestApi.03's own
     * GetDocumentInfoAsync(documentId) ALREADY returns the document's actual values
     * pre-organized into KeywordGroups (each with TypeGroupId identifying which schema
     * group it belongs to, see RestApi.03's own corrected KeywordGroup, and RestApi.04's
     * LectureNotes.md for why that correction mattered) and standalone Keywords, this
     * method just matches each schema group's TypeGroupId against the document's own
     * KeywordGroups to find its existing instance(s), rather than deriving that grouping
     * from a lower-level record walk itself.
     */
    #endregion

    /// <summary>
    /// Aggregates the <see cref="KeywordGroupEditor"/>(s) and <see cref="StandaloneKeywordEditor"/>(s)
    /// needed to edit every keyword on a Document Type (or an existing document).
    /// </summary>
    public class KeywordEditorSet
    {
        #region Properties
        /// <summary>
        /// One editor per named (non-Standalone) Keyword Group Type.
        /// </summary>
        public ObservableCollection<KeywordGroupEditor> Groups { get; } = [];

        /// <summary>
        /// One editor per standalone Keyword Type.
        /// </summary>
        public ObservableCollection<StandaloneKeywordEditor> Standalone { get; } = [];
        #endregion

        #region Constructors
        // Private: only built via the static factory methods below, since building one
        // requires awaiting real HTTP calls, which a constructor can't do.
        private KeywordEditorSet() { }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new, empty KeywordEditorSet, one editor per Keyword Group Type/
        /// standalone Keyword Type on the given Document Type.
        /// </summary>
        /// <param name="documentTypeId">The Document Type id to build editors for.</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The new KeywordEditorSet.</returns>
        public static async Task<KeywordEditorSet> CreateForDocumentTypeAsync(string documentTypeId, HttpClient client)
        {
            var editors = new KeywordEditorSet();

            var allGroups = await OnBaseTaxonomy.GetDocumentTypeKeywordGroupsAsync(documentTypeId, client);
            var (groups, standalone) = OnBaseTaxonomy.SplitKeywordGroups(allGroups);

            foreach (var groupType in groups) editors.Groups.Add(new KeywordGroupEditor(groupType));
            foreach (var keywordType in standalone) editors.Standalone.Add(new StandaloneKeywordEditor(keywordType));

            return editors;
        }

        /// <summary>
        /// Create a new KeywordEditorSet, pre-populated with an existing document's
        /// current keyword values.
        /// </summary>
        /// <param name="documentId">The document id to read existing values from.</param>
        /// <param name="documentTypeId">The document's own Document Type id (for the schema).</param>
        /// <param name="client">The connected HttpClient to use.</param>
        /// <returns>The new KeywordEditorSet.</returns>
        public static async Task<KeywordEditorSet> CreateForDocumentAsync(string documentId, string documentTypeId, HttpClient client)
        {
            var editors = new KeywordEditorSet();

            var allGroups = await OnBaseTaxonomy.GetDocumentTypeKeywordGroupsAsync(documentTypeId, client);
            var (groups, standalone) = OnBaseTaxonomy.SplitKeywordGroups(allGroups);

            var docInfo = await DocumentRetrieval.GetDocumentInfoAsync(documentId, client);

            foreach (var groupSchema in groups)
            {
                var matchingGroups = docInfo?.KeywordGroups?.Where(g => g.TypeGroupId == groupSchema.Id).ToList() ?? [];
                var instances = matchingGroups.Select(g => (IDictionary<string, string>)g.Keywords.ToDictionary(k => k.Id, k => k.Value)).ToList();

                editors.Groups.Add(instances.Count > 0
                    ? new KeywordGroupEditor(groupSchema, instances)
                    : new KeywordGroupEditor(groupSchema));
            }

            foreach (var keywordTypeSchema in standalone)
            {
                var values = docInfo?.Keywords?.Where(k => k.Id == keywordTypeSchema.Id).Select(k => k.Value).ToList() ?? [];

                editors.Standalone.Add(values.Count > 0
                    ? new StandaloneKeywordEditor(keywordTypeSchema, values)
                    : new StandaloneKeywordEditor(keywordTypeSchema));
            }

            return editors;
        }
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
