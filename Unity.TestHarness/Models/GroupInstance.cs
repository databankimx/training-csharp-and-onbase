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
using Unity.TestHarness.ViewModels;
#endregion

namespace Unity.TestHarness.Models
{
    #region Training Notes
    /*
     * *Migration Note: one GroupInstance represents ONE full set of a Keyword Group
     * Type's fields, a MultiInstance group can have several GroupInstances (Add/Remove
     * Instance), a SingleInstance group is permanently capped at exactly one (no
     * add/remove at all). Each instance's Fields are a fresh set of SearchKeywordField
     * wrappers (reused from the search-field design in Taxonomy/Retrieval), one per
     * Keyword Type on the group, so editing one instance never touches another's values.
     */
    #endregion

    /// <summary>
    /// One editable instance of a Keyword Group Type's fields (a MultiInstance group can
    /// have several of these; a SingleInstance group always has exactly one).
    /// </summary>
    public class GroupInstance
    {
        #region Properties
        /// <summary>
        /// This instance's editable fields, one per Keyword Type on the group.
        /// </summary>
        public ObservableCollection<SearchKeywordField> Fields { get; } = new ObservableCollection<SearchKeywordField>();
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the GroupInstance class
        /// </summary>
        /// <param name="groupType">The Keyword Group Type this is an instance of.</param>
        public GroupInstance(KeywordRecordType groupType)
        {
            foreach (var keywordType in groupType.KeywordTypes) Fields.Add(new SearchKeywordField(keywordType));
        }

        /// <summary>
        /// Create a new instance of the GroupInstance class, pre-populated with existing values.
        /// </summary>
        /// <param name="groupType">The Keyword Group Type this is an instance of.</param>
        /// <param name="existingValues">Existing Keyword Type ID/value pairs to populate the new fields with.</param>
        public GroupInstance(KeywordRecordType groupType, IDictionary<long, string> existingValues) : this(groupType)
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
