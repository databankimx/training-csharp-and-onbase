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
using RestApi._02.AccessingTaxonomy.Models.Objects;
using RestApi.TestHarness.ViewModels;
#endregion

namespace RestApi.TestHarness.Models
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own
     * SearchKeywordField. Id is a string (matching RestApi.02's own KeywordType.Id), and
     * DataType is a string too ("Numeric9"/"Alphanumeric"/"Date"/etc., matching
     * document-api.json's own KeywordType.dataType enum), not Hyland.Unity's
     * KeywordDataType enum. No Length property: RestApi.02's own KeywordType doesn't
     * expose a data-length field at all (document-api.json's KeywordType schema doesn't
     * include one), so there's nothing to carry through here either; this app's own UI
     * doesn't visibly enforce field-length validation from it regardless.
     */
    #endregion

    /// <summary>
    /// A single, dynamically-generated search field for one Keyword Type, with a
    /// bindable <see cref="Value"/> for the user to fill in.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the SearchKeywordField class
    /// </remarks>
    /// <param name="keywordType">The keyword type this field represents.</param>
    public class SearchKeywordField(KeywordType keywordType) : ViewModelBase
    {
        #region Private Members
        private string value;
        #endregion

        #region Properties
        /// <summary>
        /// The keyword type's ID.
        /// </summary>
        public string Id { get; } = keywordType.Id;

        /// <summary>
        /// The keyword type's name (the field's label).
        /// </summary>
        public string Name { get; } = keywordType.Name;

        /// <summary>
        /// The keyword type's data type (e.g. "Numeric9", "Alphanumeric", "Date").
        /// </summary>
        public string DataType { get; } = keywordType.DataType;

        /// <summary>
        /// The value the user has entered for this field.
        /// </summary>
        public string Value
        {
            get => value;
            set => SetField(ref this.value, value);
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
