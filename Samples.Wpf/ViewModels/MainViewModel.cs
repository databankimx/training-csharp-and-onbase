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
using System.Linq;
using CSharp.SharedLibrary.Models;
using Samples.Wpf.Models;
#endregion

namespace Samples.Wpf.ViewModels
{
    #region Training Notes
    /*
     * MVVM (Model-View-ViewModel) is WPF's standard architectural pattern:
     *
     * - MODEL: the plain data, ZipCode here, an EF6 Database-First entity, no UI awareness
     *   at all.
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
     *
     * *Migration Note: net48 (not net10.0-windows, see Samples.Wpf.csproj for why),
     * EF6 Database-First (ExternalDataEntities, reading its connection string from
     * App.config automatically, no manual configuration wiring needed anywhere in this
     * project), and DatabankException (CSharp.SharedLibrary is a valid reference on
     * net48).
     */
    #endregion

    /// <summary>
    /// The ViewModel for <see cref="MainWindow"/>. Exposes the ZIP code search state and
    /// command that the View binds to; contains no reference to any WPF/UI type.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private string zipCode = "75067";
        private string errorMessage;
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
        public string ErrorMessage
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
        /// automatically pick up Add/Remove/Clear without any manual "refresh the UI" call.
        /// </remarks>
        public ObservableCollection<ZipCode> Locations { get; } = new ObservableCollection<ZipCode>();

        /// <summary>
        /// Gets the command that executes a search operation.
        /// </summary>
        public RelayCommand SearchCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <c>MainViewModel</c> class.
        /// </summary>
        public MainViewModel()
        {
            SearchCommand = new RelayCommand(Search, () => !IsSearching);
        }
        #endregion

        #region Helper Functions
        // This is the actual search logic, called by the SearchCommand. Runs synchronously,
        // matching the direct EF6 usage pattern already established in
        // Samples.MvcWebPortal/Samples.WebForms, rather than EF Core's async query methods.
        private void Search()
        {
            try
            {
                IsSearching = true;
                ErrorMessage = null;
                Locations.Clear();

                using (var db = new ExternalDataEntities())
                {
                    var results = db.ZipCodes.Where(z => z.ZipCode1 == ZipCode).ToList();
                    foreach (var result in results) Locations.Add(result);
                }
            }
            catch (System.Exception ex)
            {
                var wrapped = new DatabankException("Error looking up locations!", ex);
                ErrorMessage = wrapped.Message;
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
