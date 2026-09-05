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
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Hyland.Unity;
using Unity._00.CommonFunctionality.Models.Enumerations;
using Unity._01.ConnectingToOnBase.HelperClasses.OnBase;
using UnityApplication = Hyland.Unity.Application;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: this is BOTH the Connect page's own view model AND the shared
     * connection state every other page reads (via MainViewModel.Connection), a single
     * instance serves both roles, there's exactly one connected Application at a time,
     * exactly one place that should track it. Taxonomy/Retrieval/Archiving all take a
     * ConnectionViewModel reference in their constructors and read CurrentApplication
     * when performing operations, they never call Connect()/Disconnect() themselves.
     *
     * Two ways to reconnect are exposed, matching Unity.01.ConnectingToOnBase's two
     * actual entry points: the main Connect button uses whatever's configured in
     * Settings (ServiceLocation.SessionId first if set, falling back to
     * AuthenticationMode), while "Reconnect to Session ID" exercises
     * Connect(this string sessionId) directly, an ad hoc session ID independent of
     * whatever Settings currently holds, useful for testing that entry point on its own.
     * Reconnecting to a session ID is available regardless of whether this harness
     * instance already happens to be connected to something else, the real constraint on
     * whether it SUCCEEDS is whether the TARGET session was itself kept alive
     * (IsDisconnectEnabled false), not the harness's own current connection state.
     *
     * KeepAlive here reads/writes SessionManagement.ServiceLocation.KeepAlive DIRECTLY,
     * the same live instance Connect() will use, not a separate copy, so toggling it here
     * takes effect immediately, no need to visit Settings and click Apply just to flip
     * one flag while testing.
     *
     * TestServerCommand exists specifically for the case Connect() itself doesn't cover:
     * telling apart "the Application Server is unreachable" from "the server is up, but
     * something about my credentials/config is wrong", useful to check BEFORE attempting
     * to connect at all, or immediately after a failed attempt, to narrow down where the
     * problem actually is. It calls Service.asmx's own HttpGet "Ping" operation directly
     * (bypassing the Unity API entirely, this is a plain HTTP call, not
     * SessionManagement), parsing the plain-text "INITIALIZED" response the Application
     * Server returns when it's up.
     */
    #endregion

    /// <summary>
    /// The Connect page's view model, and the shared connection state
    /// (<see cref="CurrentApplication"/>) every other page reads.
    /// </summary>
    public class ConnectionViewModel : ViewModelBase
    {
        #region Private Members
        private readonly LogViewModel log;

        private UnityApplication currentApplication;
        private string sessionId;
        private string currentUserDisplayName;
        private string reconnectSessionIdInput;
        private bool? isServerAvailable;
        private string serverStatusMessage = "Not checked yet.";
        private bool isCheckingServer;
        #endregion

        #region Properties
        /// <summary>
        /// The connected Unity API Application object, or <see langword="null"/> if not
        /// currently connected. Every other page reads this to perform its own operations.
        /// </summary>
        public UnityApplication CurrentApplication
        {
            get => currentApplication;
            private set
            {
                if (!SetField(ref currentApplication, value)) return;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        /// <summary>
        /// Whether <see cref="CurrentApplication"/> is currently connected.
        /// </summary>
        public bool IsConnected => CurrentApplication != null;

        /// <summary>
        /// The connected session's ID, or <see langword="null"/> if not connected.
        /// </summary>
        public string SessionId
        {
            get => sessionId;
            private set => SetField(ref sessionId, value);
        }

        /// <summary>
        /// The connected user's display name, or <see langword="null"/> if not connected.
        /// </summary>
        public string CurrentUserDisplayName
        {
            get => currentUserDisplayName;
            private set => SetField(ref currentUserDisplayName, value);
        }

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        public string ServicePath => SessionManagement.ServiceLocation?.ServicePath;

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        public string DataSource => SessionManagement.ServiceLocation?.DataSource;

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        public AuthenticationMode AuthenticationMode => SessionManagement.ServiceLocation?.AuthenticationMode ?? AuthenticationMode.OnBaseCredentials;

        /// <summary>
        /// Duplicated here (from Settings) for convenience while testing: reads/writes
        /// SessionManagement.ServiceLocation.KeepAlive directly, so toggling it takes
        /// effect immediately without a trip to Settings.
        /// </summary>
        public bool KeepAlive
        {
            get => SessionManagement.ServiceLocation?.KeepAlive ?? false;
            set
            {
                if (SessionManagement.ServiceLocation == null || SessionManagement.ServiceLocation.KeepAlive == value) return;
                SessionManagement.ServiceLocation.KeepAlive = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The Session ID to reconnect to, used by <see cref="ReconnectBySessionIdCommand"/>.
        /// </summary>
        public string ReconnectSessionIdInput
        {
            get => reconnectSessionIdInput;
            set => SetField(ref reconnectSessionIdInput, value);
        }

        /// <summary>
        /// Whether the Application Server was reachable the last time it was checked, or
        /// <see langword="null"/> if it hasn't been checked yet this run.
        /// </summary>
        public bool? IsServerAvailable
        {
            get => isServerAvailable;
            private set => SetField(ref isServerAvailable, value);
        }

        /// <summary>
        /// A short human-readable description of the last <see cref="IsServerAvailable"/> check.
        /// </summary>
        public string ServerStatusMessage
        {
            get => serverStatusMessage;
            private set => SetField(ref serverStatusMessage, value);
        }

        /// <summary>
        /// Whether a server availability check is currently in progress.
        /// </summary>
        public bool IsCheckingServer
        {
            get => isCheckingServer;
            private set => SetField(ref isCheckingServer, value);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Connects using whatever's currently configured in Settings.
        /// </summary>
        public RelayCommand ConnectCommand { get; }

        /// <summary>
        /// Disconnects <see cref="CurrentApplication"/>.
        /// </summary>
        public RelayCommand DisconnectCommand { get; }

        /// <summary>
        /// Reconnects to <see cref="ReconnectSessionIdInput"/> directly, independent of
        /// whatever Settings currently holds.
        /// </summary>
        public RelayCommand ReconnectBySessionIdCommand { get; }

        /// <summary>
        /// Refreshes <see cref="ServicePath"/>/<see cref="DataSource"/>/<see cref="AuthenticationMode"/>/
        /// <see cref="KeepAlive"/> from whatever's currently configured (e.g., after editing Settings).
        /// </summary>
        public RelayCommand RefreshSummaryCommand { get; }

        /// <summary>
        /// Calls the Application Server's Ping operation directly to check whether it's
        /// reachable, independent of (and without requiring) an OnBase session.
        /// </summary>
        public AsyncRelayCommand TestServerCommand { get; }

        /// <summary>
        /// Copies <see cref="SessionId"/> to the clipboard.
        /// </summary>
        public RelayCommand CopySessionIdCommand { get; }

        /// <summary>
        /// Pastes the clipboard's current text into <see cref="ReconnectSessionIdInput"/>.
        /// </summary>
        public RelayCommand PasteSessionIdCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the ConnectionViewModel class
        /// </summary>
        /// <param name="log">The shared output log.</param>
        public ConnectionViewModel(LogViewModel log)
        {
            this.log = log;

            ConnectCommand = new RelayCommand(_ => Connect(), _ => !IsConnected);
            DisconnectCommand = new RelayCommand(_ => Disconnect(), _ => IsConnected);
            ReconnectBySessionIdCommand = new RelayCommand(_ => ReconnectBySessionId(),
                _ => !string.IsNullOrEmpty(ReconnectSessionIdInput));
            RefreshSummaryCommand = new RelayCommand(_ => RefreshSummary());
            TestServerCommand = new AsyncRelayCommand(_ => TestServer(), _ => !IsCheckingServer);
            CopySessionIdCommand = new RelayCommand(_ => CopySessionId(), _ => !string.IsNullOrEmpty(SessionId));
            PasteSessionIdCommand = new RelayCommand(_ => PasteSessionId());
        }
        #endregion

        #region Private Methods
        // Connect using whatever's currently configured
        private void Connect()
        {
            try
            {
                CurrentApplication = SessionManagement.Connect();
                UpdateConnectedState();
                log.Success($"Connected (Session ID: {SessionId}, User: {CurrentUserDisplayName}).");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Reconnect to an ad hoc session ID, independent of Settings
        private void ReconnectBySessionId()
        {
            try
            {
                CurrentApplication = ReconnectSessionIdInput.Connect();
                UpdateConnectedState();
                log.Success($"Reconnected (Session ID: {SessionId}, User: {CurrentUserDisplayName}).");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Disconnect
        private void Disconnect()
        {
            try
            {
                CurrentApplication.Disconnect();
                CurrentApplication = null;
                SessionId = null;
                CurrentUserDisplayName = null;
                log.Success("Disconnected.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Re-read the settings summary properties (they're computed from
        // SessionManagement.ServiceLocation, which Settings may have just changed)
        private void RefreshSummary()
        {
            OnPropertyChanged(nameof(ServicePath));
            OnPropertyChanged(nameof(DataSource));
            OnPropertyChanged(nameof(AuthenticationMode));
            OnPropertyChanged(nameof(KeepAlive));
        }

        // Refresh SessionId/CurrentUserDisplayName from CurrentApplication after connecting
        private void UpdateConnectedState()
        {
            SessionId = CurrentApplication?.SessionID;
            CurrentUserDisplayName = CurrentApplication?.CurrentUser?.DisplayName;
        }

        // Copy the current SessionId to the clipboard
        private void CopySessionId()
        {
            try
            {
                Clipboard.SetText(SessionId);
                log.Info($"Copied Session ID [{SessionId}] to clipboard.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Paste the clipboard's current text into ReconnectSessionIdInput
        private void PasteSessionId()
        {
            try
            {
                if (!Clipboard.ContainsText())
                {
                    log.Error("Clipboard does not contain text.");
                    return;
                }

                ReconnectSessionIdInput = Clipboard.GetText().Trim();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Call Service.asmx's own HttpGet "Ping" operation directly, entirely independent
        // of the Unity API/an OnBase session, to check whether the Application Server
        // itself is reachable
        private async Task TestServer()
        {
            var servicePath = ServicePath;

            if (string.IsNullOrEmpty(servicePath))
            {
                IsServerAvailable = false;
                ServerStatusMessage = "No Service Path configured.";
                log.Error("Cannot test server: no Service Path configured.");
                return;
            }

            IsCheckingServer = true;

            try
            {
                var pingUrl = $"{servicePath}/Ping";

                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(pingUrl);
                    var value = XDocument.Parse(response).Root?.Value;

                    if (string.Equals(value, "INITIALIZED", StringComparison.OrdinalIgnoreCase))
                    {
                        IsServerAvailable = true;
                        ServerStatusMessage = $"Server available ({value}).";
                        log.Success($"Ping succeeded at [{pingUrl}]: {value}");
                    }
                    else
                    {
                        IsServerAvailable = false;
                        ServerStatusMessage = $"Server responded, but not initialized: {value}";
                        log.Error($"Ping at [{pingUrl}] returned unexpected value: {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                IsServerAvailable = false;
                ServerStatusMessage = "Server unreachable.";
                log.Error(ex);
            }
            finally
            {
                IsCheckingServer = false;
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
