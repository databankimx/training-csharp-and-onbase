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
using System.IO;
using System.Reflection;
using Unity.TestHarness.Views;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: deliberately minimal, almost all of this page's actual content
     * (per-page instructions, the About/License text) lives directly in HelpView.xaml as
     * static markup rather than bound properties here, none of it changes at runtime or
     * needs a data source, a view model would just be ceremony around plain text.
     * AppVersion is the one thing that DOES need code (pulled from the assembly itself,
     * so it can never drift out of sync with an actual build).
     *
     * ShowLicenseCommand reads Resources\LICENSE from the OUTPUT directory at runtime
     * (copied there by the .csproj's CopyToOutputDirectory, from a copy of the solution
     * root's own LICENSE kept in this project's Resources folder), rather than embedding
     * its text as a string constant here, so it's always the REAL file's content, not a
     * duplicate that could drift out of sync with it.
     */
    #endregion

    /// <summary>
    /// The Help page: usage instructions, an About section, and license/support terms.
    /// Nearly all of its content lives directly in HelpView.xaml; this exists mainly for
    /// <see cref="AppVersion"/> and <see cref="ShowLicenseCommand"/>.
    /// </summary>
    public class HelpViewModel : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// This build's version, read from the assembly itself.
        /// </summary>
        public string AppVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
        #endregion

        #region Commands
        /// <summary>
        /// Opens a dialog showing the license's real text, read from
        /// <c>Resources\LICENSE</c> in the output directory.
        /// </summary>
        public RelayCommand ShowLicenseCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the HelpViewModel class
        /// </summary>
        public HelpViewModel()
        {
            ShowLicenseCommand = new RelayCommand(_ => ShowLicense());
        }
        #endregion

        #region Private Methods
        // Open a dialog showing the license's real text
        private void ShowLicense()
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "LICENSE");
                var text = File.Exists(path) ? File.ReadAllText(path) : "LICENSE file not found.";

                new LicenseDialog(text) { Owner = System.Windows.Application.Current?.MainWindow }.ShowDialog();
            }
            catch (Exception ex)
            {
                new LicenseDialog($"Error reading LICENSE file:\n\n{ex.Message}").ShowDialog();
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
