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
using Microsoft.Extensions.Configuration;
using Samples.Wpf.ViewModels;
#endregion

namespace Samples.Wpf
{
    /// <summary>
    /// The main entry point for the WPF application. It reads configuration from appsettings.json,
    /// initializes the MainViewModel with the database connection string, and sets up the
    /// MainWindow with the ViewModel as its DataContext.
    /// </summary>
    public partial class App : Application
    {
        #region Methods
        // *Migration Note: a plain WPF app (unlike the ASP.NET Core samples) has no
        //   WebApplicationBuilder-style host reading appsettings.json automatically, this is
        //   wired up by hand here, the same situation Samples.MvcWebApi.Core.Client (also a
        //   plain, non-hosted app) was in. See LectureNotes.md.
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var connectionString = configuration.GetConnectionString("LocationLookupDatabase")
                ?? throw new InvalidOperationException("LocationLookupDatabase connection string is not configured!");

            var viewModel = new MainViewModel(connectionString);

            var mainWindow = new MainWindow { DataContext = viewModel };
            mainWindow.Show();
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
