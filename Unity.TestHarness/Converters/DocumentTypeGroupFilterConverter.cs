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
using Hyland.Unity;
#endregion

namespace Unity.TestHarness.Converters
{
    #region Training Notes
    /*
     * *Migration Note: TargetNullValue does NOT catch this case, it substitutes when a
     * binding's PROPERTY PATH evaluates to null (e.g., {Binding Name} where Name itself
     * is null), not when the DataContext being bound against is itself null in the first
     * place, WPF can't evaluate "Name" on a null object at all, so that path never
     * produces a null TargetNullValue could intercept. Binding directly to the item
     * ({Binding}, an empty path) and handling null explicitly in a converter sidesteps
     * the ambiguity entirely.
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
