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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Samples.WinForms.Data;
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
     */
    #endregion

    /// <summary>
    /// The application's main window. Looks up city/county/state by ZIP code, the same
    /// domain every other sample in this training set uses, entirely through direct event
    /// handlers rather than data binding.
    /// </summary>
    public partial class MainForm : Form
    {
        #region Fields
        // The connection string, read from appsettings.json once, in the constructor.
        private readonly string connectionString;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class, reading the
        /// database connection string from <c>appsettings.json</c>.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            connectionString = configuration.GetConnectionString("LocationLookupDatabase")
                ?? throw new InvalidOperationException("LocationLookupDatabase connection string is not configured!");
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the Search button's Click event: queries EF Core for the entered ZIP
        /// code and binds the results directly to <c>gridResults</c>.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The event data.</param>
        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            try
            {
                lblError.Text = string.Empty;
                btnSearch.Enabled = false;

                var options = new DbContextOptionsBuilder<LocationLookupContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                await using var db = new LocationLookupContext(options);
                var results = await db.ZipCodes.Where(z => z.ZipCode1 == txtZipCode.Text).ToListAsync();

                gridResults.DataSource = results;
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
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
