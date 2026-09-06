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
using Hyland.Unity;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: shared by every Archiving mode that needs a keyword editor (Store
     * New builds one from a chosen Document Type's own KeywordRecordTypes; Modify/Add
     * Revision/Add Rendition build one from an EXISTING document's KeywordRecordTypes,
     * pre-populated with its current values). Both constructors' schema-level split
     * (which groups are named vs. StandAlone) now goes through
     * OnBaseTaxonomy.SplitKeywordGroups, a reusable library helper (this exact split used
     * to be independently re-derived here, in Taxonomy, and in DocumentRetrieval, before
     * it existed). It needs no connection/Application, it only operates on the
     * already-loaded DocumentType object passed in, so a plain `new OnBaseTaxonomy()`
     * works fine here, this class has no App/connection of its own to draw on.
     *
     * The SECOND constructor's walk of doc.KeywordRecords (the actual VALUES on this
     * specific document, not the schema) is genuinely different data SplitKeywordGroups
     * doesn't touch, and stays as its own logic here.
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
        /// One editor per named (non-StandAlone) Keyword Group Type.
        /// </summary>
        public ObservableCollection<KeywordGroupEditor> Groups { get; } = new ObservableCollection<KeywordGroupEditor>();

        /// <summary>
        /// One editor per standalone Keyword Type.
        /// </summary>
        public ObservableCollection<StandaloneKeywordEditor> Standalone { get; } = new ObservableCollection<StandaloneKeywordEditor>();
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new, empty instance of the KeywordEditorSet class, one editor per
        /// Keyword Group Type/standalone Keyword Type on <paramref name="docType"/>.
        /// </summary>
        /// <param name="docType">The Document Type to build editors for.</param>
        public KeywordEditorSet(DocumentType docType)
        {
            var (groups, standalone) = new OnBaseTaxonomy().SplitKeywordGroups(docType);

            foreach (var groupType in groups) Groups.Add(new KeywordGroupEditor(groupType));
            foreach (var keywordType in standalone) Standalone.Add(new StandaloneKeywordEditor(keywordType));
        }

        /// <summary>
        /// Create a new instance of the KeywordEditorSet class, pre-populated with an
        /// existing document's current keyword values.
        /// </summary>
        /// <param name="doc">The document to read existing values from.</param>
        public KeywordEditorSet(Document doc)
        {
            // Keyed by ID rather than the KeywordType/KeywordRecordType object itself:
            // Unity API isn't guaranteed to hand back the SAME object reference from
            // doc.KeywordRecords as from doc.DocumentType.KeywordRecordTypes, even though
            // they describe the same underlying definition, ID is the reliable identity.
            var groupTypeInstances = new Dictionary<long, List<Dictionary<long, string>>>();
            var standaloneValues = new Dictionary<long, List<string>>();

            foreach (var record in doc.KeywordRecords)
            {
                if (record.KeywordRecordType.RecordType == RecordType.StandAlone)
                {
                    foreach (var keyword in record.Keywords)
                    {
                        if (keyword == null || keyword.IsBlank) continue;
                        if (!standaloneValues.TryGetValue(keyword.KeywordType.ID, out var values))
                        {
                            values = new List<string>();
                            standaloneValues[keyword.KeywordType.ID] = values;
                        }
                        values.Add(keyword.Value.ToString());
                    }
                }
                else
                {
                    if (!groupTypeInstances.TryGetValue(record.KeywordRecordType.ID, out var instances))
                    {
                        instances = new List<Dictionary<long, string>>();
                        groupTypeInstances[record.KeywordRecordType.ID] = instances;
                    }
                    instances.Add(record.Keywords.Where(k => k != null && !k.IsBlank).ToDictionary(k => k.KeywordType.ID, k => k.Value.ToString()));
                }
            }

            var (groups, standalone) = new OnBaseTaxonomy().SplitKeywordGroups(doc.DocumentType);

            foreach (var groupType in groups)
            {
                Groups.Add(groupTypeInstances.TryGetValue(groupType.ID, out var instances)
                    ? new KeywordGroupEditor(groupType, instances)
                    : new KeywordGroupEditor(groupType));
            }

            foreach (var keywordType in standalone)
            {
                Standalone.Add(standaloneValues.TryGetValue(keywordType.ID, out var values)
                    ? new StandaloneKeywordEditor(keywordType, values)
                    : new StandaloneKeywordEditor(keywordType));
            }
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
