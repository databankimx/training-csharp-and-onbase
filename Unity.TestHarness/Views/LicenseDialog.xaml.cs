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
using System.Windows;
#endregion

namespace Unity.TestHarness.Views
{
    /// <summary>
    /// A simple, scrollable dialog for displaying the license's real text (not a
    /// MessageBox, which handles a multi-paragraph document poorly).
    /// </summary>
    public partial class LicenseDialog : Window
    {
        /// <summary>
        /// Create a new instance of the LicenseDialog class
        /// </summary>
        /// <param name="licenseText">The license text to display.</param>
        public LicenseDialog(string licenseText)
        {
            InitializeComponent();
            LicenseTextBlock.Text = licenseText;
        }

        // Close the dialog
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
