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
     * *Correction*: RestApi.01's SessionManagement was originally static/process-wide,
     * matching RestApi.00's own "admin-style, shared" documentation of Settings. That was
     * wrong for RestApi.TestHarness.Web (every web user needs their own IdP token/
     * session, not a shared one), so SessionManagement was converted to an ordinary
     * instance class. This app is still single-user (a desktop app), so nothing about
     * ITS OWN behavior changes, one SessionManagement instance, owned here (Session),
     * lives for the app's whole lifetime, same as the old static class effectively did.
     * Every other view model that needs an HttpClient now goes through
     * connection.Session.GetHttpClient() explicitly, rather than relying on RestApi.02-04's
     * own helper classes silently falling back to a static default (that fallback no
     * longer exists, see those projects' own Training Notes).
     *
     * - No CurrentApplication: there's no single object to hold, IsConnected here is a
     *   thin, explicitly-notified wrapper around Session.IsConnected, which is itself
     *   the actual source of truth.
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
     *   Session.ConnectAsync()/DisconnectAsync() are genuinely async (unlike Unity API's
     *   synchronous Application.Connect()/.Disconnect()), so these commands need to
     *   actually await real I/O, not just delegate to a synchronous call. This has a real
     *   ripple effect elsewhere: Unity.TestHarness's own TaxonomyViewModel (and others)
     *   call connection.ConnectCommand.Execute(null) synchronously, then immediately
     *   check connection.IsConnected, relying on Execute() having already finished. That
     *   pattern breaks here (AsyncRelayCommand.Execute() is fire-and-forget from the
     *   caller's perspective, IsConnected may still read false right after it returns),
     *   so ConnectAsync()/DisconnectAsync() below are exposed as ordinary, awaitable
     *   public methods specifically so other view models can await the real operation
     *   directly, instead of going through the ICommand indirection.
     */
    #endregion

    /// <summary>
    /// The Connect page's view model, and the shared connection state every other page
    /// checks (via <see cref="IsConnected"/>) before performing its own operations.
    /// </summary>
    public class ConnectionViewModel : ViewModelBase, IDisposable
    {
        #region Private Members
        private readonly LogViewModel log;
        private bool disposed;
        #endregion

        #region Properties
        /// <summary>
        /// This app's one SessionManagement instance, owned here, for the app's whole
        /// lifetime (this is a single-user desktop app, unlike RestApi.TestHarness.Web,
        /// which needs one instance PER USER, see this class's own Training Notes).
        /// Every other view model that needs a connected HttpClient goes through
        /// <c>connection.Session.GetHttpClient()</c>/<c>GetFormsHttpClient()</c> explicitly.
        /// </summary>
        public SessionManagement Session { get; } = new();

        /// <summary>
        /// Whether a session is currently established. A thin, explicitly-notified
        /// wrapper around <see cref="Session"/>'s own <see cref="SessionManagement.IsConnected"/>,
        /// which is itself the actual source of truth (see this class's own Training Notes).
        /// </summary>
        public bool IsConnected => Session.IsConnected;

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        public string ApiServerUrl => Session.ServiceLocation?.ApiServerUrl;

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        public string FormsApiUrl => Session.ServiceLocation?.FormsApiUrl;

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        public AuthenticationMode AuthenticationMode => Session.ServiceLocation?.AuthenticationMode ?? AuthenticationMode.OnBaseCredentials;

        /// <summary>
        /// Duplicated here (from Settings) for convenience while testing: reads/writes
        /// Session.ServiceLocation.KeepAlive directly, so toggling it takes effect
        /// immediately without a trip to Settings.
        /// </summary>
        public bool KeepAlive
        {
            get => Session.ServiceLocation?.KeepAlive ?? false;
            set
            {
                if (Session.ServiceLocation == null || Session.ServiceLocation.KeepAlive == value) return;
                Session.ServiceLocation.KeepAlive = value;
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
        /// Returns the connected, fully-configured HttpClient every other view model
        /// needs to make Document Management API calls. A thin convenience wrapper
        /// around <c>Session.GetHttpClient()</c>.
        /// </summary>
        /// <returns>The connected HttpClient.</returns>
        public HttpClient GetHttpClient() => Session.GetHttpClient();

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

        /// <inheritdoc />
        public void Dispose()
        {
            if (disposed) return;
            Session.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Methods
        // Connect using whatever's currently configured
        private async Task Connect()
        {
            try
            {
                await Session.ConnectAsync();
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
                await Session.DisconnectAsync();
                OnPropertyChanged(nameof(IsConnected));
                log.Success("Disconnected.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Re-read the settings summary properties (they're computed from
        // Session.ServiceLocation, which Settings may have just changed)
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
