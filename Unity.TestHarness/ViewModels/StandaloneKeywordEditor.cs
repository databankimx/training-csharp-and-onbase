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
using Hyland.Unity;
using Unity.TestHarness.Models;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: standalone keywords manage multiplicity PER KEYWORD, not per
     * group, unlike KeywordGroupEditor's per-GROUP instance management: any standalone
     * Keyword Type can independently have more than one value on a document (the
     * "Description" keyword showing up twice, say), there's no SingleInstance/
     * MultiInstance distinction at the standalone level the way there is for groups,
     * every standalone keyword allows multiple values.
     */
    #endregion

    /// <summary>
    /// Manages one or more editable values for a single standalone Keyword Type.
    /// </summary>
    public class StandaloneKeywordEditor : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// The Keyword Type being edited.
        /// </summary>
        public KeywordType KeywordType { get; }

        /// <summary>
        /// The keyword's name.
        /// </summary>
        public string Name => KeywordType.Name;

        /// <summary>
        /// This keyword's current values.
        /// </summary>
        public ObservableCollection<SearchKeywordField> Values { get; } = new ObservableCollection<SearchKeywordField>();
        #endregion

        #region Commands
        /// <summary>
        /// Adds a new, empty value.
        /// </summary>
        public RelayCommand AddValueCommand { get; }

        /// <summary>
        /// Removes the <see cref="SearchKeywordField"/> passed as the command parameter.
        /// Disabled once down to the last remaining value.
        /// </summary>
        public RelayCommand RemoveValueCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the StandaloneKeywordEditor class
        /// </summary>
        /// <param name="keywordType">The Keyword Type to edit.</param>
        public StandaloneKeywordEditor(KeywordType keywordType)
        {
            KeywordType = keywordType;

            AddValueCommand = new RelayCommand(_ => AddValue());
            RemoveValueCommand = new RelayCommand(param => RemoveValue(param as SearchKeywordField), _ => Values.Count > 1);

            AddValue();
        }

        /// <summary>
        /// Create a new instance of the StandaloneKeywordEditor class, pre-populated with
        /// an existing document's value(s) for this keyword.
        /// </summary>
        /// <param name="keywordType">The Keyword Type to edit.</param>
        /// <param name="existingValues">The existing value(s) to populate.</param>
        public StandaloneKeywordEditor(KeywordType keywordType, IEnumerable<string> existingValues)
        {
            KeywordType = keywordType;

            AddValueCommand = new RelayCommand(_ => AddValue());
            RemoveValueCommand = new RelayCommand(param => RemoveValue(param as SearchKeywordField), _ => Values.Count > 1);

            foreach (var value in existingValues)
            {
                var field = new SearchKeywordField(KeywordType) { Value = value };
                Values.Add(field);
            }
            if (Values.Count == 0) AddValue();
        }
        #endregion

        #region Private Methods
        // Add a new, empty value
        private void AddValue() => Values.Add(new SearchKeywordField(KeywordType));

        // Remove the given value
        private void RemoveValue(SearchKeywordField field)
        {
            if (field != null) Values.Remove(field);
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
