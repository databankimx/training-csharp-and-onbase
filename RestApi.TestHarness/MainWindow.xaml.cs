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
using RestApi.TestHarness.ViewModels;
#endregion

namespace RestApi.TestHarness
{
    #region Training Notes
    /*
     * *Migration Note: same purpose as Unity.TestHarness's own MainWindow.xaml.cs
     * (disconnect-on-exit, so a session's OnBase license isn't left orphaned), but
     * genuinely different mechanics: Unity API's Application.Disconnect() is
     * synchronous, so the WPF Closing event (which has no async-aware overload,
     * CancelEventHandler is plain void) could just call it directly and let the window
     * close immediately after. SessionManagement.DisconnectAsync() here is genuinely
     * async, calling it fire-and-forget from Closing (async void Execute(), same as any
     * ICommand) would let the window close before the disconnect request actually
     * completes, or even started, in the worst case.
     *
     * The fix: cancel the FIRST Closing event (e.Cancel = true), await the disconnect,
     * then call Close() again programmatically once it's done. The second Closing event
     * fires with alreadyDisconnecting already true, so it's let through without
     * repeating the disconnect (and without cancelling this second, final close).
     */
    #endregion

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members
        // Set once the disconnect-on-close sequence has started, so the second
        // (programmatic) Closing event isn't cancelled/re-triggered
        private bool alreadyDisconnecting;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;
        }
        #endregion

        #region Private Methods
        // Disconnect any still-active OnBase session before the app actually exits, so its
        // concurrent client license isn't left orphaned on the API Server
        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (alreadyDisconnecting) return;

            if (DataContext is not MainViewModel mainViewModel || !mainViewModel.Connection.IsConnected)
            {
                return;
            }

            e.Cancel = true;
            alreadyDisconnecting = true;

            await mainViewModel.Connection.DisconnectQuietlyAsync();

            Close();
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
