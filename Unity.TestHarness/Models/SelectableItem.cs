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
using Unity.TestHarness.ViewModels;
#endregion

namespace Unity.TestHarness.Models
{
    #region Training Notes
    /*
     * *Migration Note: WPF's ListBox doesn't naturally support MVVM-bound multi-select
     * (SelectedItems isn't a DependencyProperty), the standard workaround is exactly
     * this: wrap each item with its own bindable IsSelected, and use CheckBoxes (bound to
     * IsSelected) inside an ItemsControl instead of a ListBox's own selection mechanism.
     * Generic so it can wrap any item type (used here for DocumentType, but not tied to it).
     */
    #endregion

    /// <summary>
    /// Wraps an item with a bindable <see cref="IsSelected"/>, for MVVM-bound multi-select lists.
    /// </summary>
    /// <typeparam name="T">The wrapped item's type.</typeparam>
    /// <remarks>
    /// Create a new instance of the SelectableItem class
    /// </remarks>
    /// <param name="item">The item to wrap.</param>
    public class SelectableItem<T>(T item) : ViewModelBase
    {
        #region Private Members
        private bool isSelected;
        #endregion

        #region Properties
        /// <summary>
        /// The wrapped item.
        /// </summary>
        public T Item { get; } = item;

        /// <summary>
        /// Whether this item is currently selected.
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set => SetField(ref isSelected, value);
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
