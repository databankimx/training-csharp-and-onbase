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
using System.Windows;
using Unity.TestHarness.ViewModels;
#endregion

namespace Unity.TestHarness
{
    #region Training Notes
    /*
     * *Migration Note: Closing is what makes disconnect-on-exit actually happen. An
     * OnBase session left connected consumes a concurrent client license on the
     * Application Server until it's either explicitly disconnected or the server itself
     * eventually reclaims it (typically only on an Application Server restart or an idle
     * timeout, if configured); simply letting the process exit without disconnecting
     * orphans that license for however long that takes. This only covers a NORMAL close
     * (the window's own close button, Alt+F4, Disconnect not already clicked), a killed
     * process (Task Manager, a crash, a forced shutdown) can't run any handler at all,
     * an inherent limitation of any graceful-shutdown hook, not something this app can
     * work around.
     */
    #endregion

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Create a new instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;
        }

        // Disconnect any still-active OnBase session before the app actually exits, so its
        // concurrent client license isn't left orphaned on the Application Server
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (DataContext is MainViewModel mainViewModel && mainViewModel.Connection.IsConnected)
                {
                    mainViewModel.Connection.DisconnectCommand.Execute(null);
                }
            }
            catch (Exception)
            {
                // Never block the window from closing over a disconnect failure; the
                // underlying ConnectionViewModel.Disconnect() already logs its own errors.
            }
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
