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
using System.Linq;
using System.Windows.Forms;
using CSharp.SharedLibrary.Models;
using Samples.WinForms.Models;
#endregion

namespace Samples.WinForms
{
    #region Training Notes
    /*
     * WinForms' idiomatic pattern is direct and imperative, genuinely different from
     * Samples.Wpf's MVVM approach:
     *
     * - No ViewModel, no data binding, no ICommand. BtnSearch_Click below reads
     *   txtZipCode.Text DIRECTLY, runs the query, and writes the results DIRECTLY into
     *   gridResults.DataSource, all imperative code triggered by an event subscription
     *   (see MainForm.Designer.cs's "btnSearch.Click += BtnSearch_Click;" line).
     *
     * - The UI itself (MainForm.Designer.cs) is generated C# code, "new Label()",
     *   "Controls.Add(...)", property assignments, not a markup language. Compare this
     *   directly against Samples.Wpf/MainWindow.xaml, which declares an equivalent UI
     *   shape entirely in XAML.
     *
     * - WinForms DOES support data binding (BindingSource, Bindings.Add), but it's far
     *   less central to the framework's culture than in WPF, most real-world WinForms
     *   code looks like this: event handlers directly manipulating control properties.
     *
     * *Migration Note: net48 (not net10.0-windows, see Samples.WinForms.csproj), EF6
     * Database-First (ExternalDataEntities, reading its connection string from App.config
     * automatically), and DatabankException (CSharp.SharedLibrary is a valid reference on
     * net48).
     */
    #endregion

    /// <summary>
    /// The application's main window. Looks up city/county/state by ZIP code, the same
    /// domain every other sample in this training set uses, entirely through direct event
    /// handlers rather than data binding.
    /// </summary>
    public partial class MainForm : Form
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the Search button's Click event: queries EF6 for the entered ZIP code
        /// and binds the results directly to <c>gridResults</c>.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The event data.</param>
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                lblError.Text = string.Empty;
                btnSearch.Enabled = false;

                using (var db = new ExternalDataEntities())
                {
                    var results = db.ZipCodes.Where(z => z.ZipCode1 == txtZipCode.Text).ToList();
                    gridResults.DataSource = results;
                }
            }
            catch (Exception ex)
            {
                var wrapped = new DatabankException("Error looking up locations!", ex);
                lblError.Text = wrapped.Message;
            }
            finally
            {
                btnSearch.Enabled = true;
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
