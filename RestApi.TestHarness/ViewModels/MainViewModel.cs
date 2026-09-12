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
using RestApi.TestHarness;
#endregion

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own
     * MainViewModel. Same shell role (sidebar NavigationItems, CurrentPage, the shared
     * Log), same lazy-create-and-cache pattern for page view models. Taxonomy/Retrieval/
     * Archiving/Settings/Help still use PlaceholderViewModel here, matching how
     * Unity.TestHarness itself was built incrementally, page by page, this app is being
     * built the same way; each will be swapped in as its own view model/view are built.
     * "Edit in Archiving" (Retrieval -> Archiving) isn't wired up yet either, for the
     * same reason, neither page exists yet.
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
        private readonly Dictionary<NavigationItem, object> pageCache = [];

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
        public ObservableCollection<NavigationItem> NavigationItems { get; } = [];

        /// <summary>
        /// The shared output log, visible across every page.
        /// </summary>
        public LogViewModel Log { get; } = new LogViewModel();

        /// <summary>
        /// The shared connection state (also the Connect page's own view model). Every
        /// other page checks Connection.IsConnected before performing its own operations.
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
            NavigationItems.Add(new NavigationItem("Retrieval", "\U0001F4C4", () => new RetrievalViewModel(Connection, Log)));
            NavigationItems.Add(new NavigationItem("Archiving", "\U0001F4E6", () => new ArchivingViewModel(Connection, Log)));
            NavigationItems.Add(new NavigationItem("Settings", "\u2699", () => new SettingsViewModel(Connection, Log)));
            NavigationItems.Add(new NavigationItem("Help", "\u2753", () => new HelpViewModel()));

            if (NavigationItems.Count > 0) NavigateTo(NavigationItems[0]);
        }
        #endregion

        #region Private Methods
        // Switch the currently-displayed page
        private void NavigateTo(object parameter)
        {
            if (parameter is not NavigationItem item) return;

            selectedItem?.IsSelected = false;
            selectedItem = item;
            selectedItem.IsSelected = true;

            if (!pageCache.TryGetValue(item, out var pageViewModel))
            {
                pageViewModel = item.GetViewModel();
                pageCache[item] = pageViewModel;
            }

            // The Connect page's settings summary (ApiServerUrl/FormsApiUrl/
            // AuthenticationMode/KeepAlive) is computed from SessionManagement.ServiceLocation,
            // which Settings may have just changed elsewhere; refresh it every time the
            // page is navigated to.
            if (pageViewModel == Connection)
            {
                Connection.RefreshSummaryCommand.Execute(null);
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
