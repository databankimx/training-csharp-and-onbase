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
using Unity.TestHarness;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: this is the whole shell, the sidebar's NavigationItems, which page
     * is currently shown (CurrentPage), and the shared Log every page's view model writes
     * to. Each page's view model is created LAZILY (on first visit, via
     * NavigationItem.GetViewModel) and CACHED thereafter, so switching pages preserves
     * whatever state you had (a populated results grid, filled-in form fields), rather
     * than rebuilding the page from scratch every time you navigate to it. Pages not yet
     * built use PlaceholderViewModel as a stand-in, so the shell itself is fully
     * functional (and testable) before every real page exists.
     */
    #endregion

    /// <summary>
    /// The application shell: sidebar navigation, the currently-displayed page, and the
    /// shared output log.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Private Members
        // Cached page view model instances, keyed by NavigationItem
        private readonly Dictionary<NavigationItem, object> pageCache = new Dictionary<NavigationItem, object>();

        // The currently-displayed page's view model
        private object currentPage;

        // The currently-selected navigation item
        private NavigationItem selectedItem;

        // Whether the sidebar is expanded, initialized from the value HarnessSettings
        // remembered from the previous run
        private bool isSidebarExpanded = HarnessSettings.GetSidebarExpanded();
        #endregion

        #region Properties
        /// <summary>
        /// The sidebar's navigation entries.
        /// </summary>
        public ObservableCollection<NavigationItem> NavigationItems { get; } = new ObservableCollection<NavigationItem>();

        /// <summary>
        /// The shared output log, visible across every page.
        /// </summary>
        public LogViewModel Log { get; } = new LogViewModel();

        /// <summary>
        /// The shared connection state (also the Connect page's own view model). Every
        /// other page reads Connection.CurrentApplication to perform its own operations.
        /// </summary>
        public ConnectionViewModel Connection { get; }

        /// <summary>
        /// The currently-displayed page's view model.
        /// </summary>
        public object CurrentPage
        {
            get => currentPage;
            private set => SetField(ref currentPage, value);
        }

        /// <summary>
        /// Whether the sidebar shows full labels (<see langword="true"/>) or is collapsed
        /// to just its glyph icons (<see langword="false"/>). Changes are remembered
        /// across runs via <see cref="HarnessSettings"/>.
        /// </summary>
        public bool IsSidebarExpanded
        {
            get => isSidebarExpanded;
            set
            {
                if (!SetField(ref isSidebarExpanded, value)) return;
                HarnessSettings.SetSidebarExpanded(value);
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Navigates to the <see cref="NavigationItem"/> passed as the command parameter.
        /// </summary>
        public RelayCommand NavigateCommand { get; }

        /// <summary>
        /// Toggles <see cref="IsSidebarExpanded"/>.
        /// </summary>
        public RelayCommand ToggleSidebarCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the MainViewModel class
        /// </summary>
        public MainViewModel()
        {
            Connection = new ConnectionViewModel(Log);

            NavigateCommand = new RelayCommand(NavigateTo);
            ToggleSidebarCommand = new RelayCommand(_ => IsSidebarExpanded = !IsSidebarExpanded);

            NavigationItems.Add(new NavigationItem("Connect", "\U0001F50C", () => Connection));
            NavigationItems.Add(new NavigationItem("Taxonomy", "\U0001F50D", () => new TaxonomyViewModel(Connection, Log)));
            NavigationItems.Add(new NavigationItem("Retrieval", "\U0001F4C4", () => new PlaceholderViewModel("Retrieval")));
            NavigationItems.Add(new NavigationItem("Archiving", "\U0001F4E6", () => new PlaceholderViewModel("Archiving")));
            NavigationItems.Add(new NavigationItem("Settings", "\u2699", () => new SettingsViewModel(Log)));

            if (NavigationItems.Count > 0) NavigateTo(NavigationItems[0]);
        }
        #endregion

        #region Private Methods
        // Switch the currently-displayed page
        private void NavigateTo(object parameter)
        {
            if (!(parameter is NavigationItem item)) return;

            if (selectedItem != null) selectedItem.IsSelected = false;
            selectedItem = item;
            selectedItem.IsSelected = true;

            if (!pageCache.TryGetValue(item, out var pageViewModel))
            {
                pageViewModel = item.GetViewModel();
                pageCache[item] = pageViewModel;
            }

            // The Connect page's settings summary (ServicePath/DataSource/AuthenticationMode/
            // KeepAlive) is computed from SessionManagement.ServiceLocation, which Settings
            // may have just changed elsewhere; refresh it, and re-check server
            // availability, every time the page is navigated to.
            if (pageViewModel == Connection)
            {
                Connection.RefreshSummaryCommand.Execute(null);
                Connection.TestServerCommand.Execute(null);
            }

            CurrentPage = pageViewModel;
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
