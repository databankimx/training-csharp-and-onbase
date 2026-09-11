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
using RestApi._02.AccessingTaxonomy.Models.Objects;
#endregion

namespace RestApi.TestHarness.Models
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own GroupInstance.
     * Built from RestApi.02's own DocumentTypeKeywordGroup, whose KeywordTypes are
     * already fully resolved (see RestApi.02's own Training Notes), so this class needs
     * no further lookups of its own, same as before. Keyed by string ids throughout
     * (existingValues is now Dictionary&lt;string, string&gt;), matching RestApi.02/03's
     * own id typing.
     */
    #endregion

    /// <summary>
    /// One editable instance of a Keyword Group Type's fields (a Multi-Instance group can
    /// have several of these; a Single-Instance group always has exactly one).
    /// </summary>
    public class GroupInstance
    {
        #region Properties
        /// <summary>
        /// This instance's editable fields, one per Keyword Type on the group.
        /// </summary>
        public ObservableCollection<SearchKeywordField> Fields { get; } = [];
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the GroupInstance class
        /// </summary>
        /// <param name="groupType">The Keyword Group Type this is an instance of.</param>
        public GroupInstance(DocumentTypeKeywordGroup groupType)
        {
            foreach (var keywordType in groupType.KeywordTypes) Fields.Add(new SearchKeywordField(keywordType));
        }

        /// <summary>
        /// Create a new instance of the GroupInstance class, pre-populated with existing values.
        /// </summary>
        /// <param name="groupType">The Keyword Group Type this is an instance of.</param>
        /// <param name="existingValues">Existing Keyword Type ID/value pairs to populate the new fields with.</param>
        public GroupInstance(DocumentTypeKeywordGroup groupType, IDictionary<string, string> existingValues) : this(groupType)
        {
            foreach (var field in Fields.Where(f => existingValues.ContainsKey(f.Id))) field.Value = existingValues[field.Id];
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
