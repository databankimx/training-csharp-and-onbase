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
#endregion

namespace Unity.TestHarness.Converters
{
    /// <summary>
    /// Converts <see cref="ViewModels.MainViewModel.IsSidebarExpanded"/> into the
    /// collapse/expand toggle button's chevron: "&#8249;" (collapse) when expanded,
    /// "&#8250;" (expand) when collapsed.
    /// </summary>
    public class CollapseGlyphConverter : IValueConverter
    {
        #region Public Methods
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isExpanded && isExpanded ? "\u2039" : "\u203A";
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
