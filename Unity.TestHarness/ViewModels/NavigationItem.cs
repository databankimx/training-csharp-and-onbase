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
#endregion

namespace Unity.TestHarness.ViewModels
{
    /// <summary>
    /// A single entry in the sidebar navigation.
    /// </summary>
    public class NavigationItem : ViewModelBase
    {
        #region Private Members
        // Whether this is the currently-displayed page
        private bool isSelected;
        #endregion

        #region Properties
        /// <summary>
        /// The label shown in the sidebar.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The emoji glyph shown in the sidebar, both expanded and collapsed.
        /// </summary>
        public string Glyph { get; }

        /// <summary>
        /// Lazily creates (once) and returns this page's view model.
        /// </summary>
        public Func<object> GetViewModel { get; }

        /// <summary>
        /// Whether this is the currently-displayed page.
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set => SetField(ref isSelected, value);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the NavigationItem class
        /// </summary>
        /// <param name="name">The label shown in the sidebar.</param>
        /// <param name="glyph">The emoji glyph shown in the sidebar.</param>
        /// <param name="getViewModel">Lazily creates (once) and returns this page's view model.</param>
        public NavigationItem(string name, string glyph, Func<object> getViewModel)
        {
            Name = name;
            Glyph = glyph;
            GetViewModel = getViewModel;
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
