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
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Samples.Wpf.Data;
using Samples.Wpf.Models;
#endregion

namespace Samples.Wpf.ViewModels
{
    #region Training Notes
    /*
     * MVVM (Model-View-ViewModel) is WPF's standard architectural pattern:
     *
     * - MODEL: the plain data, ZipCode here, an EF Core entity, no UI awareness at all.
     *
     * - VIEW: MainWindow.xaml, pure declarative markup. It has NO code that queries the
     *   database, builds a URL, or handles a click event directly, it just DECLARES what a
     *   TextBox's Text is bound to, what a Button's Command is bound to, and what an
     *   ItemsControl's ItemsSource is bound to. Compare this against every web sample in
     *   this training set, where the "view" (a .cshtml/.aspx file) is rendered fresh on
     *   every single request, a WPF View is a long-lived object that stays on screen and
     *   gets updated in place as bound properties change.
     *
     * - VIEWMODEL: this class. It exposes bindable properties (ZipCode, Locations,
     *   ErrorMessage, IsSearching) and a bindable command (SearchCommand). It has NO
     *   reference to any WPF/UI type at all, nothing here imports System.Windows, it could
     *   be unit tested with no UI involved whatsoever, a real, practical benefit of the
     *   pattern.
     *
     * Data binding is what wires the two together, entirely through XAML markup
     * (Text="{Binding ZipCode}", Command="{Binding SearchCommand}"), no manual
     * event-wiring code at all.
     */
    #endregion

    /// <summary>
    /// The ViewModel for <see cref="MainWindow"/>. Exposes the ZIP code search state and
    /// command that the View binds to; contains no reference to any WPF/UI type.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        // The connection string is passed in from the View's code-behind, which reads it
        private readonly string connectionString;

        // Zip code to search for, defaulting to 75067 (Lewisville, TX) so the user can just
        // press search immediately.
        private string zipCode = "75067";

        // Error message to display if the search fails, null if no error.
        private string? errorMessage;

        // True if a search is in progress, false otherwise. This is used to disable the Search
        private bool isSearching;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the ZIP Code.
        /// </summary>
        public string ZipCode
        {
            get => zipCode;
            set => SetField(ref zipCode, value);
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage
        {
            get => errorMessage;
            set => SetField(ref errorMessage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a search operation is in progress.
        /// </summary>
        public bool IsSearching
        {
            get => isSearching;
            set => SetField(ref isSearching, value);
        }

        /// <summary>
        /// Gets the collection of locations.
        /// </summary>
        /// <remarks>
        /// ObservableCollection, not List, this is what lets the bound DataGrid/ItemsControl
        ///   automatically pick up Add/Remove/Clear without any manual "refresh the UI" call.
        /// </remarks>
        public ObservableCollection<ZipCode> Locations { get; } = [];

        /// <summary>
        /// Gets the command that executes a search operation.
        /// </summary>
        public RelayCommand SearchCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <c>MainViewModel</c> class.
        /// </summary>
        /// <remarks>Initializes the search command with asynchronous execution and a can-execute
        /// condition based on <c>IsSearching</c>.</remarks>
        /// <param name="connectionString">The connection string used to access the backing data store.</param>
        public MainViewModel(string connectionString)
        {
            this.connectionString = connectionString;
            // async void is generally discouraged, but is the accepted, idiomatic exception
            //   for a UI command handler, there's no caller to await it, WPF's dispatcher is
            //   already the "top" of this call stack.
            SearchCommand = new RelayCommand(async () => await SearchAsync(), () => !IsSearching);
        }
        #endregion

        #region Helper Functions
        // This is the actual search logic, called by the SearchCommand. It uses EF Core to query the database for matching ZIP codes.
        private async Task SearchAsync()
        {
            try
            {
                IsSearching = true;
                ErrorMessage = null;
                Locations.Clear();

                var options = new DbContextOptionsBuilder<LocationLookupContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                await using var db = new LocationLookupContext(options);
                var results = await db.ZipCodes.Where(z => z.ZipCode1 == ZipCode).ToListAsync();

                foreach (var result in results) Locations.Add(result);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsSearching = false;
            }
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
