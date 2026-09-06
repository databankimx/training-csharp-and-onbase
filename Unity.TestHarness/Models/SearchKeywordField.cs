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
using Hyland.Unity;
using Unity.TestHarness.ViewModels;
#endregion

namespace Unity.TestHarness.Models
{
    #region Training Notes
    /*
     * *Migration Note: mirrors the original WinForms harness's KeywordsForm, one field
     * per keyword type, dynamically generated, except inline in the search pane rather
     * than a separate modal dialog, and bindable rather than manually wiring up
     * Label/TextBox pairs in code-behind.
     */
    #endregion

    /// <summary>
    /// A single, dynamically-generated search field for one Keyword Type, with a
    /// bindable <see cref="Value"/> for the user to fill in.
    /// </summary>
    public class SearchKeywordField : ViewModelBase
    {
        #region Private Members
        private string value;
        #endregion

        #region Properties
        /// <summary>
        /// The keyword type's ID.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// The keyword type's name (the field's label).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The keyword type's data type (governs input validation, e.g., max length for
        /// AlphaNumeric fields).
        /// </summary>
        public KeywordDataType DataType { get; }

        /// <summary>
        /// The keyword type's maximum data length.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// The value the user has entered for this field.
        /// </summary>
        public string Value
        {
            get => value;
            set => SetField(ref this.value, value);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the SearchKeywordField class
        /// </summary>
        /// <param name="keywordType">The keyword type this field represents.</param>
        public SearchKeywordField(KeywordType keywordType)
        {
            Id = keywordType.ID;
            Name = keywordType.Name;
            DataType = keywordType.DataType;
            Length = keywordType.DataLength;
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
