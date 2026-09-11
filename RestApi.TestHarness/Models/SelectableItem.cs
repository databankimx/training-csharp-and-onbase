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
using RestApi.TestHarness.ViewModels;
#endregion

namespace RestApi.TestHarness.Models
{
    #region Training Notes
    /*
     * *Migration Note: identical to Unity.TestHarness's own SelectableItem, purely
     * generic MVVM infrastructure with no Unity API/REST API dependency at all.
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
