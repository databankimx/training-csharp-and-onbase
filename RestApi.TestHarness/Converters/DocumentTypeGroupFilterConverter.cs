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
using System.Globalization;
using System.Windows.Data;
using RestApi._02.AccessingTaxonomy.Models.Objects;
#endregion

namespace RestApi.TestHarness.Converters
{
    #region Training Notes
    /*
     * *Migration Note: identical to Unity.TestHarness's own DocumentTypeGroupFilterConverter,
     * just bound to RestApi.02's own DocumentTypeGroup instead of Hyland.Unity's. See
     * that class's own Training Notes for why a converter is needed here at all
     * (TargetNullValue doesn't catch a null DataContext itself, only a null property
     * path evaluated against a non-null one).
     */
    #endregion

    /// <summary>
    /// Converts a <see cref="DocumentTypeGroup"/> (or <see langword="null"/>, representing
    /// "no filter") into display text for the group filter dropdown.
    /// </summary>
    public class DocumentTypeGroupFilterConverter : IValueConverter
    {
        #region Public Methods
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is DocumentTypeGroup group ? group.Name : "All Groups";
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
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
