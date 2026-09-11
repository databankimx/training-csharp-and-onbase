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
using System.Threading.Tasks;
using RestApi._00.CommonFunctionality.Models.Enumerations;
using RestApi._01.ConnectingToOnBase.HelperClasses.OnBase;
#endregion

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own
     * ConnectionViewModel, but genuinely reshaped, not a like-for-like port, since the
     * underlying session model is fundamentally different (see RestApi.01.ConnectingToOnBase's
     * own LectureNotes.md for the full picture):
     *
     * - No CurrentApplication: there's no single object to hold, RestApi.01's
     *   SessionManagement is itself static/process-wide, the same "admin-style, shared
     *   across every user" design already confirmed for RestApi.00's ServiceLocation.
     *   IsConnected here is a thin, explicitly-notified wrapper around
     *   SessionManagement.IsConnected, not a locally-tracked field, SessionManagement
     *   itself is the actual source of truth.
     *
     * - No SessionId/CurrentUserDisplayName, no "Reconnect to Session ID", no
     *   Copy/PasteSessionId: all dropped, a confirmed gap. The REST API's session is
     *   carried by an opaque cookie (see RestApi.00's own ServiceLocation Training
     *   Notes), not a client-visible session identifier the way Unity API's SessionID
     *   is, and there's no documented "who am I" endpoint to source a display name from
     *   either.
     *
     * - No TestServerCommand ("Test API Availability"): Unity's version calls
     *   Service.asmx's own Ping operation directly, bypassing the Unity API entirely.
     *   No equivalent unauthenticated health-check endpoint was found documented
     *   anywhere in document-api.json's own paths. Rather than fabricate one, this
     *   command is omitted; ConnectCommand itself is the only way to find out whether
     *   the server/credentials are actually working.
     *
     * - ConnectCommand/DisconnectCommand are AsyncRelayCommand, not RelayCommand:
     *   SessionManagement.ConnectAsync()/DisconnectAsync() are genuinely async (unlike
     *   Unity API's synchronous Application.Connect()/.Disconnect()), so these commands
     *   need to actually await real I/O, not just delegate to a synchronous call. This
     *   has a real ripple effect elsewhere: Unity.TestHarness's own TaxonomyViewModel
     *   (and others) call connection.ConnectCommand.Execute(null) synchronously, then
     *   immediately check connection.IsConnected, relying on Execute() having already
     *   finished. That pattern breaks here (AsyncRelayCommand.Execute() is fire-and-forget
     *   from the caller's perspective, IsConnected may still read false right after it
     *   returns), so ConnectAsync()/DisconnectAsync() below are exposed as ordinary,
     *   awaitable public methods specifically so other view models can await the real
     *   operation directly, instead of going through the ICommand indirection.
     */
    #endregion

    /// <summary>
    /// The Connect page's view model, and the shared connection state every other page
    /// checks (via <see cref="IsConnected"/>) before performing its own operations.
    /// </summary>
    public class ConnectionViewModel : ViewModelBase
    {
        #region Private Members
        private readonly LogViewModel log;
        #endregion

        #region Properties
        /// <summary>
        /// Whether a session is currently established. A thin, explicitly-notified
        /// wrapper around <see cref="SessionManagement.IsConnected"/>, which is itself
        /// the actual source of truth (see this class's own Training Notes).
        /// </summary>
        public bool IsConnected => SessionManagement.IsConnected;

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        public static string ApiServerUrl => SessionManagement.ServiceLocation?.ApiServerUrl;

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        public static string FormsApiUrl => SessionManagement.ServiceLocation?.FormsApiUrl;

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        public static AuthenticationMode AuthenticationMode => SessionManagement.ServiceLocation?.AuthenticationMode ?? AuthenticationMode.OnBaseCredentials;

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
        #endregion

        #region Commands
        /// <summary>
        /// Connects using whatever's currently configured in Settings.
        /// </summary>
        public AsyncRelayCommand ConnectCommand { get; }

        /// <summary>
        /// Disconnects the current session.
        /// </summary>
        public AsyncRelayCommand DisconnectCommand { get; }

        /// <summary>
        /// Refreshes <see cref="ApiServerUrl"/>/<see cref="FormsApiUrl"/>/<see cref="AuthenticationMode"/>/
        /// <see cref="KeepAlive"/> from whatever's currently configured (e.g., after editing Settings).
        /// </summary>
        public RelayCommand RefreshSummaryCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the ConnectionViewModel class
        /// </summary>
        /// <param name="log">The shared output log.</param>
        public ConnectionViewModel(LogViewModel log)
        {
            this.log = log;

            ConnectCommand = new AsyncRelayCommand(_ => Connect(), _ => !IsConnected);
            DisconnectCommand = new AsyncRelayCommand(_ => Disconnect(), _ => IsConnected);
            RefreshSummaryCommand = new RelayCommand(_ => RefreshSummary());
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connects using whatever's currently configured in Settings. Exposed as an
        /// ordinary awaitable method (not just via <see cref="ConnectCommand"/>) so
        /// other view models can await the real connect operation directly before
        /// proceeding, see this class's own Training Notes for why that matters here.
        /// </summary>
        public Task ConnectAsync() => Connect();

        /// <summary>
        /// Disconnects the current session, if any, without throwing (errors are logged,
        /// not propagated). Used by MainWindow's own Closing handler, where a disconnect
        /// failure should never block the window from actually closing.
        /// </summary>
        public async Task DisconnectQuietlyAsync()
        {
            try
            {
                if (IsConnected) await Disconnect();
            }
            catch (Exception)
            {
                // Swallowed deliberately, see method summary above; Disconnect() itself
                // already logs the underlying error.
            }
        }
        #endregion

        #region Private Methods
        // Connect using whatever's currently configured
        private async Task Connect()
        {
            try
            {
                await SessionManagement.ConnectAsync();
                OnPropertyChanged(nameof(IsConnected));
                log.Success("Connected.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Disconnect
        private async Task Disconnect()
        {
            try
            {
                await SessionManagement.DisconnectAsync();
                OnPropertyChanged(nameof(IsConnected));
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
            OnPropertyChanged(nameof(ApiServerUrl));
            OnPropertyChanged(nameof(FormsApiUrl));
            OnPropertyChanged(nameof(AuthenticationMode));
            OnPropertyChanged(nameof(KeepAlive));
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
