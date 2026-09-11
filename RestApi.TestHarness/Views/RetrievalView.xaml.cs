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
using System.Windows.Controls;
using RestApi.TestHarness.ViewModels;
#endregion

namespace RestApi.TestHarness.Views
{
    #region Training Notes
    /*
     * *Migration Note: identical to Unity.TestHarness's own RetrievalView.xaml.cs.
     * Auto-scrolling to the results list after a search is genuinely a VIEW concern, so
     * it's handled here in code-behind rather than via a binding.
     */
    #endregion

    /// <summary>
    /// Interaction logic for RetrievalView.xaml
    /// </summary>
    public partial class RetrievalView : UserControl
    {
        /// <summary>
        /// Create a new instance of the RetrievalView class
        /// </summary>
        public RetrievalView()
        {
            InitializeComponent();

            Loaded += RetrievalView_Loaded;
        }

        // Subscribe to SearchCompleted once the view model is available. Unsubscribe
        // first: MainViewModel caches page view model instances, but WPF recreates this
        // View's visual (and re-fires Loaded) each time you navigate back to this page,
        // without the unsubscribe this would register a duplicate handler on the SAME
        // long-lived view model every time, firing BringIntoView() multiple times per search.
        private void RetrievalView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is not RetrievalViewModel viewModel) return;
            viewModel.SearchCompleted -= ViewModel_SearchCompleted;
            viewModel.SearchCompleted += ViewModel_SearchCompleted;
        }

        // Scroll the results list into view after a search completes
        private void ViewModel_SearchCompleted(object sender, EventArgs e)
        {
            ResultsListBox.BringIntoView();
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
